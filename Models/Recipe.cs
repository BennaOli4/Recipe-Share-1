namespace RecipeShare.Models
{
    public class Recipe
    {
        public int Id { get; set; } // Primary Key
        public string Title { get; set; } = string.Empty;
        public string Ingredients { get; set; } = string.Empty;
        public string Steps { get; set; } = string.Empty;
        public int CookingTime { get; set; } // in minutes
        public string? DietaryTags { get; set; }
    }
}
