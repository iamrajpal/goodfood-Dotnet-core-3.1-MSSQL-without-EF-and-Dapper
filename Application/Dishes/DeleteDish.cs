using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using MediatR;

namespace Application.Dishes
{
    public class DeleteDish
    {
        public class DeleteDishCommand : IRequest
        {
            public List<int> DishIds { get; set; }
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<DeleteDishCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IDishGenerator _dishGenerator;
            public Handler(IUserAuth userAuth, IDishGenerator dishGenerator)
            {
                _dishGenerator = dishGenerator;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(DeleteDishCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                foreach (var dishId in request.DishIds)
                {
                    var dish = await _dishGenerator.GetDish(dishId, user.Id);
                    if (dish == null)
                        throw new RestException(HttpStatusCode.NotFound, new { Dish = "Not found" });
                }

                var success = await _dishGenerator.Delete(user.Id, request.DishIds);
                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}