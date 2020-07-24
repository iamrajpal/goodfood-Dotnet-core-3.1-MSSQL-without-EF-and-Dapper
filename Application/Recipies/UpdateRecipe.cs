using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Recipies
{
    public class UpdateRecipe
    {
        public class UpdateRecipeCommand : IRequest
        {
            public int RecipeId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public RecipeCategory Category { get; set; }
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<UpdateRecipeCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IRecipeGenerator _recipeGenerator;
            public Handler(IUserAuth userAuth, IRecipeGenerator recipeGenerator)
            {
                _recipeGenerator = recipeGenerator;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(UpdateRecipeCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var recipe = await _recipeGenerator.GetRecipe(request.RecipeId, user.Id);
                if (recipe == null)
                    throw new RestException(HttpStatusCode.NotFound, new { Recipe = "Not found" });

                if (await _recipeGenerator.IsRecipeExits(request.Title, user.Id))
                    throw new RestException(HttpStatusCode.BadRequest, new { Recipe_Title = "Already exist" });

                var updateRecipe = new Recipe
                {
                    Title = request.Title ?? recipe.Title,
                    Description = request.Description ?? recipe.Description,
                    SlugUrl = recipe.SlugUrl,
                    Category = request.Category,
                };

                var success = await _recipeGenerator.Update(user.Id, request.RecipeId, updateRecipe);
                if(success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }

    }
}