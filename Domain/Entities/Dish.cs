using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Domain.Entities
{
    public class Dish
    {
        public Dish()
        {
            Ingredients =new Collection<Ingredients>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SlugUrl { get; set; }
        public int DishCategoryId { get; set; } 
        public string DishCategoryTitle { get; set; }
        public ICollection<Ingredients> Ingredients { get; set; }
    }
}