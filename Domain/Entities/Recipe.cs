using System.Collections.Generic;
using System.Collections.ObjectModel;
using Domain.Enums;

namespace Domain.Entities
{
    public class Recipe
    {
        public Recipe()
        {
            Ingredients =new Collection<Ingredients>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SlugUrl { get; set; }
        public RecipeCategory Category { get; set; } 
        public ICollection<Ingredients> Ingredients { get; set; }
    }
}