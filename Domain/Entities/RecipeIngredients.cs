namespace Domain.Entities
{
    public class RecipeIngredients
    {
        public Dish Dish { get; set; }
        public Ingredients Ingredient { get; set; }
    }
}