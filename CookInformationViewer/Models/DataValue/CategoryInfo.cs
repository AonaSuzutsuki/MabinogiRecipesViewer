﻿using System.Windows.Media;
using CookInformationViewer.Models.Db.Context;
using Prism.Mvvm;

namespace CookInformationViewer.Models.DataValue;

public class CategoryInfo : BindableBase
{
    public static CategoryInfo Empty { get; } = new();

    public static CategoryInfo Favorite => new()
    {
        Id = 0,
        Name = FavoriteContent
    };

    public const string FavoriteContent = "お気に入り";

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Brush Foreground => SameFavorite() ? Constants.FavoriteForeground : new SolidColorBrush(Colors.White);

    public CategoryInfo()
    {
    }

    public CategoryInfo(DbCookCategories category)
    {
        Id = category.Id;
        Name = category.Name;
    }

    public bool SameFavorite()
    {
        return Name == FavoriteContent;
    }
}