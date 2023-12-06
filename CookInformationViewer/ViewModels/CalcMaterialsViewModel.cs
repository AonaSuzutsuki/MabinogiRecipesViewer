using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CookInformationViewer.ViewModels
{
    public class CalcMaterialsViewModel : ViewModelWindowStyleBase
    {
        private readonly CalcMaterialsModel _model;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ReactiveProperty<bool> IgnoreCanPurchasableChecked { get; set; }
        public ReactiveProperty<string> RecipeCountText { get; set; }
        public ReadOnlyReactiveCollection<CalcMaterialFlatInfo> Materials { get; set; }
        public ReactiveProperty<CalcMaterialFlatInfo> MaterialSelectedItem { get; set; }

        public ICommand RecipeCountTextChangedCommand { get; set; }
        public ICommand ReduceRecipeCountCommand { get; set; }
        public ICommand IncreaseRecipeCountCommand { get; set; }
        public ICommand UsedRecipesMouseDoubleClickCommand { get; set; }

        public CalcMaterialsViewModel(IWindowService windowService, CalcMaterialsModel model, MainWindowViewModel mainWindowViewModel) : base(windowService, model)
        {
            _model = model;
            _mainWindowViewModel = mainWindowViewModel;

            IgnoreCanPurchasableChecked = model.ToReactivePropertyAsSynchronized(x => x.IgnoreCanPurchasableChecked)
                .AddTo(CompositeDisposable);
            IgnoreCanPurchasableChecked.PropertyChanged += IgnoreCanPurchasableCheckedOnPropertyChanged;
            RecipeCountText = new ReactiveProperty<string>("1");
            Materials = model.Materials.ToReadOnlyReactiveCollection(x => x).AddTo(CompositeDisposable);
            MaterialSelectedItem = new ReactiveProperty<CalcMaterialFlatInfo>();

            RecipeCountTextChangedCommand = new DelegateCommand(RecipeCountTextChanged);
            ReduceRecipeCountCommand = new DelegateCommand(ReduceRecipeCount);
            IncreaseRecipeCountCommand = new DelegateCommand(IncreaseRecipeCount);
            UsedRecipesMouseDoubleClickCommand = new DelegateCommand<CalcMaterialInfo?>(UsedRecipesMouseDoubleClick);
        }

        private void IgnoreCanPurchasableCheckedOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!int.TryParse(RecipeCountText.Value, out var count))
            {
                count = 1;
            }

            _model.Analyze(count);
        }

        private void RecipeCountTextChanged()
        {
            if (!int.TryParse(RecipeCountText.Value, out var count))
            {
                count = 1;
            }

            _model.Analyze(count);
        }

        private void ReduceRecipeCount()
        {
            if (!int.TryParse(RecipeCountText.Value, out var count))
            {
                count = 1;
            }

            count--;

            if (count <= 0)
                return;

            RecipeCountText.Value = count.ToString();
        }

        private void IncreaseRecipeCount()
        {
            if (!int.TryParse(RecipeCountText.Value, out var count))
            {
                count = 1;
            }

            count++;

            RecipeCountText.Value = count.ToString();
        }

        private void UsedRecipesMouseDoubleClick(CalcMaterialInfo? materialInfo)
        {
            if (materialInfo == null)
                return;

            var recipeHeader = _model.ToRecipeHeader(materialInfo);

            if (recipeHeader == null)
                return;

            _mainWindowViewModel.SelectCategory(recipeHeader);

            _mainWindowViewModel.SelectRecipe(recipeHeader);
        }
    }
}
