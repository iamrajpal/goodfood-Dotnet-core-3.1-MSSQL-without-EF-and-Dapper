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
    public class CreateRecipe
    {
        public class Ingredients
        {
            public int IngredientId { get; set; }
            public string Amount { get; set; }
        }
        public class CreateRecipeCommand : IRequest<Recipe>
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string SlugUrl { get; set; }
            public RecipeCategory Category { get; set; }
            public string Username { get; set; }
            public List<Ingredients> Ingredients { get; set; }
        }
        public class Handler : IRequestHandler<CreateRecipeCommand, Recipe>
        {
            private readonly IUserAuth _userAuth;
            private readonly IRecipeGenerator _recipeGenerator;
            private readonly IIngredientGenerator _ingredientGenerator;
            private readonly IMeasurementGenerator _measurementGenerator;
            private readonly IRecipeIngredientGenerator _recipeIngredientGenerator;

            public Handler(
                IUserAuth userAuth,
                IRecipeGenerator recipeGenerator,
                IIngredientGenerator ingredientGenerator,
                IMeasurementGenerator measurementGenerator,
                IRecipeIngredientGenerator recipeIngredientGenerator)
            {
                _ingredientGenerator = ingredientGenerator;
                _measurementGenerator = measurementGenerator;
                _recipeIngredientGenerator = recipeIngredientGenerator;
                _recipeGenerator = recipeGenerator;
                _userAuth = userAuth;
            }

            public async Task<Recipe> Handle(CreateRecipeCommand request,
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

                if (request.Ingredients != null && request.Ingredients.Count > 0)
                {
                    foreach (var ingredient in request.Ingredients)
                    {
                        if (!await _ingredientGenerator.IsIngredientExitById(ingredient.IngredientId, user.Id))
                            throw new RestException(HttpStatusCode.NotFound, new { Ingredient = "Not found" });
                        int? measurementId = null;
                        if (!string.IsNullOrEmpty(ingredient.Amount))
                        {
                            measurementId = await _measurementGenerator.Create(ingredient.Amount);
                        }
                        bool recipeIngredient = await _recipeIngredientGenerator.Create(createdRecipe.Id, ingredient.IngredientId, measurementId);
                        if (!recipeIngredient)
                            throw new Exception("Problem creating recipe");
                    }
                }

                return createdRecipe;
            }
        }
    }
}