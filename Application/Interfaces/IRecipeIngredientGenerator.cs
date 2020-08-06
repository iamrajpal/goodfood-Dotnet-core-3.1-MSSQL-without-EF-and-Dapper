using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRecipeIngredientGenerator
    {
        Task<bool> Create(int recipeId, int ingredientId, string measurementAmount);
        Task<bool> IsIdsExitInRecipeIngredient(int ingredientId, int recipeId);
        Task<int> Update(int recipeId, int ingredientId, string measurementId);
    }
}