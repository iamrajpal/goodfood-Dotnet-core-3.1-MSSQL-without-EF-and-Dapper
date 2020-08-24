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
        }
        public class UpdateDishCommand : IRequest
        {
            public int DishId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int DishCategoryId { get; set; }
            public List<UpdateIngredients> UpdateIngredients { get; set; }
        }
        public class Handler : IRequestHandler<UpdateDishCommand>
        {
            private readonly IDishCategory _dishCategory;
            private readonly IUserAuth _userAuth;
            private readonly IDish _dish;
            private readonly IIngredient _ingredient;
            private readonly IRecipe _recipe;

            public Handler(
                IDishCategory dishCategory,
                IUserAuth userAuth,
                IDish dish,
                IIngredient ingredient,
                IRecipe recipe)
            {
                ;
                _ingredient = ingredient;
                _recipe = recipe;
                _dish = dish;
                _dishCategory = dishCategory;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(UpdateDishCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetCurrentUser();
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var dishCategory = await _dishCategory.GetDishCategory(request.DishCategoryId, user.Id);
                if (dishCategory == null)
                    throw new RestException(HttpStatusCode.NotFound, new { DishCategory = "Not found" });

                var dish = await _dish.GetDish(request.DishId, user.Id);
                if (dish == null)
                    throw new RestException(HttpStatusCode.NotFound, new { Dish = "Not found" });

                if (await _dish.IsDishExitsWithTitle(request.Title, user.Id))
                    throw new RestException(HttpStatusCode.BadRequest, new { DishTitle = "Already exist" });

                bool haveIngredient = false;
                if (request.UpdateIngredients != null && request.UpdateIngredients.Count > 0)
                {
                    haveIngredient = true;
                    foreach (var test in request.UpdateIngredients)
                    {

                        if (!await _ingredient.IsIngredientExitById(test.IngredientId, user.Id))
                            throw new RestException(HttpStatusCode.NotFound, new { Ingredient = "Not found" });
                        //to check right combination of Ingredient and Dish
                        if (!await _recipe.IsIdsExitInRecipeIngredient(test.IngredientId, request.DishId))
                            throw new RestException(HttpStatusCode.NotFound, new { DishIngredient = "Not exist" });

                    }
                }

                var updateDish = new Dish
                {
                    Title = request.Title ?? dish.Title,
                    Description = request.Description ?? dish.Description,
                    DishCategoryId = request.DishCategoryId,
                };

                var success = await _dish.Update(user.Id, request.DishId, updateDish);
                if (!success) throw new Exception("Problem saving changes");

                if (haveIngredient)
                {
                    foreach (var updateIngredient in request.UpdateIngredients)
                    {
                        var ingredientIdentityId = 0;

                        var ingredientFromDB = await _ingredient.GetIngredient(user.Id, updateIngredient.IngredientId);
                        if (ingredientFromDB != null)
                        {
                            var updateIngredent = new IngredientDto
                            {
                                Name = updateIngredient.Name ?? ingredientFromDB.Name,
                                Description = updateIngredient.Description ?? ingredientFromDB.Description
                            };

                            ingredientIdentityId = await _ingredient.Update(user.Id, updateIngredient.IngredientId, updateIngredent);
                            if (ingredientIdentityId <= 0) throw new Exception("Problem saving changes ingredients");
                        }

                        if (!string.IsNullOrWhiteSpace(updateIngredient.Amount))
                        {
                            var newMeasurementResult = await _recipe.Update(request.DishId, updateIngredient.IngredientId, updateIngredient.Amount);
                            if (newMeasurementResult <= 0) throw new Exception("Problem saving changes with new dish list");
                        }
                    }
                }

                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }

    }
}