using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Dishes
{
    public class CreateDish
    {
        public class Ingredients
        {
            public int IngredientId { get; set; }
            public string Amount { get; set; }
        }
        public class CreateDishCommand : IRequest
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string SlugUrl { get; set; }
            public int DishCategoryId { get; set; }
            public string Username { get; set; }
            public List<Ingredients> Ingredients { get; set; }
        }
        public class Handler : IRequestHandler<CreateDishCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IDishCategory _dishCategory;
            private readonly IDishGenerator _dishGenerator;
            private readonly IIngredientGenerator _ingredientGenerator;
            private readonly IRecipeIngredientGenerator _recipeIngredientGenerator;

            public Handler(
                IUserAuth userAuth,
                IDishCategory dishCategory,
                IDishGenerator dishGenerator,
                IIngredientGenerator ingredientGenerator,
                IRecipeIngredientGenerator recipeIngredientGenerator)
            {
                _ingredientGenerator = ingredientGenerator;
                _recipeIngredientGenerator = recipeIngredientGenerator;
                _dishGenerator = dishGenerator;
                _userAuth = userAuth;
                _dishCategory = dishCategory;
            }

            public async Task<Unit> Handle(CreateDishCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var dishCategory = await _dishCategory.GetDishCategory(request.DishCategoryId, user.Id);
                if (dishCategory == null)
                    throw new RestException(HttpStatusCode.NotFound, new { DishCategory = "Not found" });

                if (await _dishGenerator.IsDishExitsWithSlug(user.Id, request.SlugUrl))
                    throw new RestException(HttpStatusCode.BadRequest, new { Dish_Slug = "Already exist" });

                if (await _dishGenerator.IsDishExits(request.Title, user.Id))
                    throw new RestException(HttpStatusCode.BadRequest, new { DishTitle = "Already exist" });

                bool haveIngredients = false;
                if (request.Ingredients != null && request.Ingredients.Count > 0)
                {
                    haveIngredients = true;
                    foreach (var ingredient in request.Ingredients)
                    {
                        if (!await _ingredientGenerator.IsIngredientExitById(ingredient.IngredientId, user.Id))
                            throw new RestException(HttpStatusCode.NotFound, new { Ingredient = "Not found" });
                    }
                }
                var toCreateDish = new Dish
                {
                    Title = request.Title,
                    Description = request.Description,
                    SlugUrl = request.SlugUrl,
                    DishCategoryId = request.DishCategoryId,
                };

                var createdDish = await _dishGenerator.Create(user.Id, toCreateDish);

                if (haveIngredients)
                {
                    foreach (var ingredient in request.Ingredients)
                    {
                        bool dishIngredient = await _recipeIngredientGenerator.Create(createdDish.Id, ingredient.IngredientId, ingredient.Amount);
                        if (!dishIngredient)
                            throw new Exception("Problem adding Dish Ingredients");
                    }
                }

                return Unit.Value;

                throw new Exception("Problem creating dish");
            }
        }
    }
}