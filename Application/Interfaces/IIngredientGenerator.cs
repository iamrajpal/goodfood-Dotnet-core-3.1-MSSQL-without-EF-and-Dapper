using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IIngredientGenerator
    {
        Task<bool> Create(int userId, Ingredients recipe);
        Task<bool> IsIngredientExitByName(string ingredientName, int userId, string slugUrl);
        Task<bool> IsIngredientExitById(int ingredientId, int userId);
        Task<List<IngredientDto>> GetIngredients(int userId);
        Task<IngredientDto> GetIngredient(int userId, int ingredientId);
        Task<bool> Update(int userId, int ingredientId, IngredientDto ingredient);
        Task<bool> IsIngredientExitInRecipeIngredient(int ingredientId);
        Task<bool> Delete(int userId, int ingredientId);
    }
}