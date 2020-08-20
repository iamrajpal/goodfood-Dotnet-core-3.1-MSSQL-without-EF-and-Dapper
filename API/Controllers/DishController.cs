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
        [HttpPost("{username}/create")]
        public async Task<ActionResult<Unit>> Create(string username, CreateDish.CreateDishCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }

        [HttpPut("{username}/edit")]
        public async Task<ActionResult<Unit>> Edit(string username, UpdateDish.UpdateDishCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }
        [HttpDelete("{username}/delete")]
        public async Task<ActionResult<Unit>> Delete(string username, DeleteDish.DeleteDishCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }
        [HttpGet("list")]
        public async Task<ActionResult<List<Dish>>> List(string username, int? limit,
            int? offset)
        {
            return await Mediator.Send(new GetDishes.GetDishesQuery(username, limit, offset));
        }

    }
}