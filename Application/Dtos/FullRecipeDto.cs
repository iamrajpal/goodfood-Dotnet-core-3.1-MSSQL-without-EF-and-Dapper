using Domain.Enums;

namespace Application.Dtos
{
    public class FullRecipeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SlugUrl { get; set; }
        public RecipeCategory Category { get; set; }
    }
}