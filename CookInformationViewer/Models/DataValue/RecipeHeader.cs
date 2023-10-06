namespace CookInformationViewer.Models.DataValue;

public class RecipeHeader
{
    public bool IsHeader { get; set; }

    public CategoryInfo Category { get; set; }

    public RecipeInfo Recipe { get; set; }

    public string Additional { get; set; } = "";

    public RecipeHeader(RecipeInfo recipe)
    {
        Recipe = recipe;
        Category = recipe.Category ?? new CategoryInfo();
    }

    public RecipeHeader(string name)
    {
        IsHeader = true;
        Recipe = new RecipeInfo();
        Category = new CategoryInfo
        {
            Name = name
        };
    }
}