using System;
using System.Collections.Generic;
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
    public class UpdateRecipe
    {

        public class UpdateIngredients
        {
            public int IngredientId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string SlugUrl { get; set; }
            public string Amount { get; set; }
            public bool IsNewIngredient { get; set; }

        }
        public class UpdateRecipeCommand : IRequest
        {
            public int RecipeId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public RecipeCategory Category { get; set; }
            public string Username { get; set; }
            public List<UpdateIngredients> UpdateIngredients { get; set; }
        }
        public class Handler : IRequestHandler<UpdateRecipeCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IRecipeGenerator _recipeGenerator;
            private readonly IIngredientGenerator _ingredientGenerator;
            private readonly IRecipeIngredientGenerator _recipeIngredientGenerator;

            public Handler(
                IUserAuth userAuth,
                IRecipeGenerator recipeGenerator,
                IIngredientGenerator ingredientGenerator,
                IRecipeIngredientGenerator recipeIngredientGenerator)
            {
                _recipeIngredientGenerator = recipeIngredientGenerator;
                _ingredientGenerator = ingredientGenerator;
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

                bool haveIngredient = false;
                if (request.UpdateIngredients != null && request.UpdateIngredients.Count > 0)
                {
                    haveIngredient = true;
                    foreach (var test in request.UpdateIngredients)
                    {
                        if (test.IsNewIngredient)
                        {
                            if (await _ingredientGenerator.IsIngredientExitByName(test.Name, user.Id, test.SlugUrl))
                                throw new RestException(HttpStatusCode.BadRequest, new { Ingredient_slug = "Already exist" });
                        }
                        else
                        {
                            if (!await _ingredientGenerator.IsIngredientExitById(test.IngredientId, user.Id))
                                throw new RestException(HttpStatusCode.NotFound, new { Ingredient = "Not found" });
                            //to check right combination of Ingredient and recipe
                            if (!await _recipeIngredientGenerator.IsIdsExitInRecipeIngredient(test.IngredientId, request.RecipeId))
                                throw new RestException(HttpStatusCode.NotFound, new { RecipeIngredient = "Not exist" });
                        }
                    }
                }

                var updateRecipe = new Recipe
                {
                    Title = request.Title ?? recipe.Title,
                    Description = request.Description ?? recipe.Description,
                    Category = request.Category,
                };

                var success = await _recipeGenerator.Update(user.Id, request.RecipeId, updateRecipe);
                if (!success) throw new Exception("Problem saving changes");

                if (haveIngredient)
                {
                    foreach (var updateIngredient in request.UpdateIngredients)
                    {
                        var ingredientIdentityId = 0;
                        if (updateIngredient.IsNewIngredient)
                        {
                            var createIngredient = new Domain.Entities.Ingredients
                            {
                                Name = updateIngredient.Name,
                                Description = updateIngredient.Description,
                                SlugUrl = updateIngredient.SlugUrl
                            };

                            ingredientIdentityId = await _ingredientGenerator.Create(user.Id, createIngredient);

                            bool recipeIngredient = await _recipeIngredientGenerator.Create(request.RecipeId, ingredientIdentityId, updateIngredient.Amount);
                            if (!recipeIngredient)
                                throw new Exception("Problem creating recipe ingredient list");
                        }
                        else
                        {
                            var ingredientFromDB = await _ingredientGenerator.GetIngredient(user.Id, updateIngredient.IngredientId);
                            if (ingredientFromDB != null)
                            {
                                var updateIngredent = new IngredientDto
                                {
                                    Name = updateIngredient.Name ?? ingredientFromDB.Name,
                                    Description = updateIngredient.Description ?? ingredientFromDB.Description
                                };

                                ingredientIdentityId = await _ingredientGenerator.Update(user.Id, updateIngredient.IngredientId, updateIngredent);
                                if (ingredientIdentityId <= 0) throw new Exception("Problem saving changes ingredients");
                            }


                            if (!string.IsNullOrWhiteSpace(updateIngredient.Amount))
                            {
                                var newMeasurementResult = await _recipeIngredientGenerator.Update(request.RecipeId, updateIngredient.IngredientId, updateIngredient.Amount);
                                if (newMeasurementResult <= 0) throw new Exception("Problem saving changes with new recipe list");
                            }
                        }
                    }
                }

                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }

    }
}