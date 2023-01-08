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
using System.Threading.Tasks;
using CommonExtensionLib.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UpdateLib.Http;
using UpdateLib.Update;

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
                BaseUrl = "http://localhost:58415/Export/"
            };
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

                var history = Context.Histories.FirstOrDefault(x => x.Name == fileName);
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
                lock (DownloadClient)
                    lock (Context)
                    {
                        RebuildItems(DownloadClient, Context.CookMaterials, TargetFiles["Materials"]);
                        RebuildItems(DownloadClient, Context.CookLocations, TargetFiles["Locations"]);
                        RebuildItems(DownloadClient, Context.CookCategories, TargetFiles["Categories"]);
                        RebuildItems(DownloadClient, Context.CookSellers, TargetFiles["Sellers"]);
                        RebuildItems(DownloadClient, Context.CookMaterialDrops, TargetFiles["MaterialDrops"]);
                        RebuildItems(DownloadClient, Context.CookMaterialSellers, TargetFiles["MaterialSellers"]);
                        RebuildRecipes(DownloadClient, Context.CookRecipes, TargetFiles["Recipes"]);
                        RebuildItems(DownloadClient, Context.Additionals, TargetFiles["Additionals"]);
                        RebuildItems(DownloadClient, Context.CookEffects, TargetFiles["Effects"]);
                    }
            });
        }

        public async Task UpdateDataBase(Dictionary<string, bool> targetFileName)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (DownloadClient)
                    lock (Context)
                    {
                        RebuildItems(DownloadClient, Context.CookMaterials, TargetFiles["Materials"], targetFileName.Get("Materials"));
                        RebuildItems(DownloadClient, Context.CookLocations, TargetFiles["Locations"], targetFileName.Get("Locations"));
                        RebuildItems(DownloadClient, Context.CookCategories, TargetFiles["Categories"], targetFileName.Get("Categories"));
                        RebuildItems(DownloadClient, Context.CookSellers, TargetFiles["Sellers"], targetFileName.Get("Sellers"));
                        RebuildItems(DownloadClient, Context.CookMaterialDrops, TargetFiles["MaterialDrops"], targetFileName.Get("MaterialDrops"));
                        RebuildItems(DownloadClient, Context.CookMaterialSellers, TargetFiles["MaterialSellers"], targetFileName.Get("MaterialSellers"));
                        RebuildRecipes(DownloadClient, Context.CookRecipes, TargetFiles["Recipes"], targetFileName.Get("Recipes"));
                        RebuildItems(DownloadClient, Context.Additionals, TargetFiles["Additionals"], targetFileName.Get("Additionals"));
                        RebuildItems(DownloadClient, Context.CookEffects, TargetFiles["Effects"], targetFileName.Get("Effects"));
                    }
            });
        }

        public Dictionary<string, string> CurrentTableVersion()
        {
            lock (Context)
            {
                return Context.Histories.ToDictionary(x => x.Name, x => x.Date);
            }
        }

        private void RebuildItems<T>(UpdateClient downloadClient, DbSet<T> db, string fileName, bool canUpdate = true) where T : class
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
            var history = Context.Histories.FirstOrDefault(x => x.Name == fileName);

            if (history == null)
            {
                Context.Histories.Add(new DbDownloadHistory
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

            Context.SaveChanges();

            _progressChangedSubject.OnNext(new UpdateEventArgs
            {
                FileName = fileName,
                Percentage = 100,
                Completed = true
            });
        }

        private void RebuildRecipes(UpdateClient downloadClient, DbSet<DbCookRecipes> db, string fileName, bool canUpdate = true)
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

                foreach (var material in materials)
                {
                    if (material.ImagePath != null)
                    {
                        var entry = archive.GetEntry(material.ImagePath);
                        if (entry != null)
                        {
                            using var entryStream = entry.Open();
                            var imageData = new byte[entry.Length];
                            _ = entryStream.Read(imageData);

                            material.ImageData = imageData;
                        }
                    }

                    Context.Add(material);

                    _progressChangedSubject.OnNext(new UpdateEventArgs
                    {
                        FileName = fileName,
                        Percentage = 50 + (long)Math.Round(((double)count / length * 100) / 2),
                    });

                    count++;
                }
            }

            var version = AvailableTableUpdate[fileName].Item1;
            var history = Context.Histories.FirstOrDefault(x => x.Name == fileName);

            if (history == null)
            {
                Context.Histories.Add(new DbDownloadHistory
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

            Context.SaveChanges();

            _progressChangedSubject.OnNext(new UpdateEventArgs
            {
                FileName = fileName,
                Percentage = 100,
                Completed = true
            });
        }

    }

    public class ContextManager : IDisposable
    {
        protected readonly CookInfoContext Context = new();
        protected readonly SqlExecutor Executor;
        
        public ContextManager()
        {
            Executor = new SqlExecutor(Constants.DatabaseFileName);

            var tables = TableColumns.GetTables().Values.ToList();
            foreach (var table in tables.Where(table => !Executor.ExistsTable(table.TableName)))
            {
                Executor.CreateTable(table);
            }
        }

        public IList<DbCookCategories> GetCategories(Func<CookInfoContext, IEnumerable<DbCookCategories>>? whereFunc = null)
        {
            lock (Context)
            {
                return whereFunc != null ? whereFunc.Invoke(Context).ToList() : Context.CookCategories.ToList();
            }
        }

        public List<T> GetItems<T>(Func<CookInfoContext, IEnumerable<T>> whereFunc)
        {
            lock (Context)
            {
                return whereFunc.Invoke(Context).ToList();
            }
        }

        public T GetRecipe<T>(Func<CookInfoContext, T> whereFunc)
        {
            lock (Context)
            {
                return whereFunc.Invoke(Context);
            }
        }

        public string? GetRecipeMaterialName(int? id, bool isRecipe)
        {
            lock (Context)
            {
                if (isRecipe)
                {
                    var recipe = Context.CookRecipes.FirstOrDefault(x => x.Id == id);

                    return recipe?.Name;
                }

                var material = Context.CookMaterials.FirstOrDefault(x => x.Id == id);
                return material?.Name;
            }
        }

        public void Dispose()
        {
            Context.Dispose();
            Executor.Dispose();
        }
    }
}
