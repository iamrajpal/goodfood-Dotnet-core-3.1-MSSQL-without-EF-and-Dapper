using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Errors;
using Application.Interfaces;
using MediatR;

namespace Application.GoodFoodUsers
{
    public class RegisterUser
    {
        public class RegisterUserCommand : IRequest<GoodFoodUserDto>
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
        public class Handler : IRequestHandler<RegisterUserCommand, GoodFoodUserDto>
        {
            private readonly IUserAuth _userAuth;
            private readonly IJwtGenerator _jwtGenerator;
            public Handler(IUserAuth userAuth, IJwtGenerator jwtGenerator)
            {
                _jwtGenerator = jwtGenerator;
                _userAuth = userAuth;
            }

            public async Task<GoodFoodUserDto> Handle(RegisterUserCommand request,
                CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    throw new RestException(HttpStatusCode.BadRequest, new { User_Password = "Required" });

                var userFromDB = await _userAuth.Register(request.Username, request.Password);

                if (userFromDB == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var user = new GoodFoodUserDto
                {
                    Username = userFromDB.Username,
                    Token = _jwtGenerator.CreateToken(userFromDB)
                };

                return user;
            }
        }
    }
}