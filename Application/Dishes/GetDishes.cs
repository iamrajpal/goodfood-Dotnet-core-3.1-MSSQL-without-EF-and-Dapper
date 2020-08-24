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
            private readonly IDish _dish;
            private readonly IIngredient _ingredient;
            public Handler(IUserAuth userAuth, IDish dish, IIngredient ingredient)
            {
                _ingredient = ingredient;
                _dish = dish;
                _userAuth = userAuth;
            }

            public async Task<List<Dish>> Handle(GetDishesQuery request,
                CancellationToken cancellationToken)
            {
                string selectCommandText = @"SELECT Dish.DishId, Dish.DishTitle, 
                Dish.DishDescription, Dish.DishSlug, DishCategory.DishCategoryId, DishCategory.DishCategoryTitle
                FROM Dish JOIN DishCategory ON Dish.DishCategoryId = DishCategory.DishCategoryId";

                int userId = 0;
                if (!string.IsNullOrEmpty(request.Username))
                {
                    var user = await _userAuth.GetUser(request.Username);
                    if (user == null)
                        throw new RestException(HttpStatusCode.NotFound, new { User = "Not found" });

                    userId = user.Id;
                    selectCommandText += @" WHERE Dish.UserId = @userId";
                }

                selectCommandText += @" ORDER BY Dish.DishId
                OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";



                var dishesFromDB = await _dish.GetDishes(userId, selectCommandText, request.Offset, request.Limit);
                if (dishesFromDB.Count > 0)
                {
                    selectCommandText = @"SELECT Ingredients.IngredientId, Ingredients.IngredientName, Ingredients.IngredientDescription, 
                    Ingredients.IngredientSlug, Recipe.Amount
                    FROM Recipe
                    JOIN Ingredients ON Recipe.IngredientId = Ingredients.IngredientId
                    WHERE Recipe.DishId = @dishId";

                    foreach (var dish in dishesFromDB)
                    {
                        dish.Ingredients = await _ingredient.GetIngredientsByDishId(dish.Id, selectCommandText);
                    }
                }


                return dishesFromDB;
            }
        }
    }
}