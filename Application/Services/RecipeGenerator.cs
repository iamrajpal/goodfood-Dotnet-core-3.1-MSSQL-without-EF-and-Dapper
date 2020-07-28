using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Application.SqlClientSetup;
using Domain.Enums;
using System.Collections.Generic;

namespace Application.Services
{
    public class RecipeGenerator : IRecipeGenerator
    {
        private readonly IConnectionString _connection;
        private readonly IUserAuth _userAuth;
        public string conStr = string.Empty;
        public RecipeGenerator(IConnectionString connection, IUserAuth userAuth)
        {
            _connection = connection;
            _userAuth = userAuth;
            conStr = _connection.GetConnectionString();
        }

        public async Task<bool> IsRecipeExits(string recipeTitle, int userId)
        {
            string commandText = @"SELECT Count([recipe_title]) FROM [dbo].[recipes] 
                WHERE recipe_title=@recipeTitle AND user_Id=@userId";
            SqlParameter recipe_title = new SqlParameter("@recipename", SqlDbType.VarChar);
            recipe_title.Value = recipeTitle;
            SqlParameter user_Id = new SqlParameter("@userId", SqlDbType.Int);
            user_Id.Value = userId;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                commandText,
                CommandType.Text,
                recipe_title,
                user_Id);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }

        public async Task<bool> IsRecipeExitsWithSlug(int userId, string recipeSlug)
        {
            string commandText = @"SELECT Count([recipe_title]) FROM [dbo].[recipes] 
                WHERE recipe_slug=@recipeSlug AND user_Id=@userId";

            SqlParameter user_Id = new SqlParameter("@userId", SqlDbType.Int);
            user_Id.Value = userId;
            SqlParameter recipe_slug = new SqlParameter("@recipeSlug", SqlDbType.NVarChar);
            recipe_slug.Value = recipeSlug;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                commandText,
                CommandType.Text,
                user_Id,
                recipe_slug);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }

        public async Task<Recipe> Create(int userId, Recipe recipe)
        {
            string insertCommandText = @"INSERT INTO [dbo].[recipes] (recipe_title, recipe_description, recipe_slug, recipe_category, user_id)
                OUTPUT INSERTED.recipe_id
                values (@recipeTitle, @recipeDescription, @recipeSlug, @recipeCategory, @userId)";

            SqlParameter recipe_title = new SqlParameter("@recipeTitle", recipe.Title);
            SqlParameter recipe_description = new SqlParameter("@recipeDescription", recipe.Description);
            SqlParameter recipe_slug = new SqlParameter("@recipeSlug", recipe.SlugUrl);
            SqlParameter recipe_category = new SqlParameter("@recipeCategory", recipe.Category);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            var identityId = await SqlHelper.ExecuteScalarAsync(conStr, insertCommandText, CommandType.Text,
                recipe_title, recipe_description, recipe_slug, recipe_category, user_id);
            if (identityId != null)
            {
                var recipeToReturn = new Recipe
                {
                    Id = (int)identityId,
                    Title = recipe.Title,
                    Description = recipe.Description,
                    SlugUrl = recipe.SlugUrl,
                    Category = recipe.Category
                };
                return recipeToReturn;
            }

            throw new Exception("Problem creating recipe");
        }

        public async Task<bool> Update(int userId, int recipeId, Recipe recipe)
        {
            string updateCommandText = @"UPDATE [dbo].[recipes] SET recipe_title = @recipeTitle, 
                recipe_description = @recipeDescription, recipe_category = @recipeCategory 
                WHERE recipe_id = @recipeId AND user_Id = @userId";

            SqlParameter recipe_title = new SqlParameter("@recipeTitle", recipe.Title);
            SqlParameter recipe_description = new SqlParameter("@recipeDescription", recipe.Description);
            SqlParameter recipe_category = new SqlParameter("@recipeCategory", recipe.Category);
            SqlParameter recipe_id = new SqlParameter("@recipeId", recipeId);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, updateCommandText, CommandType.Text,
                recipe_title, recipe_description, recipe_category, recipe_id, user_id);

            if (rows >= 1) return true;

            return false;
        }

        public async Task<Recipe> GetRecipe(int recipeId, int userId)
        {
            var recipe = new Recipe();
            bool isRecipeExist = false;
            string selectCommandText = "dbo.getRecipeByUserId";
            SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
            user_id.Value = userId;
            SqlParameter recipe_id = new SqlParameter("@recipeId", SqlDbType.Int);
            recipe_id.Value = recipeId;

            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.StoredProcedure, user_id, recipe_id))
            {
                while (reader.Read())
                {
                    isRecipeExist = true;
                    recipe.Title = reader["recipe_title"].ToString();
                    recipe.Description = reader["recipe_description"].ToString();
                    recipe.SlugUrl = reader["recipe_slug"].ToString();
                    recipe.Category = (RecipeCategory)Enum.Parse(typeof(RecipeCategory), reader["recipe_category"].ToString());
                }
                await reader.CloseAsync();
            }
            return isRecipeExist ? recipe : null;
        }

        public async Task<bool> Delete(int userId, List<int> recipeIds)
        {
            bool checkDeleteStatus = false;

            foreach (var recipeId in recipeIds)
            {
                checkDeleteStatus = false;
                string DELETECommandText = @"DELETE FROM [dbo].[recipes] WHERE user_id = @userId AND recipe_id = @recipeId";

                SqlParameter recipe_id = new SqlParameter("@recipeId", recipeId);
                SqlParameter user_id = new SqlParameter("@userId", userId);

                Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, DELETECommandText, CommandType.Text, recipe_id, user_id);

                if (rows >= 1) checkDeleteStatus = true;
            }

            return checkDeleteStatus;
        }

        public async Task<List<Recipe>> GetRecipes(int userId, string commandText)
        {
            List<Recipe> recipes = new List<Recipe>();
            bool isRecipeExist = false;          
            SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
            user_id.Value = userId;

            int preRecipeId = -1;
            int newRecipeId = -10;

            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, commandText,
                CommandType.Text, user_id))
            {
                var recipe = new Recipe();
                bool start = true;
                while (reader.Read())
                {
                    newRecipeId = (int)reader["recipe_id"];
                    if (start)
                    {
                        recipe.Id = (int)reader["recipe_id"];
                        recipe.Title = reader["recipe_title"].ToString();
                        recipe.Description = reader["recipe_description"].ToString();
                        recipe.SlugUrl = reader["recipe_slug"].ToString();
                        recipe.Category = (RecipeCategory)Enum.Parse(typeof(RecipeCategory), reader["recipe_category"].ToString());
                        var Ingredient = new Ingredients
                        {
                            Id = (int)reader["ingredient_id"],
                            Name = (string)reader["ingredient_name"],
                            Description = (string)reader["ingredient_description"],
                            SlugUrl = (string)reader["ingredient_slug"],
                            Amount = (string)reader["amount"],
                        };
                        recipe.Ingredients.Add(Ingredient);
                    }
                    if (newRecipeId == preRecipeId)
                    {
                        var Ingredient = new Ingredients
                        {
                            Id = (int)reader["ingredient_id"],
                            Name = (string)reader["ingredient_name"],
                            Description = (string)reader["ingredient_description"],
                            SlugUrl = (string)reader["ingredient_slug"],
                            Amount = (string)reader["amount"],
                        };
                        recipe.Ingredients.Add(Ingredient);
                    }
                    if (newRecipeId != preRecipeId && !start)
                    {
                        if (recipe != null)
                        {
                            recipes.Add(recipe);
                        }
                        recipe = new Recipe();
                        recipe.Id = (int)reader["recipe_id"];
                        recipe.Title = reader["recipe_title"].ToString();
                        recipe.Description = reader["recipe_description"].ToString();
                        recipe.SlugUrl = reader["recipe_slug"].ToString();
                        recipe.Category = (RecipeCategory)Enum.Parse(typeof(RecipeCategory), reader["recipe_category"].ToString());
                        var Ingredient = new Ingredients
                        {
                            Id = (int)reader["ingredient_id"],
                            Name = (string)reader["ingredient_name"],
                            Description = (string)reader["ingredient_description"],
                            SlugUrl = (string)reader["ingredient_slug"],
                            Amount = (string)reader["amount"],
                        };
                        recipe.Ingredients.Add(Ingredient);
                    }
                    isRecipeExist = true;
                    start = false;
                    preRecipeId = (int)reader["recipe_id"];
                }

                await reader.CloseAsync();

            }
            return isRecipeExist ? recipes : null;
        }
    }
}