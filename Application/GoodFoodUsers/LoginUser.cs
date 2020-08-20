using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Errors;
using Application.Interfaces;
using MediatR;

namespace Application.GoodFoodUsers
{
    public class LoginUser
    {
        public class LoginUserCommand : IRequest<GoodFoodUserDto>
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
        public class Handler : IRequestHandler<LoginUserCommand, GoodFoodUserDto>
        {
            private readonly IUserAuth _userAuth;
            private readonly IConnectionString _connection;
            private readonly IJwtGenerator _jwtGenerator;
            public Handler(IUserAuth userAuth, IConnectionString connection, IJwtGenerator jwtGenerator)
            {
                _jwtGenerator = jwtGenerator;
                _connection = connection;
                _userAuth = userAuth;
            }

            public async Task<GoodFoodUserDto> Handle(LoginUserCommand request,
                CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    throw new RestException(HttpStatusCode.BadRequest, new { UserPassword = "Required" });

                var userFromDB = await _userAuth.VerifyUser(request.Username, request.Password);

                if (userFromDB == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var user = new GoodFoodUserDto
                {
                    UserName = userFromDB.Username,
                    Token = _jwtGenerator.CreateToken(userFromDB)
                };

                return user;
            }

        }
    }
}