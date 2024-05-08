using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CookInformationViewer.Models.Db.Context;

namespace CookInformationViewer.Models.DataValue;

public class QualityItemInfo
{
    public int Star { get; set; }
    public string Quality { get; set; }
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

    public string Hp { get; set; }
    public string Mp { get; set; }
    public string Sp { get; set; }
    public string Str { get; set; }
    public string Int { get; set; }
    public string Dex { get; set; }
    public string Will { get; set; }
    public string Luck { get; set; }
    public string MaxDamage { get; set; }
    public string MinDamage { get; set; }
    public string MagicDamage { get; set; }
    public string Protection { get; set; }
    public string Defense { get; set; }
    public string MagicProtection { get; set; }
    public string MagicDefense { get; set; }

    public QualityItemInfo(int star, IEnumerable<DbCookEffects> effects)
    {
        var list = effects.ToList();

        Star = star;
        Quality = GetStarString(star);

        Hp = MinMaxString(list, x => x.Hp);
        Mp = MinMaxString(list, x => x.Mana);
        Sp = MinMaxString(list, x => x.Stamina);
        Str = MinMaxString(list, x => x.Str);
        Int = MinMaxString(list, x => x.Int);
        Dex = MinMaxString(list, x => x.Dex);
        Will = MinMaxString(list, x => x.Will);
        Luck = MinMaxString(list, x => x.Luck);
        MaxDamage = MinMaxString(list, x => x.Damage);
        MinDamage = MinMaxString(list, x => x.MinDamage);
        MagicDamage = MinMaxString(list, x => x.MagicDamage);
        Protection = MinMaxString(list, x => x.Protection);
        Defense = MinMaxString(list, x => x.Defense);
        MagicProtection = MinMaxString(list, x => x.MagicProtection);
        MagicDefense = MinMaxString(list, x => x.MagicDefense);
    }

    public QualityItemInfo(int star, IEnumerable<EffectInfo> effects)
    {
        var list = effects.ToList();

        Star = star;
        Quality = GetStarString(star);

        Hp = MinMaxString(list, x => x.Hp);
        Mp = MinMaxString(list, x => x.Mana);
        Sp = MinMaxString(list, x => x.Stamina);
        Str = MinMaxString(list, x => x.Str);
        Int = MinMaxString(list, x => x.Int);
        Dex = MinMaxString(list, x => x.Dex);
        Will = MinMaxString(list, x => x.Will);
        Luck = MinMaxString(list, x => x.Luck);
        MaxDamage = MinMaxString(list, x => x.Damage);
        MinDamage = MinMaxString(list, x => x.MinDamage);
        MagicDamage = MinMaxString(list, x => x.MagicDamage);
        Protection = MinMaxString(list, x => x.Protection);
        Defense = MinMaxString(list, x => x.Defense);
        MagicProtection = MinMaxString(list, x => x.MagicProtection);
        MagicDefense = MinMaxString(list, x => x.MagicDefense);
    }

    private string MinMaxString(List<DbCookEffects> effects, Func<DbCookEffects, int> selector)
    {
        var min = effects.MinBy(selector);
        var max = effects.MaxBy(selector);

        if (min == null || max == null)
            return string.Empty;

        var minValue = selector(min);
        var maxValue = selector(max);

        if (minValue == 0 || maxValue == 0)
            return string.Empty;

        return minValue.Equals(maxValue) ? minValue.ToString() : $"{minValue}-{maxValue}";
    }

    private string MinMaxString(List<EffectInfo> effects, Func<EffectInfo, int> selector)
    {
        var min = effects.MinBy(selector);
        var max = effects.MaxBy(selector);

        if (min == null || max == null)
            return string.Empty;

        var minValue = selector(min);
        var maxValue = selector(max);

        if (minValue == 0 || maxValue == 0)
            return string.Empty;

        return minValue.Equals(maxValue) ? minValue.ToString() : $"{minValue}-{maxValue}";
    }

    public static string GetStarString(int star)
    {
        return star switch
        {
            1 => "★☆☆☆☆",
            2 => "★★☆☆☆",
            3 => "★★★☆☆",
            4 => "★★★★☆",
            5 => "★★★★★",
            6 => "★★★★★",
            _ => "",
        };
    }
}