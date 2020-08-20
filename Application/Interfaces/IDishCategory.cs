using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IDishCategory
    {
        Task<bool> Create(int userId, DishCategory dishCategory);
        Task<bool> IsDishCategoryExits(string dishCategoryTitle, int userId);
        Task<bool> Update(int userId, int dishCategoryId, DishCategory dishCategory);
        Task<DishCategory> GetDishCategory(int dishCategoryId, int userId);
        Task<List<DishCategory>> GetDishCategories(int userId);
        Task<bool> Delete(int userId, int dishCategorId);
    }
}