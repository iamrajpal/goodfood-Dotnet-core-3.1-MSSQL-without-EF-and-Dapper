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
            public Handler(IUserAuth userAuth, IConnectionString connection)
            {
                _connection = connection;
                _userAuth = userAuth;
            }

            public async Task<GoodFoodUserDto> Handle(LoginUserCommand request,
                CancellationToken cancellationToken)
            {
                var username = await _userAuth.VerifyUser(request.Username, request.Password);
                
                if (string.IsNullOrEmpty(username))
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });             
             
                var returnUser = new GoodFoodUserDto
                {
                    Username = username
                };

                return returnUser;
            }
            
        }
    }
}