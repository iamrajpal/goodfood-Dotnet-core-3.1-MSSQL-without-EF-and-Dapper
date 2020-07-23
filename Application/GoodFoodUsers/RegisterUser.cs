using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using MediatR;

namespace Application.GoodFoodUsers
{
    public class RegisterUser
    {
        public class RegisterUserCommand : IRequest<GoodFoodUserDto>
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
        public class Handler : IRequestHandler<RegisterUserCommand, GoodFoodUserDto>
        {
            private readonly IUserAuth _userAuth;
            public Handler(IUserAuth userAuth)
            {
                _userAuth = userAuth;
            }

            public async Task<GoodFoodUserDto> Handle(RegisterUserCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.Register(request.UserName, request.Password);
                return user;
            }
        }
    }
}