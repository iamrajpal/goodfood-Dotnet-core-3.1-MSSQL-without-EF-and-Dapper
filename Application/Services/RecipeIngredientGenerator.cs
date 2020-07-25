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

        public async Task<bool> Create(int recipeId, int ingredientId, int? measurementId)
        {

            string insertCommandText = @"INSERT INTO [dbo].[recipe_ingredients] (recipe_id, ingredient_id, measurement_id)
                values (@recipeId, @ingredientId, @measurementId)";

            SqlParameter recipe_id = new SqlParameter("@recipeId", recipeId);
            SqlParameter ingredient_id = new SqlParameter("@ingredientId", ingredientId);
            SqlParameter measurement_id = new SqlParameter("@measurementId", SqlDbType.Int);

            measurement_id.Value = (object)measurementId ?? DBNull.Value;

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, insertCommandText, CommandType.Text,
                recipe_id, ingredient_id, measurement_id);
            return rows >= 1 ? true : false;
        }

        public async Task<bool> IsIdsExitInRecipeIngredient(int ingredientId, int recipeId, int? measurementId)
        {
            string commandText = @"SELECT Count([recipe_id]) FROM [dbo].[recipe_ingredients] 
                WHERE recipe_id=@recipeId AND ingredient_id=@ingredientId AND measurement_id=@measurementId";

            SqlParameter measurement_id = new SqlParameter("@measurementId", SqlDbType.Int);
            measurement_id.Value = measurementId;
            measurement_id.Value = (object)measurementId ?? DBNull.Value;

            SqlParameter ingredient_id = new SqlParameter("@ingredientId", SqlDbType.Int);
            ingredient_id.Value = ingredientId;

            SqlParameter recipe_id = new SqlParameter("@recipeId", SqlDbType.Int);
            recipe_id.Value = recipeId;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                commandText,
                CommandType.Text,
                recipe_id,
                ingredient_id,
                measurement_id);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            throw new Exception("Problem saving changes");
        }

        public async Task<int> Update(int recipeId, int ingredientId, int measurementId)
        {
            string updateCommandText = @"UPDATE [dbo].[recipe_ingredients] SET measurement_id = @measurementId 
                WHERE (ingredient_id=@ingredientId AND recipe_id=@recipeId)";

            SqlParameter ingredient_id = new SqlParameter("@ingredientId", ingredientId);
            SqlParameter recipe_id = new SqlParameter("@recipeId", recipeId);
            SqlParameter measurement_Id= new SqlParameter("@measurementId", measurementId);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, updateCommandText, CommandType.Text,
                ingredient_id, recipe_id, measurement_Id);

            return rows >= 1 ? rows : 0;
            throw new Exception("Problem saving changes");
        }
    }
}