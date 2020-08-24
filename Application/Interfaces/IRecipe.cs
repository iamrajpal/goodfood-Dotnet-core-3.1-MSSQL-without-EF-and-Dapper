using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRecipe
    {
        Task<bool> Create(int dishId, int ingredientId, string measurementAmount);
        Task<bool> IsIdsExitInRecipeIngredient(int ingredientId, int dishId);
        Task<int> Update(int dishId, int ingredientId, string measurementId);
    }
}