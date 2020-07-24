using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Recipies
{
    public class CreateRecipe
    {
        public class CreateRecipeCommand : IRequest<RecipeDto>
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string SlugUrl { get; set; }
            public RecipeCategory Category { get; set; }
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<CreateRecipeCommand, RecipeDto>
        {
            private readonly IUserAuth _userAuth;
            private readonly IRecipeGenerator _recipeGenerator;
            public Handler(IUserAuth userAuth, IRecipeGenerator recipeGenerator)
            {
                _recipeGenerator = recipeGenerator;
                _userAuth = userAuth;
            }

            public async Task<RecipeDto> Handle(CreateRecipeCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                if (await _recipeGenerator.IsRecipeExitsWithSlug(request.Title, user.Id, request.SlugUrl))
                    throw new RestException(HttpStatusCode.BadRequest, new { Recipe_Slug = "Already exist" });

                var toCreateRecipe = new Recipe
                {
                    Title = request.Title,
                    Description = request.Description,
                    SlugUrl = request.SlugUrl,
                    Category = request.Category,                    
                };

                var createdRecipe = await _recipeGenerator.Create(user.Id, toCreateRecipe);
                
                return createdRecipe;
            }
        }
    }
}