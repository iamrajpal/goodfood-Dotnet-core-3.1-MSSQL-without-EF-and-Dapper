namespace Domain.Entities
{
    public class RecipeIngredients
    {
        public Recipe Recipe { get; set; }
        public Ingredients Ingredient { get; set; }
        public Measurement Measurement { get; set; }
    }
}