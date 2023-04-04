using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
using CookInformationViewer.Models.Converters;
using CookInformationViewer.Models.Db.Manager;
using CookInformationViewer.Models.Extensions;
using KimamaSqlExecutorLib.Db.Loader;
using KimamaSqlExecutorLib.Db.Raw;
using KimamaSqliteExecutorLib.Db.Raw;

namespace CookInformationViewer.Models
{
    public record UpdateHistoryItem
    {
        public string Date { get; set; } = string.Empty;
    }

    public record UpdateRecipeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public BitmapImage? Image { get; set; }
    }

    public class ListUpdateHistoryModel : ModelBase, IDisposable
    {
        private readonly SqlLoader _sqlLoader;
        private readonly ContextManager _contextManager;

        private ObservableCollection<UpdateHistoryItem> _recipeDateItems;
        private ObservableCollection<UpdateRecipeItem> _recipeItems = new();

        public ObservableCollection<UpdateHistoryItem> RecipeDateItems
        {
            get => _recipeDateItems;
            set => SetProperty(ref _recipeDateItems, value);
        }

        public ObservableCollection<UpdateRecipeItem> RecipeItems
        {
            get => _recipeItems;
            set => SetProperty(ref _recipeItems, value);
        }

        public ListUpdateHistoryModel(ContextManager contextManager)
        {
            _contextManager = contextManager;

            var loader = new SqlLoader("LatestRecipes.xml", "CookInformationViewer.Resources.SQL", Assembly.GetExecutingAssembly());
            loader.SetQuery("getLatestDates");
            var creator = SqlCreator.Create(loader.ToString());

            var dates = contextManager.Execute(creator);
            var items = dates.Select(x => new UpdateHistoryItem
            {
                Date = NullableDictionaryExtension.Get(x, "latest_date", SelectValueCreator.Create("")).GetValue<string>() ?? string.Empty
            });

            _recipeDateItems = new ObservableCollection<UpdateHistoryItem>(items);

            _sqlLoader = loader;
        }

        public void GetRecipes(string date)
        {
            _sqlLoader.SetQuery("getLatestRecipeAndDate");
            _sqlLoader.SetParameter("date", $"{date}");
            var parameters = _sqlLoader.GetParameters();
            var creator = SqlCreator.Create(_sqlLoader.ToString());

            var items = _contextManager.Execute(creator, parameters).Select(x =>
            {
                var item = new UpdateRecipeItem
                {
                    Id = (int)NullableDictionaryExtension.Get(x, "id", SelectValueCreator.Create(0)).GetValue<long>(),
                    Name = NullableDictionaryExtension.Get(x, "name", SelectValueCreator.Create("")).GetValue<string>() ?? string.Empty,
                    Date = NullableDictionaryExtension.Get(x, "latest_date", SelectValueCreator.Create("")).GetValue<string>() ?? string.Empty,
                    CategoryId = (int)NullableDictionaryExtension.Get(x, "category_id", SelectValueCreator.Create("")).GetValue<long>(),
                    CategoryName = NullableDictionaryExtension.Get(x, "category_name", SelectValueCreator.Create("")).GetValue<string>() ?? string.Empty
                };

                var imageData = NullableDictionaryExtension.Get(x, "image_data", new SelectValue(null, typeof(byte[])))
                    .GetValue<byte[]>();
                
                if (imageData == null)
                {
                    item.Image = ImageLoader.GetNoImage();
                }
                else
                {
                    using var ms = new MemoryStream(imageData);
                    item.Image = ImageLoader.CreateBitmapImage(ms, 80, 80);
                }

                return item;
            });

            RecipeItems.Clear();
            RecipeItems.AddRange(items);
        }

        public void Dispose()
        {
            _sqlLoader.Dispose();
        }
    }
}
