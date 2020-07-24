using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRecipeIngredientGenerator
    {
        Task<bool> Create(int recipeId, int ingredientId, int? measurementId);
    }
}