using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Ingredient;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class IngredientController : BaseController
    {
        [HttpPost("{username}/create")]
        [Authorize]
        public async Task<ActionResult<Unit>> Create(string username, CreateIngredient.CreateIngredientCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }
        [HttpGet("{username}/get")]
        public async Task<ActionResult<List<IngredientDto>>> Get(string username)
        {
            return await Mediator.Send(new GetIngredients.GetIngredientsQuery { Username = username });
        }
        [HttpPut("{username}/edit")]
        public async Task<ActionResult<Unit>> Edit(string username, UpdateIngredient.UpdateIngredientCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }
        [HttpDelete("{username}/user/{ingredientId}/delete")]
        public async Task<ActionResult<Unit>> Delete(string username, int ingredientId)
        {
            return await Mediator.Send(new DeleteIngredient.DeleteIngredientCommand { Username = username, IngredientId = ingredientId });
        }
    }
}