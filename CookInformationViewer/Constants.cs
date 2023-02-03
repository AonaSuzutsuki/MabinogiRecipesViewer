using CommonCoreLib;

namespace CookInformationViewer
{
    public class Constants
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

        public const string DatabaseFileName = "CookInfo.dat";
        
        public static string Version => CommonCoreLib.File.Version.GetVersion(); /* + "b";*/

        public const string UpdateUrlFile = "UpdateUrl.xml";
        public static readonly string AppDirectoryPath = AppInfo.GetAppPath();
        public static readonly string UpdaterFilePath = AppDirectoryPath + @"\Updater\update.exe";
    }
}
