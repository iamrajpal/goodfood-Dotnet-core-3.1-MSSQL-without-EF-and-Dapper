using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Ingredeients
{
    public class CreateIngredient
    {
        public class CreateIngredientCommand : IRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string SlugUrl { get; set; }
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<CreateIngredientCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IIngredientGenerator _ingredientGenerator;
            public Handler(IUserAuth userAuth, IIngredientGenerator ingredientGenerator)
            {
                _ingredientGenerator = ingredientGenerator;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(CreateIngredientCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                if (await _ingredientGenerator.IsIngredientExits(request.Name, user.Id))
                    throw new RestException(HttpStatusCode.BadRequest, new { Ingredient = "Already exist" });

                var createIngredient = new Ingredients
                {
                    Name = request.Name,
                    Description = request.Description,
                    SlugUrl = request.SlugUrl
                };

                var success = await _ingredientGenerator.Create(user.Id, createIngredient);

                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}