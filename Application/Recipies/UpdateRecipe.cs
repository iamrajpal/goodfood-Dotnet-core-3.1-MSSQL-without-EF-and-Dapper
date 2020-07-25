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
        public class UpdateMeasurement
        {
            public int MeasurementId { get; set; }
            public string Amount { get; set; }
        }
        public class UpdateIngredients
        {
            public int IngredientId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string SlugUrl { get; set; }
            public bool IsNewIngredient { get; set; }
            public UpdateMeasurement Measurement { get; set; }
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
            private readonly IMeasurementGenerator _measurementGenerator;
            private readonly IRecipeIngredientGenerator _recipeIngredientGenerator;

            public Handler(
                IUserAuth userAuth,
                IRecipeGenerator recipeGenerator,
                IIngredientGenerator ingredientGenerator,
                IMeasurementGenerator measurementGenerator,
                IRecipeIngredientGenerator recipeIngredientGenerator)
            {
                _measurementGenerator = measurementGenerator;
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

                bool haveIngredients = false;
                if (request.UpdateIngredients != null && request.UpdateIngredients.Count > 0)
                {
                    haveIngredients = true;
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
                            if (test.IngredientId > 0 && test.Measurement != null && test.Measurement.MeasurementId > 0)
                            {
                                if (!await _recipeIngredientGenerator.IsIdsExitInRecipeIngredient(test.IngredientId, request.RecipeId, test.Measurement.MeasurementId))
                                    throw new RestException(HttpStatusCode.NotFound, new { RecipeIngredient = "Not exist" });
                            }
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

                if (haveIngredients)
                {
                    foreach (var updateIngredient in request.UpdateIngredients)
                    {
                        var ingredientIdentityId = 0;
                        int? measurementIdentityId = null;

                        if (updateIngredient.IsNewIngredient)
                        {
                            var createIngredient = new Domain.Entities.Ingredients
                            {
                                Name = updateIngredient.Name,
                                Description = updateIngredient.Description,
                                SlugUrl = updateIngredient.SlugUrl
                            };

                            ingredientIdentityId = await _ingredientGenerator.Create(user.Id, createIngredient);
                            if (updateIngredient.Measurement != null)
                            {
                                measurementIdentityId = await _measurementGenerator.Create(updateIngredient.Measurement.Amount);
                                if (measurementIdentityId <= 0) throw new Exception("Problem saving changes with new measurement");
                            }

                            bool recipeIngredient = await _recipeIngredientGenerator.Create(request.RecipeId, ingredientIdentityId, measurementIdentityId);
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
                            }
                            if (ingredientIdentityId <= 0) throw new Exception("Problem saving changes ingredients");

                            if (updateIngredient.Measurement != null && !string.IsNullOrWhiteSpace(updateIngredient.Measurement.Amount))
                            {
                                var measurementFromDB = await _measurementGenerator.GetMeasurement(updateIngredient.Measurement.MeasurementId);
                                if (measurementFromDB != null)
                                {
                                    var amount = updateIngredient.Measurement.Amount ?? measurementFromDB.Amount;
                                    var isMeasurementSuccess = await _measurementGenerator.Update(updateIngredient.Measurement.MeasurementId, amount);
                                    if (!isMeasurementSuccess) throw new Exception("Problem saving changes with measurement");
                                }
                                else
                                {

                                    var newMeasurementIdentityId = await _measurementGenerator.Create(updateIngredient.Measurement.Amount);
                                    if (newMeasurementIdentityId <= 0) throw new Exception("Problem saving changes with new measurement");

                                    var newMeasurementResult = await _recipeIngredientGenerator.Update(request.RecipeId, updateIngredient.IngredientId, newMeasurementIdentityId);
                                    if (newMeasurementResult <= 0) throw new Exception("Problem saving changes with new recipe list");
                                }

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