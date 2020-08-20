using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Dishes
{
    public class UpdateDish
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
        public class UpdateDishCommand : IRequest
        {
            public int DishId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int DishCategoryId { get; set; }
            public string Username { get; set; }
            public List<UpdateIngredients> UpdateIngredients { get; set; }
        }
        public class Handler : IRequestHandler<UpdateDishCommand>
        {
            private readonly IDishCategory _dishCategory;
            private readonly IUserAuth _userAuth;
            private readonly IDishGenerator _dishGenerator;
            private readonly IIngredientGenerator _ingredientGenerator;
            private readonly IRecipeIngredientGenerator _recipeIngredientGenerator;

            public Handler(
                IDishCategory dishCategory,
                IUserAuth userAuth,
                IDishGenerator dishGenerator,
                IIngredientGenerator ingredientGenerator,
                IRecipeIngredientGenerator recipeIngredientGenerator)
            {
                _recipeIngredientGenerator = recipeIngredientGenerator;
                _ingredientGenerator = ingredientGenerator;
                _dishGenerator = dishGenerator;
                _dishCategory = dishCategory;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(UpdateDishCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var dishCategory = await _dishCategory.GetDishCategory(request.DishCategoryId, user.Id);
                if (dishCategory == null)
                    throw new RestException(HttpStatusCode.NotFound, new { DishCategory = "Not found" });

                var dish = await _dishGenerator.GetDish(request.DishId, user.Id);
                if (dish == null)
                    throw new RestException(HttpStatusCode.NotFound, new { Dish = "Not found" });

                if (await _dishGenerator.IsDishExits(request.Title, user.Id))
                    throw new RestException(HttpStatusCode.BadRequest, new { DishTitle = "Already exist" });

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
                            //to check right combination of Ingredient and Dish
                            if (!await _recipeIngredientGenerator.IsIdsExitInRecipeIngredient(test.IngredientId, request.DishId))
                                throw new RestException(HttpStatusCode.NotFound, new { DishIngredient = "Not exist" });
                        }
                    }
                }

                var updateDish = new Dish
                {
                    Title = request.Title ?? dish.Title,
                    Description = request.Description ?? dish.Description,
                    DishCategoryId = request.DishCategoryId,
                };

                var success = await _dishGenerator.Update(user.Id, request.DishId, updateDish);
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

                            bool recipeIngredient = await _recipeIngredientGenerator.Create(request.DishId, ingredientIdentityId, updateIngredient.Amount);
                            if (!recipeIngredient)
                                throw new Exception("Problem creating dish ingredient list");
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
                                var newMeasurementResult = await _recipeIngredientGenerator.Update(request.DishId, updateIngredient.IngredientId, updateIngredient.Amount);
                                if (newMeasurementResult <= 0) throw new Exception("Problem saving changes with new dish list");
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