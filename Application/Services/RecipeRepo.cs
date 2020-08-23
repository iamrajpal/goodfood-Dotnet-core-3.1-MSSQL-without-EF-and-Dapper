using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.SqlClientSetup;

namespace Application.Services
{
    public class RecipeRepo : IRecipe
    {
        private readonly IConnectionString _connection;
        public string conStr = string.Empty;
        public RecipeRepo(IConnectionString connection)
        {
            _connection = connection;
            conStr = _connection.GetConnectionString();
        }

        public async Task<bool> Create(int dishId, int ingredientId, string measurementAmount)
        {
            try
            {
                string insertCommandText = @"INSERT 
                INTO 
                    Recipe (DishId, IngredientId, Amount)
                values 
                    (@dishId, @ingredientId, @amount)";

                SqlParameter dish_id = new SqlParameter("@dishId", dishId);
                SqlParameter ingredient_id = new SqlParameter("@ingredientId", ingredientId);
                SqlParameter amount = new SqlParameter("@amount", measurementAmount);

                Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, insertCommandText, CommandType.Text,
                    dish_id, ingredient_id, amount);
                return rows >= 1 ? true : false;
            }
            catch (System.Exception)
            {
                throw new Exception("Probleb to create recipe");
            }

        }

        public async Task<bool> IsIdsExitInRecipeIngredient(int ingredientId, int dishId)
        {
            try
            {
                string commandText = @"SELECT Count([DishId]) FROM Recipe 
                WHERE DishId=@dishId AND IngredientId=@ingredientId";

                SqlParameter ingredient_id = new SqlParameter("@ingredientId", SqlDbType.Int);
                ingredient_id.Value = ingredientId;

                SqlParameter dish_id = new SqlParameter("@dishId", SqlDbType.Int);
                dish_id.Value = dishId;

                Object oValue = await SqlHelper.ExecuteScalarAsync(
                    conStr,
                    commandText,
                    CommandType.Text,
                    dish_id,
                    ingredient_id);

                Int32 count;
                if (Int32.TryParse(oValue.ToString(), out count))
                    return count > 0 ? true : false;

                return false;
            }
            catch (System.Exception)
            {
                throw new Exception("Error to connected with DB");
            }
        }

        public async Task<int> Update(int dishId, int ingredientId, string measurementAmount)
        {
            try
            {
                string updateCommandText = @"UPDATE 
                Recipe 
                    SET Amount = @measurementAmount 
                WHERE 
                    (IngredientId=@ingredientId AND DishId=@dishId)";

                SqlParameter ingredient_id = new SqlParameter("@ingredientId", ingredientId);
                SqlParameter dish_id = new SqlParameter("@dishId", dishId);
                SqlParameter amount = new SqlParameter("@measurementAmount", measurementAmount);

                Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, updateCommandText, CommandType.Text,
                    ingredient_id, dish_id, amount);

                return rows >= 1 ? rows : 0;
            }
            catch (System.Exception)
            {
                throw new Exception("Problem saving changes");
            }
        }
    }
}