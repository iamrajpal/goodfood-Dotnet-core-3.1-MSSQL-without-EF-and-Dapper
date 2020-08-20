using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Application.SqlClientSetup;
using System.Collections.Generic;

namespace Application.Services
{
    public class DishGenerator : IDishGenerator
    {
        private readonly IConnectionString _connection;
        private readonly IUserAuth _userAuth;
        public string conStr = string.Empty;
        public DishGenerator(IConnectionString connection, IUserAuth userAuth)
        {
            _connection = connection;
            _userAuth = userAuth;
            conStr = _connection.GetConnectionString();
        }

        public async Task<bool> IsDishExits(string dishTitle, int userId)
        {
            string selectCommandText = @"SELECT Count([DishTitle]) 
                FROM 
                    Dish 
                WHERE 
                    DishTitle=@dishTitle AND UserId=@userId";

            SqlParameter dish_title = new SqlParameter("@dishTitle", SqlDbType.VarChar);
            dish_title.Value = dishTitle;
            SqlParameter user_Id = new SqlParameter("@userId", SqlDbType.Int);
            user_Id.Value = userId;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                selectCommandText,
                CommandType.Text,
                dish_title,
                user_Id);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }

        public async Task<bool> IsDishExitsWithSlug(int userId, string dishSlug)
        {
            string selectCommandText = @"SELECT Count([DishTitle]) 
                FROM 
                    Dish 
                WHERE 
                    DishSlug=@dishSlug AND UserId=@userId";

            SqlParameter user_Id = new SqlParameter("@userId", SqlDbType.Int);
            user_Id.Value = userId;
            SqlParameter recipe_slug = new SqlParameter("@dishSlug", SqlDbType.NVarChar);
            recipe_slug.Value = dishSlug;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                selectCommandText,
                CommandType.Text,
                user_Id,
                recipe_slug);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }

        public async Task<Dish> Create(int userId, Dish dish)
        {
            string insertCommandText = @"INSERT 
                INTO 
                    Dish(DishTitle, DishDescription, DishSlug, DishCategoryId, UserId)
                OUTPUT 
                    INSERTED.DishId
                values 
                    (@dishTitle, @dishDescription, @dishSlug, @dishCategoryId, @userId)";

            SqlParameter dish_title = new SqlParameter("@dishTitle", dish.Title);
            SqlParameter dish_description = new SqlParameter("@dishDescription", dish.Description);
            SqlParameter dish_slug = new SqlParameter("@dishSlug", dish.SlugUrl);
            SqlParameter dish_category_id = new SqlParameter("@dishCategoryId", dish.DishCategoryId);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            var identityId = await SqlHelper.ExecuteScalarAsync(conStr, insertCommandText, CommandType.Text,
                dish_title, dish_description, dish_slug, dish_category_id, user_id);
            if (identityId != null)
            {
                var dishToReturn = new Dish
                {
                    Id = (int)identityId,
                    Title = dish.Title,
                    Description = dish.Description,
                    SlugUrl = dish.SlugUrl,
                    DishCategoryId = dish.DishCategoryId
                };
                return dishToReturn;
            }

            throw new Exception("Problem creating dish");
        }

        public async Task<bool> Update(int userId, int dishId, Dish dish)
        {
            string updateCommandText = @"UPDATE 
                    Dish 
                SET 
                    DishTitle = @dishTitle, DishDescription = @dishDescription, DishCategoryId = @dishCategoryId 
                WHERE 
                    DishId = @dishId AND UserId = @userId";

            SqlParameter dish_title = new SqlParameter("@dishTitle", dish.Title);
            SqlParameter dish_description = new SqlParameter("@dishDescription", dish.Description);
            SqlParameter dish_category_id = new SqlParameter("@dishCategoryId", dish.DishCategoryId);
            SqlParameter dish_id = new SqlParameter("@dishId", dishId);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, updateCommandText, CommandType.Text,
                dish_title, dish_description, dish_category_id, dish_id, user_id);

            if (rows >= 1) return true;

            return false;
        }

        public async Task<Dish> GetDish(int dishId, int userId)
        {
            var dish = new Dish();
            bool isDishExist = false;
            string selectCommandText = @"SELECT 
                    *
                FROM
                    Dish
                WHERE
                    UserId=@userId ANd DishId=@dishId";

            SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
            user_id.Value = userId;
            SqlParameter dish_id = new SqlParameter("@dishId", SqlDbType.Int);
            dish_id.Value = dishId;

            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.Text, user_id, dish_id))
            {
                while (reader.Read())
                {
                    isDishExist = true;
                    dish.Title = (string)reader["DishTitle"];
                    dish.Description = (string)reader["DishDescription"];
                    dish.SlugUrl = (string)reader["DishSlug"];
                    dish.DishCategoryId = (int)reader["DishCategoryId"];
                }
                await reader.CloseAsync();
            }
            return isDishExist ? dish : null;
        }

        public async Task<bool> Delete(int userId, List<int> dishIds)
        {
            bool checkDeleteStatus = false;

            foreach (var dishId in dishIds)
            {
                checkDeleteStatus = false;
                string deleteCommandText = @"DELETE 
                    FROM
                        Dish
                    WHERE 
                        UserId = @userId AND DishId = @dishId";

                SqlParameter dish_id = new SqlParameter("@dishId", dishId);
                SqlParameter user_id = new SqlParameter("@userId", userId);

                Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, deleteCommandText, CommandType.Text, dish_id, user_id);

                if (rows >= 1) checkDeleteStatus = true;
            }

            return checkDeleteStatus;
        }

        public async Task<List<Dish>> GetDishes(int userId, string selectCommandText)
        {
            List<Dish> dishes = new List<Dish>();
            bool isDishExits = false;

            SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
            user_id.Value = userId;

            int preDishId = -1;
            int newDishId = -10;

            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.Text, user_id))
            {
                var dish = new Dish();
                bool start = true;
                while (reader.Read())
                {
                    newDishId = (int)reader["DishId"];
                    if (start)
                    {
                        dish.Id = (int)reader["DishId"];
                        dish.Title = (string)reader["dish_title"];
                        dish.Description = (string)reader["dish_description"];
                        dish.SlugUrl = (string)reader["dish_slug"];
                        dish.DishCategoryId = (int)reader["DishCategoryId"];
                        var Ingredient = new Ingredients
                        {
                            Id = (int)reader["IngredientId"],
                            Name = (string)reader["IngredientName"],
                            Description = (string)reader["IngredientDescription"],
                            SlugUrl = (string)reader["IngredientSlug"],
                            Amount = (string)reader["amount"],
                        };
                        dish.Ingredients.Add(Ingredient);
                    }
                    if (newDishId == preDishId)
                    {
                        var Ingredient = new Ingredients
                        {
                            Id = (int)reader["IngredientId"],
                            Name = (string)reader["IngredientName"],
                            Description = (string)reader["IngredientDescription"],
                            SlugUrl = (string)reader["IngredientSlug"],
                            Amount = (string)reader["amount"],
                        };
                        dish.Ingredients.Add(Ingredient);
                    }
                    if (newDishId != preDishId && !start)
                    {
                        if (dish != null)
                        {
                            dishes.Add(dish);
                        }
                        dish = new Dish();
                        dish.Id = (int)reader["DishId"];
                        dish.Title = (string)reader["DishTitle"];
                        dish.Description = (string)reader["DishDescription"];
                        dish.SlugUrl = (string)reader["DishSlug"];
                        dish.DishCategoryId = (int)reader["DishCategoryId"];
                        var Ingredient = new Ingredients
                        {
                            Id = (int)reader["IngredientId"],
                            Name = (string)reader["IngredientName"],
                            Description = (string)reader["IngredientDescription"],
                            SlugUrl = (string)reader["IngredientSlug"],
                            Amount = (string)reader["amount"],
                        };
                        dish.Ingredients.Add(Ingredient);
                    }
                    isDishExits = true;
                    start = false;
                    preDishId = (int)reader["DishId"];
                }

                await reader.CloseAsync();

            }
            return isDishExits ? dishes : null;
        }

        public async Task<bool> IsDishCategoryUsedByDish(int dishCateGoryid)
        {
            string selectCommandText = @"SELECT Count([DishTitle]) 
                FROM 
                    Dish 
                WHERE 
                    DishCategoryId=@dishCategoryId";

            SqlParameter dish_category_id = new SqlParameter("@dishCategoryId", SqlDbType.Int);
            dish_category_id.Value = dishCateGoryid;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                selectCommandText,
                CommandType.Text,
                dish_category_id);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }

        // public async Task<List<Recipe>> GetRecipes(int userId, string selectCommandText)
        // {
        //     List<Recipe> recipes = new List<Recipe>();
        //     bool isRecipeExist = false;
        //     SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
        //     user_id.Value = userId;

        //     int preRecipeId = -1;
        //     int newRecipeId = -10;

        //     using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
        //         CommandType.Text, user_id))
        //     {
        //         var recipe = new Recipe();
        //         bool start = true;
        //         while (reader.Read())
        //         {
        //             newRecipeId = (int)reader["recipe_id"];
        //             if (start)
        //             {
        //                 recipe.Id = (int)reader["recipe_id"];
        //                 recipe.Title = reader["recipe_title"].ToString();
        //                 recipe.Description = reader["recipe_description"].ToString();
        //                 recipe.SlugUrl = reader["recipe_slug"].ToString();
        //                 recipe.Category = (RecipeCategory)Enum.Parse(typeof(RecipeCategory), reader["recipe_category"].ToString());
        //                 var Ingredient = new Ingredients
        //                 {
        //                     Id = (int)reader["ingredient_id"],
        //                     Name = (string)reader["ingredient_name"],
        //                     Description = (string)reader["ingredient_description"],
        //                     SlugUrl = (string)reader["ingredient_slug"],
        //                     Amount = (string)reader["amount"],
        //                 };
        //                 recipe.Ingredients.Add(Ingredient);
        //             }
        //             if (newRecipeId == preRecipeId)
        //             {
        //                 var Ingredient = new Ingredients
        //                 {
        //                     Id = (int)reader["ingredient_id"],
        //                     Name = (string)reader["ingredient_name"],
        //                     Description = (string)reader["ingredient_description"],
        //                     SlugUrl = (string)reader["ingredient_slug"],
        //                     Amount = (string)reader["amount"],
        //                 };
        //                 recipe.Ingredients.Add(Ingredient);
        //             }
        //             if (newRecipeId != preRecipeId && !start)
        //             {
        //                 if (recipe != null)
        //                 {
        //                     recipes.Add(recipe);
        //                 }
        //                 recipe = new Recipe();
        //                 recipe.Id = (int)reader["recipe_id"];
        //                 recipe.Title = reader["recipe_title"].ToString();
        //                 recipe.Description = reader["recipe_description"].ToString();
        //                 recipe.SlugUrl = reader["recipe_slug"].ToString();
        //                 recipe.Category = (RecipeCategory)Enum.Parse(typeof(RecipeCategory), reader["recipe_category"].ToString());
        //                 var Ingredient = new Ingredients
        //                 {
        //                     Id = (int)reader["ingredient_id"],
        //                     Name = (string)reader["ingredient_name"],
        //                     Description = (string)reader["ingredient_description"],
        //                     SlugUrl = (string)reader["ingredient_slug"],
        //                     Amount = (string)reader["amount"],
        //                 };
        //                 recipe.Ingredients.Add(Ingredient);
        //             }
        //             isRecipeExist = true;
        //             start = false;
        //             preRecipeId = (int)reader["recipe_id"];
        //         }

        //         await reader.CloseAsync();

        //     }
        //     return isRecipeExist ? recipes : null;
        // }
    }
}