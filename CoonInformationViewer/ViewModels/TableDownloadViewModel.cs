using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CoonInformationViewer.Models;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CoonInformationViewer.ViewModels
{
    public class DownloadItemViewInfo : BindableBase
    {
        private long _progress;
        private string _progressText = string.Empty;
        private string _message = string.Empty;

        public string Name { get; set; }

        public bool UpdateChecked { get; set; }

        public long Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public DownloadItemViewInfo(DownloadItemInfo info)
        {
            Name = info.Name;
            UpdateChecked = info.CurrentVersion != info.ServerVersion;
            Message = !UpdateChecked ? "Nothing" : "Available";
        }
    }

    public class TableDownloadViewModel : ViewModelBase
    {
        #region Fields

        private readonly TableDownloadModel _model;

        private readonly Dictionary<string, DownloadItemViewInfo?> _downloadDictionary = new();

        #endregion

        #region Properties

        public ReactiveProperty<bool> CanExit { get; set; }

        public ReactiveProperty<Visibility> LoadingVisibility { get; set; }

        public ReadOnlyReactiveCollection<DownloadItemViewInfo> DownloadList { get; set; }

        #endregion

        #region Event Properties

        public ICommand RebuildCommand { get; set; }
        public ICommand UpdateCommand { get; set; }

        #endregion

        public TableDownloadViewModel(IWindowService windowService, TableDownloadModel model) : base(windowService, model)
        {
            _model = model;
            _model.ProgressChanged.Subscribe(x =>
            {
                var item = _downloadDictionary.Get(x.FileName);
                if (item == null)
                    return;

                item.Progress = x.Percentage;
                item.ProgressText = $"{x.Percentage}%";

                if (x.Completed)
                    item.Message = "Completed";
            });

            CanExit = new ReactiveProperty<bool>(true);
            LoadingVisibility = new ReactiveProperty<Visibility>(Visibility.Visible);
            DownloadList = model.DownloadList.ToReadOnlyReactiveCollection(x =>
            {
                var item = new DownloadItemViewInfo(x);
                _downloadDictionary.Put(x.Name, item);
                return item;
            }).AddTo(CompositeDisposable);

            RebuildCommand = new DelegateCommand(Rebuild);
            UpdateCommand = new DelegateCommand(Update);

            _ = model.Initialize().ContinueWith(_ =>
            {
                LoadingVisibility.Value = Visibility.Collapsed;
            });
        }

        public void Rebuild()
        {
            CanExit.Value = false;

            foreach (var item in DownloadList)
            {
                item.Progress = 0;
                item.ProgressText = "0%";
            }

            _ = _model.RebuildDataBase().ContinueWith(_ => CanExit.Value = true);
        }

        public void Update()
        {
            CanExit.Value = false;

            foreach (var item in DownloadList)
            {
                item.Progress = 0;
                item.ProgressText = "0%";
            }

            _ = _model.UpdateDataBase(DownloadList.ToDictionary(x => Path.GetFileNameWithoutExtension(x.Name),
                x => x.UpdateChecked)).ContinueWith(_ => CanExit.Value = true);
        }

        public override void Dispose()
        {
            base.Dispose();

            _model.Dispose();
        }
    }
}
