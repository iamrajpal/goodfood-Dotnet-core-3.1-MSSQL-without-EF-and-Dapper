using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Application.SqlClientSetup;
using System.Collections.Generic;

namespace Application.Services
{
    public class DishRepo : IDish
    {
        private readonly IConnectionString _connection;
        private readonly IUserAuth _userAuth;
        public string conStr = string.Empty;
        public DishRepo(IConnectionString connection, IUserAuth userAuth)
        {
            _connection = connection;
            _userAuth = userAuth;
            conStr = _connection.GetConnectionString();
        }

        public async Task<bool> IsDishExitsWithTitle(string dishTitle, int userId)
        {
            try
            {
                string selectCommandText = @"SELECT Count([DishTitle]) 
                FROM 
                    Dish 
                WHERE 
                    DishTitle=@dishTitle AND UserId=@userId";

                SqlParameter dish_title = new SqlParameter("@dishTitle", SqlDbType.VarChar);
                dish_title.Value = dishTitle;
                SqlParameter user_Id = new SqlParameter("@userId", SqlDbType.Int);
                user_Id.Value = userId;

                Object oValue = await SqlHelper.ExecuteScalarAsync(
                    conStr,
                    selectCommandText,
                    CommandType.Text,
                    dish_title,
                    user_Id);

                Int32 count;
                if (Int32.TryParse(oValue.ToString(), out count))
                    return count > 0 ? true : false;

                return false;
            }
            catch (Exception)
            {
                throw new Exception("Error to connected with DB");
            }

        }

        public async Task<bool> IsDishExitsWithSlug(int userId, string dishSlug)
        {
            try
            {
                string selectCommandText = @"SELECT Count([DishTitle]) 
                FROM 
                    Dish 
                WHERE 
                    DishSlug=@dishSlug AND UserId=@userId";

                SqlParameter user_Id = new SqlParameter("@userId", SqlDbType.Int);
                user_Id.Value = userId;
                SqlParameter recipe_slug = new SqlParameter("@dishSlug", SqlDbType.NVarChar);
                recipe_slug.Value = dishSlug;

                Object oValue = await SqlHelper.ExecuteScalarAsync(
                    conStr,
                    selectCommandText,
                    CommandType.Text,
                    user_Id,
                    recipe_slug);

                Int32 count;
                if (Int32.TryParse(oValue.ToString(), out count))
                    return count > 0 ? true : false;

                return false;

            }
            catch (Exception)
            {
                throw new Exception("Error to connected with DB");
            }

        }

        public async Task<Dish> Create(int userId, Dish dish)
        {
            try
            {
                string insertCommandText = @"INSERT 
                INTO 
                    Dish(DishTitle, DishDescription, DishSlug, DishCategoryId, UserId)
                OUTPUT 
                    INSERTED.DishId
                values 
                    (@dishTitle, @dishDescription, @dishSlug, @dishCategoryId, @userId)";

                SqlParameter dish_title = new SqlParameter("@dishTitle", dish.Title);
                SqlParameter dish_description = new SqlParameter("@dishDescription", dish.Description);
                SqlParameter dish_slug = new SqlParameter("@dishSlug", dish.SlugUrl);
                SqlParameter dish_category_id = new SqlParameter("@dishCategoryId", dish.DishCategoryId);
                SqlParameter user_id = new SqlParameter("@userId", userId);

                var identityId = await SqlHelper.ExecuteScalarAsync(conStr, insertCommandText, CommandType.Text,
                    dish_title, dish_description, dish_slug, dish_category_id, user_id);
                if (identityId != null)
                {
                    var dishToReturn = new Dish
                    {
                        Id = (int)identityId,
                        Title = dish.Title,
                        Description = dish.Description,
                        SlugUrl = dish.SlugUrl,
                        DishCategoryId = dish.DishCategoryId
                    };
                    return dishToReturn;
                }
                return null;
            }
            catch (Exception)
            {
                throw new Exception("Problem creating dish");
            }

        }

        public async Task<bool> Update(int userId, int dishId, Dish dish)
        {
            try
            {
                string updateCommandText = @"UPDATE 
                    Dish 
                SET 
                    DishTitle = @dishTitle, DishDescription = @dishDescription, DishCategoryId = @dishCategoryId 
                WHERE 
                    DishId = @dishId AND UserId = @userId";

                SqlParameter dish_title = new SqlParameter("@dishTitle", dish.Title);
                SqlParameter dish_description = new SqlParameter("@dishDescription", dish.Description);
                SqlParameter dish_category_id = new SqlParameter("@dishCategoryId", dish.DishCategoryId);
                SqlParameter dish_id = new SqlParameter("@dishId", dishId);
                SqlParameter user_id = new SqlParameter("@userId", userId);

                Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, updateCommandText, CommandType.Text,
                    dish_title, dish_description, dish_category_id, dish_id, user_id);

                if (rows >= 1) return true;

                return false;
            }
            catch (Exception)
            {
                throw new Exception("Problem to update dish");
            }

        }

        public async Task<Dish> GetDish(int dishId, int userId)
        {
            try
            {
                var dish = new Dish();
                bool isDishExist = false;
                string selectCommandText = @"SELECT 
                    *
                FROM
                    Dish
                WHERE
                    UserId=@userId ANd DishId=@dishId";

                SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
                user_id.Value = userId;
                SqlParameter dish_id = new SqlParameter("@dishId", SqlDbType.Int);
                dish_id.Value = dishId;

                using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                    CommandType.Text, user_id, dish_id))
                {
                    while (reader.Read())
                    {
                        isDishExist = true;
                        dish.Title = (string)reader["DishTitle"];
                        dish.Description = (string)reader["DishDescription"];
                        dish.SlugUrl = (string)reader["DishSlug"];
                        dish.DishCategoryId = (int)reader["DishCategoryId"];
                    }
                    await reader.CloseAsync();
                }
                return isDishExist ? dish : null;
            }
            catch (Exception)
            {
                throw new Exception("Problem to get dish");
            }

        }

        public async Task<bool> Delete(int userId, List<int> dishIds)
        {
            try
            {
                bool checkDeleteStatus = false;

                foreach (var dishId in dishIds)
                {
                    checkDeleteStatus = false;
                    string deleteCommandText = @"DELETE 
                    FROM
                        Dish
                    WHERE 
                        UserId = @userId AND DishId = @dishId";

                    SqlParameter dish_id = new SqlParameter("@dishId", dishId);
                    SqlParameter user_id = new SqlParameter("@userId", userId);

                    Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, deleteCommandText, CommandType.Text, dish_id, user_id);

                    if (rows >= 1) checkDeleteStatus = true;
                }

                return checkDeleteStatus;
            }
            catch (Exception)
            {
                throw new Exception("Problem to delete dish");
            }

        }

        public async Task<List<Dish>> GetDishes(int userId, string selectCommandText, int? offset, int? limit)
        {
            try
            {
                List<Dish> dishes = new List<Dish>();

                SqlParameter user_id = new SqlParameter("@userId", SqlDbType.Int);
                user_id.Value = userId;
                SqlParameter par_offset = new SqlParameter("@offset", SqlDbType.Int);
                par_offset.Value = offset ?? 0;
                SqlParameter par_limit = new SqlParameter("@limit", SqlDbType.Int);
                par_limit.Value = limit ?? 2;

                using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                    CommandType.Text, user_id, par_offset, par_limit))
                {
                    var dish = new Dish();
                    while (reader.Read())
                    {
                        dish = new Dish();
                        dish.Id = (int)reader["DishId"];
                        dish.Title = (string)reader["DishTitle"];
                        dish.Description = (string)reader["DishDescription"];
                        dish.SlugUrl = (string)reader["DishSlug"];
                        dish.DishCategoryId = (int)reader["DishCategoryId"];
                        dish.DishCategoryTitle = (string)reader["DishCategoryTitle"];

                        dishes.Add(dish);
                    }

                    await reader.CloseAsync();

                }
                return dishes;
            }
            catch (Exception)
            {
                throw new Exception("Problem to get dishes");
            }
        }

        public async Task<bool> IsDishCategoryUsedByDish(int dishCateGoryid)
        {
            try
            {
                string selectCommandText = @"SELECT Count([DishTitle]) 
                FROM 
                    Dish 
                WHERE 
                    DishCategoryId=@dishCategoryId";

                SqlParameter dish_category_id = new SqlParameter("@dishCategoryId", SqlDbType.Int);
                dish_category_id.Value = dishCateGoryid;

                Object oValue = await SqlHelper.ExecuteScalarAsync(
                    conStr,
                    selectCommandText,
                    CommandType.Text,
                    dish_category_id);

                Int32 count;
                if (Int32.TryParse(oValue.ToString(), out count))
                    return count > 0 ? true : false;

                return false;
            }
            catch (Exception)
            {
                throw new Exception("Error to connected with DB");
            }

        }
    }
}