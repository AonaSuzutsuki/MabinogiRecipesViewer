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

    public class UpdateContextManager : ContextManager
    {
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

        public Dictionary<string, string> TargetFiles => new()
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

                var history = LockFunction(context => context.Histories.FirstOrDefault(x => x.Name == fileName));
                if (history == null || history.Date != version)
                    AvailableTableUpdate[fileName] = new Tuple<string, bool>(version, true);
                else
                    AvailableTableUpdate[fileName] = new Tuple<string, bool>(version, false);
            }
        }

        public async Task RebuildDataBase()
        {
            await Task.Factory.StartNew(() =>
            {
                LockAction(context =>
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
                });
            });
        }

        public async Task UpdateDataBase(Dictionary<string, bool> targetFileName)
        {
            await Task.Factory.StartNew(() =>
            {
                LockAction(context =>
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
                });
            });
        }

        public Dictionary<string, string> CurrentTableVersion()
        {
            return LockFunction(context => context.Histories.ToDictionary(x => x.Name, x => x.Date));
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

        public static void RebuildFavorite(List<FavoriteTuple> favoriteTable, CookInfoContext context)
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

    }

    public class ContextManager : IDisposable
    {
        protected readonly CookInfoContext Context;
        protected readonly SemaphoreSlim Semaphore = new(1, 1);

        protected readonly SqlExecutor Executor;
        
        public ContextManager()
        {
            Context = new CookInfoContext();
            Executor = new SqlExecutor(Constants.DatabaseFileName);
#if DEBUG
            Executor.SqlExecutedObservable.Subscribe(x => Debug.WriteLine(x));
#endif

            var tables = TableColumns.GetTables();
            var notExistsTables = Executor.ExistsObjects("table", 
                tables.Values.Select(x => x.TableName).ToArray());
            foreach (var notExistsTable in notExistsTables)
            {
                if (!tables.ContainsKey(notExistsTable))
                    return;

                var tableInfo = tables[notExistsTable];
                Executor.CreateTable(tableInfo);
            }
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
            Context.Dispose();
            Executor.Dispose();
            Semaphore.Dispose();
        }
    }
}
