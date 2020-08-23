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
        [HttpPost("create")]
        public async Task<ActionResult<Unit>> Create(CreateDishCategory.CreateDishCategoryCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpPut("edit")]
        public async Task<ActionResult<Unit>> Edit(UpdateDishCategory.UpdateDishCategoryCommand command)
        {
            return await Mediator.Send(command);
        }
        [HttpDelete("delete")]
        public async Task<ActionResult<Unit>> Delete(DeleteDishCategory.DeleteDishCategoryCommand command)
        {
            return await Mediator.Send(command);
        }
        [HttpGet("list")]
        public async Task<ActionResult<List<DishCategory>>> List(string username)
        {
            return await Mediator.Send(new GetDishCategories.GetDishCategoriesQuery());
        }

    }
}