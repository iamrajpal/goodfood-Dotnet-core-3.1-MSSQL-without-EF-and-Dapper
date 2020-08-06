using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRecipeGenerator
    {
        Task<Recipe> Create(int userId, Recipe recipe);
        Task<bool> IsRecipeExitsWithSlug(int userId, string recipeSlug);
        Task<bool> IsRecipeExits(string recipeTitle, int userId);
        Task<bool> Update(int userId, int recipeId, Recipe recipe);
        Task<Recipe> GetRecipe(int recipeId, int userId);
        Task<List<Recipe>> GetRecipes(int userId, string commandText);
        Task<bool> Delete(int userId, List<int> recipeIds);
    }
}