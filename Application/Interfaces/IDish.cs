using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IDish
    {
        Task<Dish> Create(int userId, Dish dish);
        Task<bool> IsDishExitsWithSlug(int userId, string dishSlug);
        Task<bool> IsDishExitsWithTitle(string dishTitle, int userId);
        Task<bool> Update(int userId, int dishId, Dish dish);
        Task<Dish> GetDish(int dishId, int userId);
        Task<List<Dish>> GetDishes(int userId, string selectCommandText, int? offset, int? limit);
        Task<bool> Delete(int userId, List<int> dishIds);
        Task<bool> IsDishCategoryUsedByDish(int dishCateGoryid);
    }
}