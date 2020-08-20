using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IDishGenerator
    {
        Task<Dish> Create(int userId, Dish dish);
        Task<bool> IsDishExitsWithSlug(int userId, string dishSlug);
        Task<bool> IsDishExits(string dishTitle, int userId);
        Task<bool> Update(int userId, int dishId, Dish dish);
        Task<Dish> GetDish(int dishId, int userId);
        Task<List<Dish>> GetDishes(int userId, string selectCommandText);
        Task<bool> Delete(int userId, List<int> dishIds);
        Task<bool> IsDishCategoryUsedByDish(int dishCateGoryid);
    }
}