using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;

namespace CoonInformationViewer.Models
{
    public class DownloadItemInfo
    {
        public string Name { get; set; } = string.Empty;
        public string ServerVersion { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public bool CanUpdate { get; set; } = false;
    }

    public class TableDownloadModel : ModelBase
    {
        #region Fields

        private ObservableCollection<DownloadItemInfo> _downloadList = new();

        #endregion

        public ObservableCollection<DownloadItemInfo> DownloadList
        {
            get => _downloadList;
            set => SetProperty(ref _downloadList, value);
        }

        public TableDownloadModel(Dictionary<string, Tuple<string, bool>> availableItems, Dictionary<string, string> currentVersions)
        {
            foreach (var availableItem in availableItems)
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
    }
}
