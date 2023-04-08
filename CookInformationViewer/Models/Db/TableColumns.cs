using System.Collections.Generic;
using System.Linq;
using KimamaSqliteExecutorLib.Db.Raw;

namespace CookInformationViewer.Models.Db
{
    internal static class TableColumns
    {
        public static Dictionary<string, TableInfo> GetTables()
        {
            var cookMaterials = new TableInfo
            {
                TableName = Constants.CookMaterialsTableName,
                Columns = new[]
                {
                    new ColumnInfo
                    {
                        ColumnName = "id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "name",
                        Type = ColumnType.Text,
                        NotNull = true,
                        Unique = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            var cookCategories = new TableInfo
            {
                TableName = Constants.CookCategoriesTableName,
                Columns = new[]
                {
                    new ColumnInfo
                    {
                        ColumnName = "id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "name",
                        Type = ColumnType.Text,
                        NotNull = true,
                        Unique = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "skill_rank",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            var cookLocations = new TableInfo
            {
                TableName = Constants.CookLocationsTableName,
                Columns = new[]
                {
                    new ColumnInfo
                    {
                        ColumnName = "id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "name",
                        Type = ColumnType.Text,
                        NotNull = true,
                        Unique = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            var cookSellers = new TableInfo
            {
                TableName = Constants.CookSellersTableName,
                Columns = new[]
                {
                    new ColumnInfo
                    {
                        ColumnName = "id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "name",
                        Type = ColumnType.Text,
                        NotNull = true,
                        Unique = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "location_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            var cookRecipeId = new ColumnInfo
            {
                ColumnName = "id",
                Type = ColumnType.Integer,
                PrimaryKey = true
            };
            var cookRecipes = new TableInfo
            {
                TableName = Constants.CookRecipesTableName,
                Columns = new[]
                {
                    cookRecipeId,
                    new ColumnInfo
                    {
                        ColumnName = "name",
                        Type = ColumnType.Text,
                        NotNull = true,
                        Unique = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "category_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "item1_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "item2_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "item3_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "item1_recipe_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "item2_recipe_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "item3_recipe_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "item1_amount",
                        Type = ColumnType.Real
                    },
                    new ColumnInfo
                    {
                        ColumnName = "item2_amount",
                        Type = ColumnType.Real
                    },
                    new ColumnInfo
                    {
                        ColumnName = "item3_amount",
                        Type = ColumnType.Real
                    },
                    new ColumnInfo
                    {
                        ColumnName = "image_path",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "image_data",
                        Type = ColumnType.Blob
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            var cookMaterialSellers = new TableInfo
            {
                TableName = Constants.CookMaterialSellersTableName,
                Columns = new[]
                {
                    new ColumnInfo
                    {
                        ColumnName = "id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "material_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "seller_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            var cookMaterialDrops = new TableInfo
            {
                TableName = Constants.CookMaterialDropsTableName,
                Columns = new[]
                {
                    new ColumnInfo
                    {
                        ColumnName = "id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "material_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "drop_name",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "location_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_craft",
                        Type = ColumnType.Integer,
                        Default = "0"
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            var cookEffects = new TableInfo
            {
                TableName = Constants.CookEffectsTableName,
                Columns = new[]
                {
                    new ColumnInfo
                    {
                        ColumnName = "id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "recipe_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "number",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "star",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "ignore_check",
                        Type = ColumnType.Integer,
                        Default = "0"
                    },
                    new ColumnInfo
                    {
                        ColumnName = "hp",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "mana",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "stamina",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "str",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "inteli",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "dex",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "will",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "luck",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "damage",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "magic_damage",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "min_damage",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "protection",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "defense",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "magic_protection",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "magic_defense",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            var additional = new TableInfo
            {
                TableName = Constants.CookAdditionalsTableName,
                Columns = new[]
                {
                    new ColumnInfo
                    {
                        ColumnName = "id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "recipe_id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "check_star",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_material",
                        Type = ColumnType.Integer,
                        Default = 0L
                    },
                    new ColumnInfo
                    {
                        ColumnName = "not_festival",
                        Type = ColumnType.Integer,
                        Default = 0L
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            var downloadHistory = new TableInfo
            {
                TableName = Constants.DownloadHistoriesTableName,
                Columns = new []
                {
                    new ColumnInfo
                    {
                        ColumnName = "id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true,
                        AutoIncrement = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "name",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            var favorite = new TableInfo
            {
                TableName = Constants.CookFavoritesTableName,
                Columns = new[]
                {
                    new ColumnInfo
                    {
                        ColumnName = "id",
                        Type = ColumnType.Integer,
                        PrimaryKey = true,
                        AutoIncrement = true
                    },
                    new ColumnInfo
                    {
                        ColumnName = "recipe_id",
                        Type = ColumnType.Integer
                    },
                    new ColumnInfo
                    {
                        ColumnName = "create_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "update_date",
                        Type = ColumnType.Text
                    },
                    new ColumnInfo
                    {
                        ColumnName = "is_delete",
                        Type = ColumnType.Integer,
                        Default = 0L
                    }
                }
            };

            return new Dictionary<string, TableInfo>
            {
                { cookMaterials.TableName, cookMaterials },
                { cookCategories.TableName, cookCategories },
                { cookLocations.TableName, cookLocations },
                { cookSellers.TableName, cookSellers },
                { cookRecipes.TableName, cookRecipes },
                { cookMaterialSellers.TableName, cookMaterialSellers },
                { cookMaterialDrops.TableName, cookMaterialDrops },
                { cookEffects.TableName, cookEffects },
                { additional.TableName, additional },
                { downloadHistory.TableName, downloadHistory },
                { favorite.TableName, favorite }
            };
        }
    }
}