using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
using CookInformationViewer.Models.Db;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.Db.Manager;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SavannahXmlLib.XmlWrapper;
using UpdateLib.Http;
using UpdateLib.Update;

namespace CookInformationViewer.Models
{
    public class MainWindowModel : ModelBase, IDisposable
    {

        private readonly UpdateContextManager _contextManager = new();

        private ObservableCollection<string> _categories = new();

        #region Properties

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public bool AvailableUpdate => _contextManager.AvailableUpdate;

        #endregion

        public MainWindowModel()
        {
            
        }

        public async Task Initialize()
        {
            Reload();

            await _contextManager.AvailableTableUpdateCheck();
        }

        public void Reload()
        {
            var categories = _contextManager.GetCategories(x =>
            {
                return x.CookCategories.Where(m => !m.IsDelete);
            });

            Categories.AddRange(categories.Select(x => x.Name));
        }

        public async Task RebuildDataBase()
        {
            await _contextManager.RebuildDataBase();
        }

        public void Dispose()
        {
            _contextManager.Dispose();
        }
    }
}
