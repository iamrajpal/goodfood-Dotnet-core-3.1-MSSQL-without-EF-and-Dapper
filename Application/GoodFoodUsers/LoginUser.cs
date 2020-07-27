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
                
                if (username == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });
             
                // if (!verifyPasswordHash(request.Password, user.User_Password_Hash, user.User_Password_Salt))
                //     throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var returnUser = new GoodFoodUserDto
                {
                    Username = username + " successfully login"
                };

                return returnUser;
            }

            private bool verifyPasswordHash(string password, byte[] user_Password_Hash, byte[] user_Password_Salt)
            {
                using (var hmac = new System.Security.Cryptography.HMACSHA512(user_Password_Salt))
                {
                    var computerHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                    for (int i = 0; i < computerHash.Length; i++)
                    {
                        if (computerHash[i] != user_Password_Hash[i]) return false;
                    }
                }
                return true;
            }
        }
    }
}