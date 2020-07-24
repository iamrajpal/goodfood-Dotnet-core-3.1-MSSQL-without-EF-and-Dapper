using System.Threading.Tasks;
using Application.Dtos;
using Application.Recipies;
using MediatR;
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

        [HttpPut("{username}/edit")]
        public async Task<ActionResult<Unit>> Edit(string username, UpdateRecipe.UpdateRecipeCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }
        [HttpDelete("{username}/delete")]
        public async Task<ActionResult<Unit>> Delete(string username, DeleteRecipe.DeleteRecipeCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }

    }
}