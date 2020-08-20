using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.SqlClientSetup;
using Domain.Entities;

namespace Application.Services
{
    public class DishCategoryRepo : IDishCategory
    {
        private readonly IConnectionString _connection;
        private readonly IUserAuth _userAuth;
        public string conStr = string.Empty;
        public DishCategoryRepo(IConnectionString connection, IUserAuth userAuth)
        {
            _connection = connection;
            _userAuth = userAuth;
            conStr = _connection.GetConnectionString();
        }
        public async Task<bool> Create(int userId, DishCategory dishCategory)
        {
            string insertCommandText = @"INSERT 
                INTO 
                    DishCategory(DishCategoryTitle, UserId)
                OUTPUT 
                    INSERTED.DishCategoryId
                values 
                    (@dishCategoryTitle, @userId)";

            SqlParameter dish_title = new SqlParameter("@dishCategoryTitle", dishCategory.Title);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            var identityId = await SqlHelper.ExecuteScalarAsync(conStr, insertCommandText, CommandType.Text,
                dish_title, user_id);

            if (identityId != null)
            {
                return true;
            }

            return false;

            throw new Exception("Problem creating dish category");
        }

        public async Task<bool> Delete(int userId, int dishCategorId)
        {
            bool checkDeleteStatus = false;

            string deleteCommandText = @"DELETE 
                    FROM
                        DishCategory
                    WHERE 
                        UserId = @userId AND DishCategoryId = @dishCategoryId";

            SqlParameter dish_category_id = new SqlParameter("@dishCategoryId", dishCategorId);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            Int32 rows = await SqlHelper
                .ExecuteNonQueryAsync(conStr, deleteCommandText, CommandType.Text, dish_category_id, user_id);

            if (rows >= 1) checkDeleteStatus = true;

            return checkDeleteStatus;
        }

        public async Task<List<DishCategory>> GetDishCategories(int userId)
        {
            List<DishCategory> dishCategories = new List<DishCategory>();
            // bool isDishExist = false;
            string selectCommandText = @"SELECT 
                    *
                FROM
                    DishCategory
                WHERE
                    UserId=@userId";

            SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
            user_id.Value = userId;
            
            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.Text, user_id))
            {
                while (reader.Read())
                {
                    var dishCategory = new DishCategory();
                    dishCategory.Id =  (int)reader["DishCategoryId"];
                    dishCategory.Title = (string)reader["DishCategoryTitle"];
                    dishCategories.Add(dishCategory);
                }
                await reader.CloseAsync();
            }
            return dishCategories;
        }

        public async Task<DishCategory> GetDishCategory(int dishCategoryId, int userId)
        {
            var dishCategory = new DishCategory();
            bool isDishExist = false;
            string selectCommandText = @"SELECT 
                    *
                FROM
                    DishCategory
                WHERE
                    UserId=@userId ANd DishCategoryId=@dishCategoryId";

            SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
            user_id.Value = userId;
            SqlParameter dish_category_id = new SqlParameter("@dishCategoryId", SqlDbType.Int);
            dish_category_id.Value = dishCategoryId;

            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.Text, user_id, dish_category_id))
            {
                while (reader.Read())
                {
                    isDishExist = true;
                    dishCategory.Title = (string)reader["DishCategoryTitle"];
                }
                await reader.CloseAsync();
            }
            return isDishExist ? dishCategory : null;
        }

        public async Task<bool> IsDishCategoryExits(string dishCategoryTitle, int userId)
        {
            string selectCommandText = @"SELECT Count([DishCategoryTitle]) 
                FROM 
                    DishCategory 
                WHERE 
                    DishCategoryTitle=@dishCategoryTitle AND UserId=@userId";

            SqlParameter dish_category_title = new SqlParameter("@dishCategoryTitle", SqlDbType.VarChar);
            dish_category_title.Value = dishCategoryTitle;
            SqlParameter user_Id = new SqlParameter("@userId", SqlDbType.Int);
            user_Id.Value = userId;

            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                selectCommandText,
                CommandType.Text,
                dish_category_title,
                user_Id);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }

        public async Task<bool> Update(int userId, int dishCategoryId, DishCategory dishCategory)
        {
            string updateCommandText = @"UPDATE 
                    DishCategory 
                SET 
                    DishCategoryTitle = @dishCategoryTitle 
                WHERE 
                    DishCategoryId = @dishCategoryId AND UserId = @userId";

            SqlParameter dish_title = new SqlParameter("@dishcategoryTitle", dishCategory.Title);
            SqlParameter dish_category_id = new SqlParameter("@dishCategoryId", dishCategoryId);
            SqlParameter user_id = new SqlParameter("@userId", userId);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, updateCommandText, CommandType.Text,
                dish_title, dish_category_id, user_id);

            if (rows >= 1) return true;

            return false;
        }
    }
}