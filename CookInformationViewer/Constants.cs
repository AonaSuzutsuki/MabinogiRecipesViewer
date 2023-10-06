using System.Windows.Media;
using CommonCoreLib;

namespace CookInformationViewer
{
    public static class Constants
    {
        public const string Schema = "cookinfo";

        public const string CookMaterialsTableName = "cook_materials";
        public const string CookCategoriesTableName = "cook_categories";
        public const string CookLocationsTableName = "cook_locations";
        public const string CookSellersTableName = "cook_sellers";
        public const string CookRecipesTableName = "cook_recipes";
        public const string CookMaterialSellersTableName = "cook_material_sellers";
        public const string CookMaterialDropsTableName = "cook_material_drops";
        public const string CookEffectsTableName = "cook_effects";
        public const string DownloadHistoriesTableName = "download_histories";
        public const string CookAdditionalsTableName = "cook_additionals";
        public const string CookFavoritesTableName = "cook_favorites";
        public const string MetaTableName = "meta";
        public const string CookMemosTableName = "cook_memos";

        public const string DatabaseFileName = "Data\\CookInfo.dat";

        public static readonly SolidColorBrush FavoriteForeground =
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff9460") ?? new SolidColorBrush(Colors.White));

        public static readonly SolidColorBrush StarSixForeground =
            (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff5307") ?? new SolidColorBrush(Colors.White));

        public static string Version => CommonCoreLib.File.Version.GetVersion() + "b";

        public const string UpdateUrlFile = "UpdateUrl.xml";
        public static readonly string AppDirectoryPath = AppInfo.GetAppPath();
        public static readonly string UpdaterFilePath = AppDirectoryPath + @"\Updater\update.exe";

#if DEBUG
        public static readonly bool IsDebugMode = true;
#else
        public static readonly bool IsDebugMode = false;
#endif
    }
}
