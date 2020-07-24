using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using MediatR;

namespace Application.Recipies
{
    public class DeleteRecipe
    {
        public class DeleteRecipeCommand : IRequest
        {
            public List<int> RecipeIds { get; set; }
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<DeleteRecipeCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IRecipeGenerator _recipeGenerator;
            public Handler(IUserAuth userAuth, IRecipeGenerator recipeGenerator)
            {
                _recipeGenerator = recipeGenerator;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(DeleteRecipeCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                foreach (var recipeId in request.RecipeIds)
                {
                    var recipe = await _recipeGenerator.GetRecipe(recipeId, user.Id);
                    if (recipe == null)
                        throw new RestException(HttpStatusCode.NotFound, new { Recipe = "Not found" });
                }

                var success = await _recipeGenerator.Delete(user.Id, request.RecipeIds);
                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}