using System;
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
        }
        public class Handler : IRequestHandler<DeleteIngredientCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IIngredient _ingredient;
            public Handler(IUserAuth userAuth, IIngredient ingredient)
            {
                _ingredient = ingredient;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(DeleteIngredientCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetCurrentUser();
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                if (!await _ingredient.IsIngredientExitById(request.IngredientId, user.Id))
                    throw new RestException(HttpStatusCode.NotFound, new { Ingredient = "Not found" });

                if (await _ingredient.IsIngredientExitInRecipeIngredient(request.IngredientId))
                    throw new RestException(HttpStatusCode.BadRequest, new { Ingredient = "Recipe use this ingredient" });

                var success = await _ingredient.Delete(user.Id, request.IngredientId);
                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}