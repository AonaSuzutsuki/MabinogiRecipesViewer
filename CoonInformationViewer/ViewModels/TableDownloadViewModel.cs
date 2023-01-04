using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CoonInformationViewer.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CoonInformationViewer.ViewModels
{
    public class DownloadItemViewInfo
    {
        public string Name { get; set; }

        public bool UpdateChecked { get; set; }

        public int Progress { get; set; }

        public string Message { get; set; }

        public DownloadItemViewInfo(DownloadItemInfo info)
        {
            Name = info.Name;

            Message = info.CurrentVersion == info.ServerVersion ? "Nothing" : "Available";
        }
    }

    public class TableDownloadViewModel : ViewModelBase
    {
        #region Properties

        public ReadOnlyReactiveCollection<DownloadItemViewInfo> DownloadList { get; set; }

        #endregion

        public TableDownloadViewModel(IWindowService windowService, TableDownloadModel model) : base(windowService, model)
        {
            DownloadList = model.DownloadList.ToReadOnlyReactiveCollection(x => new DownloadItemViewInfo(x)).AddTo(CompositeDisposable);
        }
    }
}
