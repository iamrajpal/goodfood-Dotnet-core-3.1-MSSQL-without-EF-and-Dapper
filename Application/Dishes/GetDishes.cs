using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System.Linq;

namespace Application.Dishes
{
    public class GetDishes
    {
        public class GetDishesQuery : IRequest<List<Dish>>
        {
            public GetDishesQuery(string username, int? limit, int? offset)
            {
                Username = username;
                Limit = limit;
                Offset = offset;
            }
            public string Username { get; set; }
            public int? Limit { get; set; }
            public int? Offset { get; set; }
        }
        public class Handler : IRequestHandler<GetDishesQuery, List<Dish>>
        {
            private readonly IUserAuth _userAuth;
            private readonly IDishGenerator _dishGenerator;
            public Handler(IUserAuth userAuth, IDishGenerator dishGenerator)
            {
                _dishGenerator = dishGenerator;
                _userAuth = userAuth;
            }

            public async Task<List<Dish>> Handle(GetDishesQuery request,
                CancellationToken cancellationToken)
            {
                // string selectCommandText = @"SELECT Dish.DishId, Dish.DishTitle, 
                // Dish.Description, Dish.DishSlug, Dish.RecipeCategory,
                // dbo.ingredients.ingredient_id, dbo.ingredients.ingredient_name, dbo.ingredients.ingredient_description, 
                // dbo.ingredients.ingredient_slug, dbo.recipe_ingredients.amount
                // FROM dbo.recipes
                // JOIN dbo.recipe_ingredients ON dbo.recipes.recipe_id = dbo.recipe_ingredients.recipe_id 
                // JOIN dbo.ingredients ON  dbo.recipe_ingredients.ingredient_id = dbo.ingredients.ingredient_id";

                string selectCommandText = @"SELECT Dish.DishId, Dish.DishTitle, 
                Dish.DishDescription, Dish.DishSlug, Dish.DishCategoryId,
	            Ingredients.IngredientId, Ingredients.IngredientName, Ingredients.IngredientDescription, 
	            Ingredients.IngredientSlug, Recipe.amount
                FROM Dish
                JOIN Recipe ON Dish.DishId = Recipe.DishId 
                JOIN Ingredients ON  Recipe.IngredientId = Ingredients.IngredientId";

                int userId = 0;
                if (!string.IsNullOrEmpty(request.Username))
                {
                    var user = await _userAuth.GetUser(request.Username);
                    if (user == null)
                        throw new RestException(HttpStatusCode.NotFound, new { User = "Not found" });
                    userId = user.Id;
                    string.Concat(selectCommandText, "WHERE Dish.UserId = @userId");
                }

                string.Concat(selectCommandText, "ORDER BY Dish.DishId");

                var dishesFromDB = await _dishGenerator.GetDishes(userId, selectCommandText);

                var dishes = dishesFromDB != null
                    ? dishesFromDB.Skip(request.Offset ?? 0)
                    .Take(request.Limit ?? 5).ToList() : null;

                return dishes;
            }
        }
    }
}