using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRecipeGenerator
    {
        Task<RecipeDto> Create(int userId, Recipe recipe);
        Task<bool> IsRecipeExits(string recipename, int userId, string recipeSlug);
    }
}