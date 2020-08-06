using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.SqlClientSetup;
using Domain.Entities;

namespace Application.Services
{
    public class RecipeIngredientGenerator : IRecipeIngredientGenerator
    {
        private readonly IConnectionString _connection;
        public string conStr = string.Empty;
        public RecipeIngredientGenerator(IConnectionString connection)
        {
            _connection = connection;
            conStr = _connection.GetConnectionString();
        }

        public async Task<bool> Create(int recipeId, int ingredientId, string measurementAmount)
        {

            string insertCommandText = @"INSERT INTO [dbo].[recipe_ingredients] (recipe_id, ingredient_id, amount)
                values (@recipeId, @ingredientId, @amount)";

            SqlParameter recipe_id = new SqlParameter("@recipeId", recipeId);
            SqlParameter ingredient_id = new SqlParameter("@ingredientId", ingredientId);
            SqlParameter amount = new SqlParameter("@amount", measurementAmount);
            // SqlParameter measurement_id = new SqlParameter("@measurementId", SqlDbType.Int);

            // measurement_id.Value = (object)measurementId ?? DBNull.Value;

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, insertCommandText, CommandType.Text,
                recipe_id, ingredient_id, amount);
            return rows >= 1 ? true : false;
        }

        public async Task<bool> IsIdsExitInRecipeIngredient(int ingredientId, int recipeId)
        {
            string commandText = @"SELECT Count([recipe_id]) FROM [dbo].[recipe_ingredients] 
                WHERE recipe_id=@recipeId AND ingredient_id=@ingredientId";

            SqlParameter ingredient_id = new SqlParameter("@ingredientId", SqlDbType.Int);
            ingredient_id.Value = ingredientId;

            SqlParameter recipe_id = new SqlParameter("@recipeId", SqlDbType.Int);
            recipe_id.Value = recipeId;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                commandText,
                CommandType.Text,
                recipe_id,
                ingredient_id);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            throw new Exception("Problem saving changes");
        }

        public async Task<int> Update(int recipeId, int ingredientId, string measurementAmount)
        {
            string updateCommandText = @"UPDATE [dbo].[recipe_ingredients] SET amount = @measurementAmount 
                WHERE (ingredient_id=@ingredientId AND recipe_id=@recipeId)";

            SqlParameter ingredient_id = new SqlParameter("@ingredientId", ingredientId);
            SqlParameter recipe_id = new SqlParameter("@recipeId", recipeId);
            SqlParameter amount = new SqlParameter("@measurementAmount", measurementAmount);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, updateCommandText, CommandType.Text,
                ingredient_id, recipe_id, amount);

            return rows >= 1 ? rows : 0;
            throw new Exception("Problem saving changes");
        }
    }
}