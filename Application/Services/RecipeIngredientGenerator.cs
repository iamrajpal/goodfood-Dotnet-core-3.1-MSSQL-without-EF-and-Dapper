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
    }
}