using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using Application.GoodFoodUsers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class UserController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<List<GoodFoodUserDto>>> CurrentUser()
        {
            return await Mediator.Send(new GetUser.GetUserQuery());
        }
        [HttpPost]
        [HttpPost("register")]
        public async Task<ActionResult<GoodFoodUserDto>> Register(RegisterUser.RegisterUserCommand command)
        {
            return await Mediator.Send(command);
        }
        [HttpPost]
        [HttpPost("login")]
        public async Task<ActionResult<GoodFoodUserDto>> Login(LoginUser.LoginUserCommand command)
        {
            return await Mediator.Send(command);
        }
    }

}