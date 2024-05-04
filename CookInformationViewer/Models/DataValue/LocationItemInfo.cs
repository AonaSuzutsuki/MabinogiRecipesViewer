namespace CookInformationViewer.Models.DataValue;

public class LocationItemInfo
{
    public static readonly string NameCook = "料理";

    public static readonly string LocationSkill = "スキル";

    public static readonly string TypeCraft = "クラフト";
    public static readonly string TypeNpc = "NPC";
    public static readonly string TypeDrop = "ドロップ";

    public static LocationItemInfo CookItem => new()
    {
        Name = NameCook,
        Location = LocationSkill,
        Type = TypeCraft
    };

    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
}