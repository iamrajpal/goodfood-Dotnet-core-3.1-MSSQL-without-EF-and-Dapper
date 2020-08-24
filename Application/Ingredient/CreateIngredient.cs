using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using MediatR;

namespace Application.Ingredient
{
    public class CreateIngredient
    {
        public class CreateIngredientCommand : IRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string SlugUrl { get; set; }
        }
        public class Handler : IRequestHandler<CreateIngredientCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IIngredient _ingredient;
            public Handler(IUserAuth userAuth, IIngredient ingredient)
            {
                _ingredient = ingredient;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(CreateIngredientCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetCurrentUser();
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                if (await _ingredient.IsIngredientExitByName(request.Name, user.Id, request.SlugUrl))
                    throw new RestException(HttpStatusCode.BadRequest, new { Ingredient_slug = "Already exist" });

                var createIngredient = new Domain.Entities.Ingredients
                {
                    Name = request.Name,
                    Description = request.Description,
                    SlugUrl = request.SlugUrl
                };

                var success = await _ingredient.Create(user.Id, createIngredient);

                if (success > 0) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}