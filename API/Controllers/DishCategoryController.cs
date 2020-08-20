using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DishCategories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class DishCategoryController : BaseController
    {
        [HttpPost("{username}/create")]
        public async Task<ActionResult<Unit>> Create(string username, CreateDishCategory.CreateDishCategoryCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }

        [HttpPut("{username}/edit")]
        public async Task<ActionResult<Unit>> Edit(string username, UpdateDishCategory.UpdateDishCategoryCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }
        [HttpDelete("{username}/delete")]
        public async Task<ActionResult<Unit>> Delete(string username, DeleteDishCategory.DeleteDishCategoryCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }
        [HttpGet("{username}/list")]
        public async Task<ActionResult<List<DishCategory>>> List(string username)
        {
            return await Mediator.Send(new GetDishCategories.GetDishCategoriesQuery { Username = username });
        }

    }
}