using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Errors;
using Application.Interfaces;
using MediatR;

namespace Application.Ingredient
{
    public class UpdateIngredient
    {
        public class UpdateIngredientCommand : IRequest
        {
            public int IngredientId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
        public class Handler : IRequestHandler<UpdateIngredientCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IIngredient _ingredient;
            public Handler(IUserAuth userAuth, IIngredient ingredient)
            {
                _ingredient = ingredient;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(UpdateIngredientCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetCurrentUser();
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var ingredient = await _ingredient.GetIngredient(user.Id, request.IngredientId);
                if (ingredient == null)
                    throw new RestException(HttpStatusCode.NotFound, new { Ingredent = "Not found" });

                var updateIngredent = new IngredientDto
                {
                    Name = request.Name ?? ingredient.Name,
                    Description = request.Description ?? ingredient.Description
                };

                var success = await _ingredient.Update(user.Id, request.IngredientId, updateIngredent);
                if (success > 0) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }

    }
}