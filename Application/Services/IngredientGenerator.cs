using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Application.SqlClientSetup;
using Domain.Entities;

namespace Application.Services
{
    public class IngredientGenerator : IIngredientGenerator
    {
        private readonly IConnectionString _connection;
        private readonly IUserAuth _userAuth;
        public string conStr = string.Empty;
        public IngredientGenerator(IConnectionString connection, IUserAuth userAuth)
        {
            _connection = connection;
            _userAuth = userAuth;
            conStr = _connection.GetConnectionString();
        }
        public async Task<bool> Create(int userId, Ingredients ingredient)
        {
            string insertCommandText = @"INSERT INTO [dbo].[ingredients] (ingredient_name, ingredient_description, ingredient_slug, user_id)
                values (@name, @description, @slug, @userId)";

            SqlParameter ingredient_name = new SqlParameter("@name", ingredient.Name);
            SqlParameter ingredient_description = new SqlParameter("@description", ingredient.Description);
            SqlParameter ingredient_slug = new SqlParameter("@slug", ingredient.SlugUrl);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, insertCommandText, CommandType.Text,
                ingredient_name, ingredient_description, ingredient_slug, user_id);

            if (rows >= 1) return true;

            return false;
        }

        public async Task<IngredientDto> GetIngredient(int userId, int ingredientId)
        {
            var ingredient = new IngredientDto();
            string selectCommandText = "dbo.getIngredientByUserIdAndIngredientId";
            SqlParameter parameterUsername = new SqlParameter("@userId", SqlDbType.Int);
            parameterUsername.Value = userId;
            SqlParameter parameterIngredientId = new SqlParameter("@ingredientId", SqlDbType.Int);
            parameterIngredientId.Value = ingredientId;

            bool isIngredientExist = false;
            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.StoredProcedure, parameterUsername, parameterIngredientId))
            {

                while (reader.Read())
                {
                    isIngredientExist = true;
                    ingredient.Id = (int)reader["ingredient_id"];
                    ingredient.Name = (string)reader["ingredient_name"];
                    ingredient.Description = (string)reader["ingredient_description"];
                    ingredient.SlugUrl = (string)reader["ingredient_slug"];
                }
                await reader.CloseAsync();
            }
            return isIngredientExist ? ingredient : null;
        }

        public async Task<List<IngredientDto>> GetIngredients(int userId)
        {
            List<IngredientDto> ingredients = new List<IngredientDto>();

            string selectCommandText = "dbo.allIngredientByUserId";
            SqlParameter parameterUserId = new SqlParameter("@userId", SqlDbType.Int);
            parameterUserId.Value = userId;

            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.StoredProcedure, parameterUserId))
            {
                while (reader.Read())
                {
                    var ingredient = new IngredientDto
                    {
                        Id = (int)reader["ingredient_id"],
                        Name = (string)reader["ingredient_name"],
                        Description = (string)reader["ingredient_description"],
                        SlugUrl = (string)reader["ingredient_slug"],
                    };

                    ingredients.Add(ingredient);
                }
                await reader.CloseAsync();
            }
            return ingredients;
        }

        public async Task<bool> IsIngredientExitByName(string ingredientName, int userId, string slugUrl)
        {
            string commandText = @"SELECT Count([ingredient_name]) FROM [dbo].[ingredients] 
                WHERE (ingredient_name=@ingredientname AND user_Id=@userId) OR (ingredient_slug=@slugUrl AND user_Id=@userId)";

            SqlParameter parameterIngredientname = new SqlParameter("@ingredientname", SqlDbType.VarChar);
            parameterIngredientname.Value = ingredientName;
            SqlParameter parameterUserId = new SqlParameter("@userId", SqlDbType.Int);
            parameterUserId.Value = userId;
            SqlParameter parameterSlugUrl = new SqlParameter("@slugUrl", SqlDbType.NVarChar);
            parameterSlugUrl.Value = slugUrl;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                commandText,
                CommandType.Text,
                parameterIngredientname,
                parameterUserId,
                parameterSlugUrl);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }
        public async Task<bool> IsIngredientExitById(int ingredientId, int userId)
        {
            string commandText = @"SELECT Count([ingredient_name]) FROM [dbo].[ingredients] 
                WHERE ingredient_id=@ingredientId AND user_Id=@userId";

            SqlParameter parameterIngredientId = new SqlParameter("@ingredientId", SqlDbType.Int);
            parameterIngredientId.Value = ingredientId;
            SqlParameter parameterUserId = new SqlParameter("@userId", SqlDbType.Int);
            parameterUserId.Value = userId;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                commandText,
                CommandType.Text,
                parameterIngredientId,
               parameterUserId);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }

        public async Task<bool> Update(int userId, int ingredientId, IngredientDto ingredient)
        {
            string updateCommandText = @"UPDATE [dbo].[ingredients] SET ingredient_name = @name, 
                ingredient_description = @description WHERE ingredient_id = @ingredientId AND user_Id = @userId";

            SqlParameter ingredient_name = new SqlParameter("@name", ingredient.Name);
            SqlParameter ingredient_description = new SqlParameter("@description", ingredient.Description);
            SqlParameter ingredient_id = new SqlParameter("@ingredientId", ingredientId);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, updateCommandText, CommandType.Text,
                ingredient_name, ingredient_description, ingredient_id, user_id);

            if (rows >= 1) return true;

            return false;
        }
    }
}