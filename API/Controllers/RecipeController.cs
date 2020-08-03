using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Recipies;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class RecipeController : BaseController
    {
        [HttpPost("{username}/create")]
        public async Task<ActionResult<Unit>> Create(string username, CreateRecipe.CreateRecipeCommand command)
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
        [HttpGet("list")]
        public async Task<ActionResult<List<Recipe>>> List(string username, int? limit,
            int? offset)
        {
            return await Mediator.Send(new GetRecipies.GetRecipiesQuery(username, limit, offset));
        }

    }
}