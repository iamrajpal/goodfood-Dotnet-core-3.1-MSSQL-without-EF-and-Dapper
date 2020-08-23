using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dishes;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class DishController : BaseController
    {
        [HttpPost("create")]
        public async Task<ActionResult<Unit>> Create(CreateDish.CreateDishCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpPut("edit")]
        public async Task<ActionResult<Unit>> Edit(UpdateDish.UpdateDishCommand command)
        {
            return await Mediator.Send(command);
        }
        [HttpDelete("delete")]
        public async Task<ActionResult<Unit>> Delete(DeleteDish.DeleteDishCommand command)
        {
            return await Mediator.Send(command);
        }
        [HttpGet("list")]
        public async Task<ActionResult<List<Dish>>> List(string username, int? limit,
            int? offset)
        {
            return await Mediator.Send(new GetDishes.GetDishesQuery(username, limit, offset));
        }
        [HttpPut("addIngredients")]
        public async Task<ActionResult<Unit>> AddIngredients(AddIngredient.AddIngredientCommand command)
        {
            return await Mediator.Send(command);
        }

    }
}