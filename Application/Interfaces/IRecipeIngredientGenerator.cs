using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRecipeIngredientGenerator
    {
        Task<bool> Create(int recipeId, int ingredientId, int? measurementId);
        Task<bool> IsIdsExitInRecipeIngredient(int ingredientId, int recipeId, int? measurementId);
        Task<int> Update(int recipeId, int ingredientId, int measurementId);
    }
}