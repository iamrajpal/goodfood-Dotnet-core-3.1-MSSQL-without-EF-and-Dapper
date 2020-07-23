using System;
using Domain.Enums;

namespace Domain.Entities
{
    public class Recipe
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string SlugUrl { get; set; }
        public RecipeCategory Category { get; set; } 
    }
}