using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CookInformationViewer.Models.Converters;
using CookInformationViewer.Models.Db.Context;
using Prism.Mvvm;

namespace CookInformationViewer.Models.DataValue;

public class RecipeInfo : BindableBase
{
    private bool _isSelected;
    private bool _isFavorite;
    private Brush _foreground = new SolidColorBrush(Colors.White);

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string? Item1Name { get; set; }
    public string? Item2Name { get; set; }
    public string? Item3Name { get; set; }

    public bool IsItem1Material => Item1RecipeId == null;
    public bool IsItem2Material => Item2RecipeId == null;
    public bool IsItem3Material => Item3RecipeId == null;

    public int? Item1Id { get; set; }

    public int? Item2Id { get; set; }

    public int? Item3Id { get; set; }

    public int? Item1RecipeId { get; set; }

    public int? Item2RecipeId { get; set; }

    public int? Item3RecipeId { get; set; }

    public decimal Item1Amount { get; set; }

    public decimal Item2Amount { get; set; }

    public decimal Item3Amount { get; set; }

    public CategoryInfo? Category { get; set; }

    public IEnumerable<LocationItemInfo>? Item1Locations { get; set; }
    public IEnumerable<LocationItemInfo>? Item2Locations { get; set; }
    public IEnumerable<LocationItemInfo>? Item3Locations { get; set; }

    public DateTime? UpdateDate { get; set; }
    public BitmapImage? Image { get; set; }

    public bool IsMaterial { get; set; }

    public bool IsNotFestival { get; set; }

    public string? Special { get; set; }

    public int Star { get; set; }

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            Foreground = value ? Constants.FavoriteForeground : new SolidColorBrush(Colors.White);
            SetProperty(ref _isFavorite, value);
        }
    }

    public string StarText
    {
        get
        {
            if (IsMaterial)
                return "料理素材 専用 (食べられないよ)";

            if (!string.IsNullOrEmpty(Special))
                return Special;

            return Star switch
            {
                1 => "★☆☆☆☆",
                2 => "★★☆☆☆",
                3 => "★★★☆☆",
                4 => "★★★★☆",
                5 => "★★★★★",
                6 => "★★★★★",
                _ => "未確認"
            };
        }
    }

    public string StarMessage
    {
        get
        {
            var text = Star switch
            {
                1 => "確認済み",
                2 => "確認済み",
                3 => "確認済み",
                4 => "究極の料理 確認済み",
                5 => "天国の料理 確認済み",
                6 => "最高の料理 確認済み",
                _ => ""
            };

            if (IsNotFestival)
                text = $"{text} フェスティバルフード不可";

            return text;
        }
    }

    public Brush StarBrush
    {
        get
        {
            return Star switch
            {
                6 => Constants.StarSixForeground,
                _ => new SolidColorBrush(Colors.White)
            };
        }
    }

    public Brush Foreground
    {
        get => _foreground;
        set => SetProperty(ref _foreground, value);
    }

    public string Url { get; set; }

    public IEnumerable<QualityItemInfo>? Effects { get; set; }

    public RecipeInfo(DbCookRecipes recipe)
    {
        Id = recipe.Id;
        Name = recipe.Name;
        Item1Id = recipe.Item1Id;
        Item2Id = recipe.Item2Id;
        Item3Id = recipe.Item3Id;
        Item1RecipeId = recipe.Item1RecipeId;
        Item2RecipeId = recipe.Item2RecipeId;
        Item3RecipeId = recipe.Item3RecipeId;
        Item1Amount = recipe.Item1Amount;
        Item2Amount = recipe.Item2Amount;
        Item3Amount = recipe.Item3Amount;
        UpdateDate = recipe.UpdateDate ?? recipe.CreateDate;

        Url = $"https://mabicook.aonsztk.xyz/Home/Recipe?recipeId={Id}";

        if (recipe.ImageData == null)
        {
            Image = ImageLoader.GetNoImage();
        }
        else
        {
            using var ms = new MemoryStream(recipe.ImageData);
            Image = ImageLoader.CreateBitmapImage(ms, 80, 80);
        }
    }

    public RecipeInfo()
    {
        Name = "";
        Url = "";
    }
}