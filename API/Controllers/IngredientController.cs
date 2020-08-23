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
        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<Unit>> Create(CreateIngredient.CreateIngredientCommand command)
        {
            return await Mediator.Send(command);
        }
        [HttpGet("get")]
        public async Task<ActionResult<List<IngredientDto>>> Get()
        {
            return await Mediator.Send(new GetIngredients.GetIngredientsQuery());
        }
        [HttpPut("edit")]
        public async Task<ActionResult<Unit>> Edit(UpdateIngredient.UpdateIngredientCommand command)
        {
            return await Mediator.Send(command);
        }
        [HttpDelete("{ingredientId}/delete")]
        public async Task<ActionResult<Unit>> Delete(int ingredientId)
        {
            return await Mediator.Send(new DeleteIngredient.DeleteIngredientCommand { IngredientId = ingredientId });
        }
    }
}