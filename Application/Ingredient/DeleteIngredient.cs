using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using MediatR;

namespace Application.Ingredient
{
    public class DeleteIngredient
    {
        public class DeleteIngredientCommand : IRequest
        {
            public int IngredientId { get; set; }
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<DeleteIngredientCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IIngredientGenerator _ingredientGenerator;
            public Handler(IUserAuth userAuth, IIngredientGenerator ingredientGenerator)
            {
                _ingredientGenerator = ingredientGenerator;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(DeleteIngredientCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                if (!await _ingredientGenerator.IsIngredientExitById(request.IngredientId, user.Id))
                    throw new RestException(HttpStatusCode.NotFound, new { Ingredient = "Not found" });

                if (await _ingredientGenerator.IsIngredientExitInRecipeIngredient(request.IngredientId))
                    throw new RestException(HttpStatusCode.NotFound, new { Ingredient = "Recipe use this ingredient" });
              
                var success = await _ingredientGenerator.Delete(user.Id, request.IngredientId);
                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}