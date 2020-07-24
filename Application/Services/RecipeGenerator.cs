using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Application.SqlClientSetup;
using Application.Errors;
using System.Net;

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

        public async Task<bool> IsRecipeExits(string recipename, int userId, string recipeSlug)
        {
            string commandText = @"SELECT Count([recipe_title]) FROM [dbo].[recipes] 
                WHERE (recipe_title=@recipename AND user_Id=@userId) OR (recipe_slug=@recipeSlug AND user_Id=@userId)";
            SqlParameter parameterRecipename = new SqlParameter("@recipename", SqlDbType.VarChar);
            parameterRecipename.Value = recipename;
            SqlParameter parameterUserId = new SqlParameter("@userId", SqlDbType.Int);
            parameterUserId.Value = userId;            
            SqlParameter parameterRecipeSlug = new SqlParameter("@recipeSlug", SqlDbType.NVarChar);
            parameterRecipeSlug.Value = recipeSlug;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                commandText,
                CommandType.Text,
                parameterRecipename,
                parameterUserId,
                parameterRecipeSlug);
                
            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }
        
        public async Task<RecipeDto> Create(int userId, Recipe recipe)
        {
            string insertCommandText = @"INSERT INTO [dbo].[recipes] (recipe_title, recipe_description, recipe_slug, recipe_category, user_id)
                values (@recipeTitle, @recipeDescription, @recipeSlug, @recipeCategory, @userId)";

            SqlParameter recipe_title = new SqlParameter("@recipeTitle", recipe.Title);
            SqlParameter recipe_description = new SqlParameter("@recipeDescription", recipe.Description);
            SqlParameter recipe_slug = new SqlParameter("@recipeSlug", recipe.SlugUrl);
            SqlParameter recipe_category = new SqlParameter("@recipeCategory", recipe.Category);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, insertCommandText, CommandType.Text,
                recipe_title, recipe_description, recipe_slug, recipe_category, user_id);
            if (rows >= 1)
            {
                var recipeToReturn = new RecipeDto
                {
                    Title = recipe.Title,
                    Description = recipe.Description,
                    Url = recipe.SlugUrl,
                    Category = recipe.Category.ToString()                    
                };
                return recipeToReturn;
            }

            throw new Exception("Problem creating recipe");
        }
    }
}