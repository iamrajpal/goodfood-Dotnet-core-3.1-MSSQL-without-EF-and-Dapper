using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using MediatR;

namespace Application.Dishes
{
    public class AddIngredient
    {
        public class NewIngredients
        {
            public int IngredientId { get; set; }
            public string Amount { get; set; }
        }
        public class AddIngredientCommand : IRequest
        {
            public int DishId { get; set; }
            public List<NewIngredients> NewIngredients { get; set; }
        }
        public class Handler : IRequestHandler<AddIngredientCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IDish _dish;
            private readonly IIngredient _ingredient;
            private readonly IRecipe _recipe;

            public Handler(
                IUserAuth userAuth,
                IDish dish,
                IIngredient ingredient,
                IRecipe recipe)
            {
                _ingredient = ingredient;
                _recipe = recipe; ;
                _dish = dish;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(AddIngredientCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetCurrentUser();
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });
                if(request.DishId < 1)
                    throw new RestException(HttpStatusCode.BadRequest, new { Dish = "Bad request" });

                bool haveIngredients = false;
                if (request.NewIngredients != null && request.NewIngredients.Count > 0)
                {
                    haveIngredients = true;
                    foreach (var ingredient in request.NewIngredients)
                    {
                        if (!await _ingredient.IsIngredientExitById(ingredient.IngredientId, user.Id))
                            throw new RestException(HttpStatusCode.NotFound, new { Ingredient = "Not found" });
                        //Check wheather already exist in Reipe
                        if (await _recipe.IsIdsExitInRecipeIngredient(ingredient.IngredientId, request.DishId))
                            throw new RestException(HttpStatusCode.BadRequest, new { Ingredient = "Already exist" });
                    }
                }

                var dish = await _dish.GetDish(request.DishId, user.Id);
                if(dish == null)
                    throw new RestException(HttpStatusCode.NotFound, new { Dish = "Not found" });

                if (haveIngredients)
                {
                    foreach (var ingredient in request.NewIngredients)
                    {
                        bool dishIngredient = await _recipe.Create(request.DishId, ingredient.IngredientId, ingredient.Amount);
                        if (!dishIngredient)
                            throw new Exception("Problem adding Dish Ingredients");
                    }
                }

                return Unit.Value;

                throw new Exception("Problem adding dish ingredients");
            }
        }
    }
}