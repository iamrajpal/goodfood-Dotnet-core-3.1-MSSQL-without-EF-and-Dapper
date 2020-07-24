using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Application.SqlClientSetup;
using Domain.Entities;
using MediatR;

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

        public async Task<bool> IsIngredientExits(string IngredientName, int userId)
        {
            string commandText = @"SELECT Count([ingredient_name]) FROM [dbo].[ingredients] 
                WHERE ingredient_name=@ingredientname AND user_Id=@userId";

            SqlParameter parameterIngredientname = new SqlParameter("@ingredientname", SqlDbType.VarChar);
            parameterIngredientname.Value = IngredientName;
            SqlParameter parameterUserId = new SqlParameter("@userId", SqlDbType.Int);
            parameterUserId.Value = userId;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                commandText,
                CommandType.Text,
                parameterIngredientname,
               parameterUserId);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }
    }
}