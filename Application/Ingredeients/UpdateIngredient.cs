using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Errors;
using Application.Interfaces;
using MediatR;

namespace Application.Ingredeients
{
    public class UpdateIngredient
    {
        public class UpdateIngredientCommand : IRequest
        {
            public int IngredientId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<UpdateIngredientCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IIngredientGenerator _ingredientGenerator;
            public Handler(IUserAuth userAuth, IIngredientGenerator ingredientGenerator)
            {
                _ingredientGenerator = ingredientGenerator;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(UpdateIngredientCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var ingredent = await _ingredientGenerator.GetIngredient(user.Id, request.IngredientId);
                if (ingredent == null)
                    throw new RestException(HttpStatusCode.NotFound, new { Ingredent = "Not found" });

                var updateIngredent = new IngredientDto
                {
                    Name = request.Name ?? ingredent.Name,
                    Description = request.Description ?? ingredent.Description
                };

                var success = await _ingredientGenerator.Update(user.Id, request.IngredientId, updateIngredent);
                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }

    }
}