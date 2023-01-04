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
        private readonly CookInfoContext _context = new();
        private readonly SqlExecutor _executor;

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
        }

        public async Task RebuildDataBase()
        {
            var webClient = new UpdateWebClient
            {
                BaseUrl = "http://localhost:58415/Export/"
            };
            var downloadClient = new UpdateClient(webClient);
            var version = await downloadClient.DownloadFile("version.xml");
            using var versionStream = new MemoryStream(version);
            var reader = new SavannahXmlReader(versionStream);

            await RebuildItems(downloadClient, _context.CookMaterials, "Materials.json", reader);
            await RebuildItems(downloadClient, _context.CookLocations, "Locations.json", reader);
            await RebuildItems(downloadClient, _context.CookCategories, "Categories.json", reader);
            await RebuildItems(downloadClient, _context.CookSellers, "Sellers.json", reader);
            await RebuildItems(downloadClient, _context.CookMaterialDrops, "MaterialDrops.json", reader);
            await RebuildItems(downloadClient, _context.CookMaterialSellers, "MaterialSellers.json", reader);
            await RebuildRecipes(downloadClient, _context.CookRecipes, "Recipes.json", reader);
            await RebuildItems(downloadClient, _context.Additionals, "Additionals.json", reader);
            await RebuildItems(downloadClient, _context.CookEffects, "Effects.json", reader);
        }

        private async Task RebuildItems<T>(UpdateClient downloadClient, DbSet<T> db, string fileName, SavannahXmlReader versionReader) where T : class
        {
            var versionNode = versionReader.GetNode($"/versions/version[@id='{fileName}']");
            if (versionNode == null)
                return;

            var version = versionNode.InnerText;

            var history = _context.Histories.FirstOrDefault(x => x.Name == fileName);
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
                _context.Histories.Add(new DbDownloadHistory
                {
                    Name = fileName,
                    Date = version
                });
            }
            else
            {
                history.Date = version;
            }

            await _context.SaveChangesAsync();
        }

        private async Task RebuildRecipes(UpdateClient downloadClient, DbSet<DbCookRecipes> db, string fileName, SavannahXmlReader versionReader)
        {
            var versionNode = versionReader.GetNode($"/versions/version[@id='{fileName}']");
            if (versionNode == null)
                return;

            var version = versionNode.InnerText;

            var history = _context.Histories.FirstOrDefault(x => x.Name == fileName);
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

                    _context.Add(material);
                }
            }

            if (history == null)
            {
                _context.Histories.Add(new DbDownloadHistory
                {
                    Name = fileName,
                    Date = version
                });
            }
            else
            {
                history.Date = version;
            }

            await _context.SaveChangesAsync();
        }
    }
}
