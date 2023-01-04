using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonStyleLib.Models;
using CoonInformationViewer.Models.Db;
using CoonInformationViewer.Models.Db.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SavannahXmlLib.XmlWrapper;
using UpdateLib.Http;
using UpdateLib.Update;

namespace CoonInformationViewer.Models
{
    public class MainWindowModel : ModelBase
    {
        public static readonly CookInfoContext Context = new();

        private readonly SqlExecutor _executor;
        private readonly UpdateClient _downloadClient;
        private SavannahXmlReader _versionReader;

        public Dictionary<string, Tuple<string, bool>> AvailableTableUpdate { get; private set; } = new();

        private readonly Dictionary<string, string> _targetFiles = new()
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

        public MainWindowModel()
        {
            _executor = new SqlExecutor("test.db");

            var tables = TableColumns.GetTables().Values.ToList();
            foreach (var table in tables)
            {
                if (_executor.ExistsTable(table.TableName))
                    continue;

                _executor.CreateTable(table);
            }

            var webClient = new UpdateWebClient
            {
                BaseUrl = "http://localhost:58415/Export/"
            };
            _downloadClient = new UpdateClient(webClient);

            _ = AvailableTableUpdateCheck();
        }

        public async Task AvailableTableUpdateCheck()
        {
            var versionData = await _downloadClient.DownloadFile("version.xml");
            using var versionStream = new MemoryStream(versionData);
            _versionReader = new SavannahXmlReader(versionStream);

            foreach (var item in _targetFiles)
            {
                var fileName = item.Value;

                AvailableTableUpdate.Add(fileName, new Tuple<string, bool>(string.Empty, false));

                var versionNode = _versionReader.GetNode($"/versions/version[@id='{fileName}']");
                if (versionNode == null)
                    continue;

                var version = versionNode.InnerText;

                var history = Context.Histories.FirstOrDefault(x => x.Name == fileName);
                if (history == null || history.Date != version)
                    AvailableTableUpdate[fileName] = new Tuple<string, bool>(version, true);
            }
        }

        public Dictionary<string, string> CurrentTableVersion()
        {
            return Context.Histories.ToDictionary(x => x.Name, x => x.Date);
        }

        public async Task RebuildDataBase()
        {
            await RebuildItems(_downloadClient, Context.CookMaterials, _targetFiles["Materials"]);
            await RebuildItems(_downloadClient, Context.CookLocations, _targetFiles["Locations"]);
            await RebuildItems(_downloadClient, Context.CookCategories, _targetFiles["Categories"]);
            await RebuildItems(_downloadClient, Context.CookSellers, _targetFiles["Sellers"]);
            await RebuildItems(_downloadClient, Context.CookMaterialDrops, _targetFiles["MaterialDrops"]);
            await RebuildItems(_downloadClient, Context.CookMaterialSellers, _targetFiles["MaterialSellers"]);
            await RebuildRecipes(_downloadClient, Context.CookRecipes, _targetFiles["Recipes"]);
            await RebuildItems(_downloadClient, Context.Additionals, _targetFiles["Additionals"]);
            await RebuildItems(_downloadClient, Context.CookEffects, _targetFiles["Effects"]);
        }

        private async Task RebuildItems<T>(UpdateClient downloadClient, DbSet<T> db, string fileName) where T : class
        {
            if (!AvailableTableUpdate.ContainsKey(fileName))
                return;

            var version = AvailableTableUpdate[fileName].Item1;

            var history = Context.Histories.FirstOrDefault(x => x.Name == fileName);
            if (history != null && history.Date == version)
                return;

            var materialsData = await downloadClient.DownloadFile(fileName);
            var materialsJson = Encoding.UTF8.GetString(materialsData);
            var materials = JsonConvert.DeserializeObject<List<T>>(materialsJson);
            if (materials != null)
            {

                foreach (var item in db)
                {
                    db.Remove(item);
                }

                db.AddRange(materials);
            }

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

            await Context.SaveChangesAsync();
        }

        private async Task RebuildRecipes(UpdateClient downloadClient, DbSet<DbCookRecipes> db, string fileName)
        {
            if (!AvailableTableUpdate.ContainsKey(fileName))
                return;

            var version = AvailableTableUpdate[fileName].Item1;

            var history = Context.Histories.FirstOrDefault(x => x.Name == fileName);
            if (history != null && history.Date == version)
                return;


            var materialsData = await downloadClient.DownloadFile(fileName);
            var materialsJson = Encoding.UTF8.GetString(materialsData);
            var materials = JsonConvert.DeserializeObject<List<DbCookRecipes>>(materialsJson);
            var imagesData = await downloadClient.DownloadFile("images.zip");

            using var ms = new MemoryStream(imagesData);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);

            if (materials != null)
            {
                foreach (var item in db)
                {
                    db.Remove(item);
                }

                foreach (var material in materials)
                {
                    if (material.ImagePath != null)
                    {
                        var entry = archive.GetEntry(material.ImagePath);
                        if (entry != null)
                        {
                            await using var entryStream = entry.Open();
                            var imageData = new byte[entry.Length];
                            _ = entryStream.Read(imageData);

                            material.ImageData = imageData;
                        }
                    }

                    Context.Add(material);
                }
            }

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

            await Context.SaveChangesAsync();
        }
    }
}
