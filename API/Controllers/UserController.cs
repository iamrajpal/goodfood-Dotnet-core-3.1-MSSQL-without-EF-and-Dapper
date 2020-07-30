using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using Application.GoodFoodUsers;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    public class UserController : BaseController
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<GoodFoodUserDto>> Register(RegisterUser.RegisterUserCommand command)
        {
            return await Mediator.Send(command);
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<GoodFoodUserDto>> Login(LoginUser.LoginUserCommand command)
        {
            return await Mediator.Send(command);
        }
        [HttpGet("alluser")]
        [Authorize]
        public async Task<ActionResult<List<GoodFoodUserDto>>> AllUser()
        {
            return await Mediator.Send(new GetAllUser.GetAllUserQuery());
        }
    }

}