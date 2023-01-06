using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.Db.Manager;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UpdateLib.Update;
using System.Reactive.Subjects;

namespace CookInformationViewer.Models
{
    public class DownloadItemInfo
    {
        public string Name { get; set; } = string.Empty;
        public string ServerVersion { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public bool CanUpdate { get; set; } = false;
    }

    public class TableDownloadModel : ModelBase, IDisposable
    {
        #region Fields

        private readonly MainWindowModel _mainWindowModel;
        private readonly UpdateContextManager _updateContextManager;
        private ObservableCollection<DownloadItemInfo> _downloadList = new();

        #endregion

        public Dictionary<string, Tuple<string, bool>> AvailableTableUpdate { get; private set; } = new();

        public ObservableCollection<DownloadItemInfo> DownloadList
        {
            get => _downloadList;
            set => SetProperty(ref _downloadList, value);
        }


        #region Events

        private readonly Subject<UpdateEventArgs> _progressChangedSubject = new();
        public IObservable<UpdateEventArgs> ProgressChanged => _progressChangedSubject;

        #endregion

        public TableDownloadModel(MainWindowModel mainWindowModel)
        {
            _mainWindowModel = mainWindowModel;
            
            _updateContextManager = new UpdateContextManager();
            _updateContextManager.ProgressChanged.Subscribe(_progressChangedSubject.OnNext);
        }

        public async Task Initialize()
        {
            await _updateContextManager.AvailableTableUpdateCheck();

            AvailableTableUpdate = _updateContextManager.AvailableTableUpdate;
            var currentVersions = _updateContextManager.CurrentTableVersion();

            foreach (var availableItem in AvailableTableUpdate)
            {
                DownloadList.Add(new DownloadItemInfo
                {
                    Name = availableItem.Key,
                    ServerVersion = availableItem.Value.Item1,
                    CurrentVersion = currentVersions.Get(availableItem.Key, string.Empty),
                    CanUpdate = availableItem.Value.Item2
                });
            }
        }

        public async Task RebuildDataBase()
        {
            await _updateContextManager.RebuildDataBase();
        }

        public async Task UpdateDataBase(Dictionary<string, bool> targetFileName)
        {
            await _updateContextManager.UpdateDataBase(targetFileName);
        }
        
        public void Dispose()
        {
            _updateContextManager.Dispose();
            _progressChangedSubject.Dispose();
        }
    }
}
