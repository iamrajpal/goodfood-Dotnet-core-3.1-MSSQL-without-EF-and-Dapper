using System.Threading.Tasks;
using Application.Dtos;
using Application.Recipies;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class RecipeController : BaseController
    {
        [HttpPost("{username}/create")]
        public async Task<ActionResult<RecipeDto>> Create(string username, CreateRecipe.CreateRecipeCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }

    }
}