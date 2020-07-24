using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Ingredeients;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class IngredientController : BaseController
    {
        [HttpPost("{username}/create")]
        public async Task<ActionResult<Unit>> Create(string username, CreateIngredient.CreateIngredientCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }
        [HttpGet("{username}/get")]
        public async Task<ActionResult<List<IngredientDto>>> Get(string username)
        {
            return await Mediator.Send(new GetIngredients.GetIngredientsQuery{Username = username});
        }
    }
}