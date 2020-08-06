using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System.Linq;

namespace Application.Recipies
{
    public class GetRecipies
    {
        public class GetRecipiesQuery : IRequest<List<Recipe>>
        {
            public GetRecipiesQuery(string username, int? limit, int? offset)
            {
                Username = username;
                Limit = limit;
                Offset = offset;
            }
            public string Username { get; set; }
            public int? Limit { get; set; }
            public int? Offset { get; set; }
        }
        public class Handler : IRequestHandler<GetRecipiesQuery, List<Recipe>>
        {
            private readonly IUserAuth _userAuth;
            private readonly IRecipeGenerator _recipeGenerator;
            public Handler(IUserAuth userAuth, IRecipeGenerator recipeGenerator)
            {
                _recipeGenerator = recipeGenerator;
                _userAuth = userAuth;
            }

            public async Task<List<Recipe>> Handle(GetRecipiesQuery request,
                CancellationToken cancellationToken)
            {
                string selectCommandText = @"SELECT dbo.recipes.recipe_id, dbo.recipes.recipe_title, 
                dbo.recipes.recipe_description, dbo.recipes.recipe_slug, dbo.recipes.recipe_category,
	            dbo.ingredients.ingredient_id, dbo.ingredients.ingredient_name, dbo.ingredients.ingredient_description, 
	            dbo.ingredients.ingredient_slug, dbo.recipe_ingredients.amount
                FROM dbo.recipes
                JOIN dbo.recipe_ingredients ON dbo.recipes.recipe_id = dbo.recipe_ingredients.recipe_id 
                JOIN dbo.ingredients ON  dbo.recipe_ingredients.ingredient_id = dbo.ingredients.ingredient_id";

                int userId = 0;
                if (!string.IsNullOrEmpty(request.Username))
                {
                    var user = await _userAuth.GetUser(request.Username);
                    if (user == null)
                        throw new RestException(HttpStatusCode.NotFound, new { User = "Not found" });
                    userId = user.Id;
                    string.Concat(selectCommandText, "WHERE dbo.recipes.user_id = @userId");
                }

                string.Concat(selectCommandText, "ORDER BY dbo.recipe_ingredients.recipe_id");

                var recipesFromDB = await _recipeGenerator.GetRecipes(userId, selectCommandText);


                var recipes = recipesFromDB.Skip(request.Offset ?? 0)
                    .Take(request.Limit ?? 5).ToList();

                return recipes;
            }
        }
    }
}