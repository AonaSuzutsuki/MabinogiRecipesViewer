using CookInformationViewer.Models.Db.Context;
using SavannahXmlLib.XmlWrapper;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonExtensionLib.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using UpdateLib.Http;
using UpdateLib.Update;
using KimamaSqlExecutorLib.Db.Raw;
using KimamaSqliteExecutorLib.Db.Raw;

namespace CookInformationViewer.Models.Db.Manager
{
    public class UpdateEventArgs : EventArgs
    {
        public bool Completed { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long Percentage { get; set; }
    }

    public class UpdateContextManager : IDisposable
    {
        private const string VersionColumnName = "db_version";

        public class FavoriteTuple
        {
            public int Id { get; set; }
            public int NewId { get; set; }
            public string RecipeName { get; set; }

            public FavoriteTuple(int id, int newId, string recipeName)
            {
                Id = id;
                NewId = newId;
                RecipeName = recipeName;
            }
        }

        private ContextManager _contextManager = new();

        public SavannahXmlReader? VersionReader { get; private set; }
        public UpdateClient DownloadClient { get; }
        public Dictionary<string, Tuple<string, bool>> AvailableTableUpdate { get; } = new();

        public bool AvailableUpdate
        {
            get
            {
                return AvailableTableUpdate.Values.Any(x => x.Item2);
            }
        }

        #region Events

        private readonly Subject<UpdateEventArgs> _progressChangedSubject = new();
        public IObservable<UpdateEventArgs> ProgressChanged => _progressChangedSubject;

        #endregion

        private static Dictionary<string, string> TargetFiles => new()
        {
            { "Materials", "Materials.json"},
            { "Locations", "Locations.json"},
            { "Categories", "Categories.json"},
            { "Sellers", "Sellers.json"},
            { "MaterialDrops", "MaterialDrops.json"},
            { "MaterialSellers", "MaterialSellers.json"},
            { "Recipes", "Recipes.json"},
            { "Additionals", "Additionals.json"},
            { "Effects", "Effects.json"}
        };

        public UpdateContextManager()
        {
            var webClient = new UpdateWebClient
            {
                BaseUrl = "https://mabicook.aonsztk.xyz/Export/"
            };

#if DEBUG
            if (File.Exists(Constants.UpdateUrlFile))
            {
                var reader = new SavannahXmlReader(Constants.UpdateUrlFile);
                var baseUrl = reader.GetValue("updates/update[@name='data']") ?? webClient.BaseUrl;
                webClient.BaseUrl = baseUrl;
            }
#endif

            DownloadClient = new UpdateClient(webClient);

            DownloadClient.DownloadStarted += (sender, args) =>
            {
                _progressChangedSubject.OnNext(new UpdateEventArgs
                {
                    FileName = args.FilePath,
                    Percentage = 0,
                });
            };
            DownloadClient.DownloadProgressChanged += (sender, args) =>
            {
                if (args.FilePath is "images.zip" or "Recipes.json")
                {
                    _progressChangedSubject.OnNext(new UpdateEventArgs
                    {
                        FileName = "Recipes.json",
                        Percentage = args.FilePath == "images.zip" ? 25 + args.Percentage / 2 / 2 : args.Percentage / 2 / 2,
                    });
                }
                else
                {
                    _progressChangedSubject.OnNext(new UpdateEventArgs
                    {
                        FileName = args.FilePath,
                        Percentage = args.Percentage / 2,
                    });
                }
            };
            DownloadClient.DownloadCompleted += (sender, args) =>
            {
                _progressChangedSubject.OnNext(new UpdateEventArgs
                {
                    FileName = args.FilePath,
                    Percentage = 50,
                });
            };
        }

        public async Task AvailableTableUpdateCheck()
        {
            var versionData = await DownloadClient.DownloadFileAsync("version.xml");
            using var versionStream = new MemoryStream(versionData);
            VersionReader = new SavannahXmlReader(versionStream);

            foreach (var item in TargetFiles)
            {
                var fileName = item.Value;

                AvailableTableUpdate.Add(fileName, new Tuple<string, bool>(string.Empty, false));

                var versionNode = VersionReader.GetNode($"/versions/version[@id='{fileName}']");
                if (versionNode == null)
                    continue;

                var version = versionNode.InnerText;

                var history = _contextManager.GetItem(context => context.Histories.FirstOrDefault(x => x.Name == fileName));
                if (history == null || history.Date != version)
                    AvailableTableUpdate[fileName] = new Tuple<string, bool>(version, true);
                else
                    AvailableTableUpdate[fileName] = new Tuple<string, bool>(version, false);
            }
        }

        public async Task RebuildDataBase()
        {
            _contextManager.RebuildContext(false);
            _contextManager.Dispose();
            _contextManager = new ContextManager();

            await Task.Factory.StartNew(() =>
            {
                _contextManager.ApplyNonSave(context =>
                {
                    var favoriteTable = (from x in context.Favorites
                        join rec in context.CookRecipes on x.RecipeId equals rec.Id
                        where !x.IsDelete
                        select new FavoriteTuple(x.Id, x.RecipeId, rec.Name)).ToList();

                    RebuildItems(context, DownloadClient, context.CookMaterials, TargetFiles["Materials"]);
                    RebuildItems(context, DownloadClient, context.CookLocations, TargetFiles["Locations"]);
                    RebuildItems(context, DownloadClient, context.CookCategories, TargetFiles["Categories"]);
                    RebuildItems(context, DownloadClient, context.CookSellers, TargetFiles["Sellers"]);
                    RebuildItems(context, DownloadClient, context.CookMaterialDrops, TargetFiles["MaterialDrops"]);
                    RebuildItems(context, DownloadClient, context.CookMaterialSellers, TargetFiles["MaterialSellers"]);
                    RebuildRecipes(context, DownloadClient, context.CookRecipes, TargetFiles["Recipes"]);
                    RebuildItems(context, DownloadClient, context.Additionals, TargetFiles["Additionals"]);
                    RebuildItems(context, DownloadClient, context.CookEffects, TargetFiles["Effects"]);

                    RebuildFavorite(favoriteTable, context);
                    RebuildMemos(favoriteTable, context);

                    UpdateDbVersion(context);
                });
            });
        }

        public async Task UpdateDataBase(Dictionary<string, bool> targetFileName)
        {
            if (IsUpdateDbVersion())
            {
                await RebuildDataBase();
                return;
            }

            await Task.Factory.StartNew(() =>
            {
                _contextManager.ApplyNonSave(context =>
                {
                    var favoriteTable = (from x in context.Favorites
                        join rec in context.CookRecipes on x.RecipeId equals rec.Id
                        where !x.IsDelete
                        select new FavoriteTuple(x.Id, x.RecipeId, rec.Name)).ToList();

                    RebuildItems(context, DownloadClient, context.CookMaterials, TargetFiles["Materials"], targetFileName.Get("Materials"));
                    RebuildItems(context, DownloadClient, context.CookLocations, TargetFiles["Locations"], targetFileName.Get("Locations"));
                    RebuildItems(context, DownloadClient, context.CookCategories, TargetFiles["Categories"], targetFileName.Get("Categories"));
                    RebuildItems(context, DownloadClient, context.CookSellers, TargetFiles["Sellers"], targetFileName.Get("Sellers"));
                    RebuildItems(context, DownloadClient, context.CookMaterialDrops, TargetFiles["MaterialDrops"], targetFileName.Get("MaterialDrops"));
                    RebuildItems(context, DownloadClient, context.CookMaterialSellers, TargetFiles["MaterialSellers"], targetFileName.Get("MaterialSellers"));
                    RebuildRecipes(context, DownloadClient, context.CookRecipes, TargetFiles["Recipes"], targetFileName.Get("Recipes"));
                    RebuildItems(context, DownloadClient, context.Additionals, TargetFiles["Additionals"], targetFileName.Get("Additionals"));
                    RebuildItems(context, DownloadClient, context.CookEffects, TargetFiles["Effects"], targetFileName.Get("Effects"));

                    RebuildFavorite(favoriteTable, context);
                    RebuildMemos(favoriteTable, context);

                    UpdateDbVersion(context);
                });
            });
        }

        public Dictionary<string, string> CurrentTableVersion()
        {
            return _contextManager.GetItem(context => context.Histories.ToDictionary(x => x.Name, x => x.Date));
        }

        private void RebuildItems<T>(CookInfoContext context, UpdateClient downloadClient, DbSet<T> db, string fileName, bool canUpdate = true) where T : class
        {
            if (!canUpdate)
                return;

            if (!AvailableTableUpdate.ContainsKey(fileName))
                return;

            var materialsData = downloadClient.DownloadFile(fileName);
            var materialsJson = Encoding.UTF8.GetString(materialsData);
            var materials = JsonConvert.DeserializeObject<List<T>>(materialsJson);
            if (materials != null)
            {
                var length = db.Count() + materials.Count;
                var count = 1;
                foreach (var item in db)
                {
                    db.Remove(item);

                    _progressChangedSubject.OnNext(new UpdateEventArgs
                    {
                        FileName = fileName,
                        Percentage = 50 + (long)Math.Round(((double)count / length * 100) / 2),
                    });

                    count++;
                }

                context.SaveChanges();

                foreach (var material in materials)
                {
                    db.Add(material);

                    _progressChangedSubject.OnNext(new UpdateEventArgs
                    {
                        FileName = fileName,
                        Percentage = 50 + (long)Math.Round(((double)count / length * 100) / 2),
                    });

                    count++;
                }
            }

            var version = AvailableTableUpdate[fileName].Item1;
            var history = context.Histories.FirstOrDefault(x => x.Name == fileName);

            if (history == null)
            {
                context.Histories.Add(new DbDownloadHistory
                {
                    Name = fileName,
                    Date = version,
                    CreateDate = DateTime.Now
                });
            }
            else
            {
                history.Date = version;
                history.UpdateDate = DateTime.Now;
            }

            context.SaveChanges();

            _progressChangedSubject.OnNext(new UpdateEventArgs
            {
                FileName = fileName,
                Percentage = 100,
                Completed = true
            });
        }

        private void RebuildRecipes(CookInfoContext context, UpdateClient downloadClient, DbSet<DbCookRecipes> db, string fileName, bool canUpdate = true)
        {
            if (!canUpdate)
                return;
            
            if (!AvailableTableUpdate.ContainsKey(fileName))
                return;

            var materialsData = downloadClient.DownloadFile(fileName);
            var materialsJson = Encoding.UTF8.GetString(materialsData);
            var materials = JsonConvert.DeserializeObject<List<DbCookRecipes>>(materialsJson);
            var imagesData = downloadClient.DownloadFile("images.zip");

            using var ms = new MemoryStream(imagesData);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            
            if (materials != null)
            {
                var length = db.Count() + materials.Count;
                var count = 1;
                foreach (var item in db)
                {
                    db.Remove(item);

                    _progressChangedSubject.OnNext(new UpdateEventArgs
                    {
                        FileName = fileName,
                        Percentage = 50 + (long)Math.Round(((double)count / length * 100) / 2),
                    });

                    count++;
                }

                context.SaveChanges();

                foreach (var material in materials)
                {
                    if (material.ImagePath != null)
                    {
                        var entry = archive.GetEntry(material.ImagePath);
                        if (entry != null)
                        {
                            using var entryStream = entry.Open();
                            using var entryMemory = new MemoryStream();
                            entryStream.CopyTo(entryMemory);
                            entryMemory.Position = 0;

                            var imageData = new byte[entry.Length];
                            _ = entryMemory.Read(imageData);

                            material.ImageData = imageData;
                        }
                    }

                    context.Add(material);

                    _progressChangedSubject.OnNext(new UpdateEventArgs
                    {
                        FileName = fileName,
                        Percentage = 50 + (long)Math.Round(((double)count / length * 100) / 2),
                    });

                    count++;
                }
            }

            var version = AvailableTableUpdate[fileName].Item1;
            var history = context.Histories.FirstOrDefault(x => x.Name == fileName);

            if (history == null)
            {
                context.Histories.Add(new DbDownloadHistory
                {
                    Name = fileName,
                    Date = version,
                    CreateDate = DateTime.Now
                });
            }
            else
            {
                history.Date = version;
                history.UpdateDate = DateTime.Now;
            }

            context.SaveChanges();

            _progressChangedSubject.OnNext(new UpdateEventArgs
            {
                FileName = fileName,
                Percentage = 100,
                Completed = true
            });
        }

        private static void UpdateDbVersion(CookInfoContext context)
        {
            var dbVersion = context.Metas.FirstOrDefault(x => x.Id == VersionColumnName);
            if (dbVersion == null)
            {
                context.Metas.Add(new DbMeta
                {
                    Id = VersionColumnName,
                    Value = TableColumns.DbVersion
                });
            }
            else
            {
                dbVersion.Value = TableColumns.DbVersion;
            }

            context.SaveChanges();
        }

        private bool IsUpdateDbVersion()
        {
            var dbVersion = _contextManager.GetItem(context => context.Metas.FirstOrDefault(x => x.Id == VersionColumnName));
            var version = dbVersion?.Value ?? "1.0";

            return version != TableColumns.DbVersion;
        }

        private static void RebuildFavorite(List<FavoriteTuple> favoriteTable, CookInfoContext context)
        {
            var newIdItems = (from x in favoriteTable
                join rec in context.CookRecipes on x.RecipeName equals rec.Name
                select new
                {
                    Recipe = rec,
                    FavoriteTable = x
                }).ToList();

            foreach (var newIdItem in newIdItems)
            {
                var favorite = context.Favorites.FirstOrDefault(x => x.Id == newIdItem.FavoriteTable.Id);
                if (favorite == null)
                    continue;

                favorite.RecipeId = newIdItem.Recipe.Id;
                favorite.UpdateDate = DateTime.Now;
            }

            context.SaveChanges();
        }

        private static void RebuildMemos(List<FavoriteTuple> favoriteTable, CookInfoContext context)
        {
            var newIdItems = (from x in favoriteTable
                join rec in context.CookRecipes on x.RecipeName equals rec.Name
                select new
                {
                    Recipe = rec,
                    FavoriteTable = x
                }).ToList();

            foreach (var newIdItem in newIdItems)
            {
                var memo = context.Memos.FirstOrDefault(x => x.Id == newIdItem.FavoriteTable.Id);
                if (memo == null)
                    continue;

                memo.RecipeId = newIdItem.Recipe.Id;
                memo.UpdateDate = DateTime.Now;
            }

            context.SaveChanges();
        }

        public void Dispose()
        {
            _contextManager.Dispose();
            _progressChangedSubject.Dispose();

            GC.SuppressFinalize(this);
        }
    }

    public class ContextManager : IDisposable
    {
        protected readonly SemaphoreSlim Semaphore = new(1, 1);

        private readonly CookInfoContext _context;
        protected readonly SqlExecutor Executor;
        
        public ContextManager()
        {
            _context = new CookInfoContext();
            Executor = new SqlExecutor(Constants.DatabaseFileName);
#if DEBUG
            Executor.SqlExecutedObservable.Subscribe(x => Debug.WriteLine(x));
#endif

            RebuildContext(true);
        }

        public void RebuildContext(bool isInit)
        {
            var tables = TableColumns.GetTables();
            
            if (!isInit)
            {
                Executor.ExecuteNonQuery(SqlCreator.Create("PRAGMA foreign_keys = 0"));
                var targetTables = tables.Select(x => x.Value).Where(x =>
                    !TableColumns.IsSystemTable(x.TableName));
                foreach (var table in targetTables)
                {
                    Executor.ExecuteNonQuery(SqlCreator.Create($"DROP TABLE {table.TableName}"));
                }
                Executor.ExecuteNonQuery(SqlCreator.Create("PRAGMA foreign_keys = 1"));
            }

            var realTables = Executor.ExistsObjects("table", tables.Values.Select(x => x.TableName).ToArray());
            var notExistsTables = tables.Select(x => x.Value.TableName).Except(realTables);
            foreach (var notExistsTable in notExistsTables)
            {
                if (!tables.ContainsKey(notExistsTable))
                    return;

                var tableInfo = tables[notExistsTable];
                Executor.CreateTable(tableInfo);
            }

            // Create a column that does not exist.
            foreach (var tableInfo in TableColumns.GetTables())
            {
                var definitionColumns = tableInfo.Value.Columns.ToDictionary(x => x.ColumnName);
                var rawColumns = Executor.Execute(SqlCreator.Create($"PRAGMA table_info('{tableInfo.Value.TableName}')"))
                    .Select(x => x["name"].ToString());

                var except = definitionColumns.Keys.Except(rawColumns).ToList();

                if (except.Any())
                {
                    foreach (var columnName in except)
                    {
                        if (!definitionColumns.ContainsKey(columnName))
                            continue;

                        var columnInfo = definitionColumns[columnName];
                        Executor.ExecuteNonQuery(SqlCreator.Create($"ALTER TABLE {tableInfo.Value.TableName} ADD COLUMN {columnInfo}"));
                    }
                }
            }

            _context.SaveChanges();
        }

        public IList<DbCookCategories> GetCategories(Func<CookInfoContext, IEnumerable<DbCookCategories>>? whereFunc = null)
        {
            return LockFunction(context => whereFunc != null ? whereFunc.Invoke(context).ToList() : context.CookCategories.ToList());
        }

        public T GetItem<T>(Func<CookInfoContext, T> whereFunc)
        {
            return LockFunction(whereFunc.Invoke);
        }

        public void Apply(Action<CookInfoContext> whereAction)
        {
            LockAction(context =>
            {
                whereAction.Invoke(context);
                context.SaveChanges();
            });
        }

        public void ApplyNonSave(Action<CookInfoContext> whereAction)
        {
            LockAction(whereAction.Invoke);
        }

        protected T LockFunction<T>(Func<CookInfoContext, T> whereFunc)
        {
            Semaphore.Wait();

            try
            {
                return whereFunc.Invoke(_context);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        protected void LockAction(Action<CookInfoContext> whereAction)
        {
            Semaphore.Wait();

            try
            {
                whereAction.Invoke(_context);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public void ExecuteNonQuery(SqlCreator creator, params SelectParameter[] values)
        {
            Executor.ExecuteNonQuery(creator, values);
        }

        public IEnumerable<Dictionary<string, SelectValue>> Execute(SqlCreator creator, params SelectParameter[] values)
        {
            return Executor.Execute(creator, values);
        }

        public void Dispose()
        {
            _context.Dispose();
            Executor.Dispose();
            Semaphore.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
