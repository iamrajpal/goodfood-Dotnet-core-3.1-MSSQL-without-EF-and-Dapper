using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Interfaces
{
    public interface IRecipeGenerator
    {
        Task<RecipeDto> Create(int userId, Recipe recipe);
        Task<bool> IsRecipeExits(string recipename, int userId, string recipeSlug);
        Task<bool> Update(int userId, int recipeId, Recipe recipe);
        Task<Recipe> GetRecipe(int recipeId, int userId);
    }
}