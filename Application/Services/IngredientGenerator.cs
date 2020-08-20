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
        public async Task<int> Create(int userId, Ingredients ingredient)
        {
            string insertCommandText = @"INSERT 
                INTO Ingredients (IngredientName, IngredientDescription, IngredientSlug, UserId)
                OUTPUT INSERTED.IngredientId
                values (@name, @description, @slug, @userId)";

            SqlParameter ingredient_name = new SqlParameter("@name", ingredient.Name);
            SqlParameter ingredient_description = new SqlParameter("@description", ingredient.Description);
            SqlParameter ingredient_slug = new SqlParameter("@slug", ingredient.SlugUrl);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            var identityId = await SqlHelper.ExecuteScalarAsync(conStr, insertCommandText, CommandType.Text,
                ingredient_name, ingredient_description, ingredient_slug, user_id);

            return (int)(identityId != null ? identityId : null);

            throw new Exception("Problem saving changes");
        }

        public async Task<IngredientDto> GetIngredient(int userId, int ingredientId)
        {
            var ingredient = new IngredientDto();
            // string selectCommandText = "dbo.getIngredientByUserIdAndIngredientId";
            string selectCommandText = @"SELECT 
            IngredientId, IngredientName, IngredientDescription, IngredientSlug 
            FROM Ingredients
            WHERE UserId=@userId AND IngredientId=@ingredientId";

            SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
            user_id.Value = userId;
            SqlParameter ingredient_id = new SqlParameter("@ingredientId", SqlDbType.Int);
            ingredient_id.Value = ingredientId;

            bool isIngredientExist = false;
            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.Text, user_id, ingredient_id))
            {

                while (reader.Read())
                {
                    isIngredientExist = true;
                    ingredient.Id = (int)reader["IngredientId"];
                    ingredient.Name = (string)reader["IngredientName"];
                    ingredient.Description = (string)reader["IngredientDescription"];
                    ingredient.SlugUrl = (string)reader["IngredientSlug"];
                }
                await reader.CloseAsync();
            }
            return isIngredientExist ? ingredient : null;
        }

        public async Task<List<IngredientDto>> GetIngredients(int userId)
        {
            List<IngredientDto> ingredients = new List<IngredientDto>();

            string selectCommandText = @"SELECT 
            IngredientId, IngredientName, IngredientDescription, IngredientSlug 
            FROM Ingredients
            WHERE UserId=@userId";

            SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
            user_id.Value = userId;

            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.Text, user_id))
            {
                while (reader.Read())
                {
                    var ingredient = new IngredientDto
                    {
                        Id = (int)reader["IngredientId"],
                        Name = (string)reader["IngredientName"],
                        Description = (string)reader["IngredientDescription"],
                        SlugUrl = (string)reader["IngredientSlug"],
                    };

                    ingredients.Add(ingredient);
                }
                await reader.CloseAsync();
            }
            return ingredients;
        }

        public async Task<bool> IsIngredientExitByName(string ingredientName, int userId, string slugUrl)
        {
            string selectCommandText = @"SELECT 
                Count([IngredientName]) 
                FROM Ingredients 
                WHERE 
                (IngredientName=@ingredientName AND UserId=@userId) 
                OR (IngredientSlug=@slugUrl AND UserId=@userId)";

            SqlParameter ingredient_name = new SqlParameter("@ingredientName", SqlDbType.VarChar);
            ingredient_name.Value = ingredientName;
            SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
            user_id.Value = userId;
            SqlParameter slug_url = new SqlParameter("@slugUrl", SqlDbType.NVarChar);
            slug_url.Value = slugUrl;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                selectCommandText,
                CommandType.Text,
                ingredient_name,
                user_id,
                slug_url);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            throw new Exception("Problem saving changes");
        }
        public async Task<bool> IsIngredientExitById(int ingredientId, int userId)
        {
            string selectCommandText = @"SELECT 
                Count([IngredientId]) 
                FROM Ingredients 
                WHERE IngredientId=@ingredientId AND UserId=@userId";

            SqlParameter ingredient_id = new SqlParameter("@ingredientId", SqlDbType.Int);
            ingredient_id.Value = ingredientId;
            SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
            user_id.Value = userId;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                selectCommandText,
                CommandType.Text,
                ingredient_id,
                user_id);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            throw new Exception("Problem saving changes");
        }

        public async Task<int> Update(int userId, int ingredientId, IngredientDto ingredient)
        {
            string updateCommandText = @"UPDATE 
                Ingredients 
                SET IngredientName = @name, 
                IngredientDescription = @description 
                WHERE IngredientId = @ingredientId AND UserId = @userId";

            SqlParameter ingredient_name = new SqlParameter("@name", ingredient.Name);
            SqlParameter ingredient_description = new SqlParameter("@description", ingredient.Description);
            SqlParameter ingredient_id = new SqlParameter("@ingredientId", ingredientId);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, updateCommandText, CommandType.Text,
                ingredient_name, ingredient_description, ingredient_id, user_id);

            return rows >= 1 ? rows : 0;
            throw new Exception("Problem saving changes");
        }

        public async Task<bool> IsIngredientExitInRecipeIngredient(int ingredientId)
        {
            string commandText = @"SELECT 
                Count([ingredient_id]) 
                FROM Recipe
                WHERE IngredientId=@ingredientId";

            SqlParameter ingredient_id = new SqlParameter("@ingredientId", SqlDbType.Int);
            ingredient_id.Value = ingredientId;
            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                commandText,
                CommandType.Text,
                ingredient_id);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            throw new Exception("Problem saving changes");
        }

        public async Task<bool> Delete(int userId, int ingredientId)
        {
            string DELETECommandText = @"DELETE 
                FROM Ingredients 
                WHERE 
                UserId = @userId AND IngredientId = @ingredientId";

            SqlParameter ingredient_id = new SqlParameter("@ingredientId", ingredientId);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, DELETECommandText, CommandType.Text, ingredient_id, user_id);

            return rows >= 1 ? true : false;
            throw new Exception("Problem saving changes");
        }
    }
}