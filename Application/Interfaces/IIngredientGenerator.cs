using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IIngredientGenerator
    {
        Task<int> Create(int userId, Ingredients ingredient);
        Task<bool> IsIngredientExitByName(string ingredientName, int userId, string slugUrl);
        Task<bool> IsIngredientExitById(int ingredientId, int userId);
        Task<List<IngredientDto>> GetIngredients(int userId);
        Task<IngredientDto> GetIngredient(int userId, int ingredientId);
        Task<int> Update(int userId, int ingredientId, IngredientDto ingredient);
        Task<bool> IsIngredientExitInRecipeIngredient(int ingredientId);
        Task<bool> Delete(int userId, int ingredientId);
    }
}