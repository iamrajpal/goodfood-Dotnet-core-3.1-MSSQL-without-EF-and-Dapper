using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using Application.SqlClientSetup;

namespace Infrastructure.Security
{
    public class UserAuth : IUserAuth
    {
        private readonly IConnectionString _connection;
        public string conStr = string.Empty;
        private readonly IUserAccessor _userAccessor;
        public UserAuth(IConnectionString connection, IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
            _connection = connection;
            conStr = _connection.GetConnectionString();
        }

        public async Task<GoodFoodUser> GetCurrentUser()
        {
            var username = _userAccessor.GetCurrentUsername();
            if (!string.IsNullOrWhiteSpace(username))
            {
                var user = await GetUser(username);
                return user;
            }

            return null;
        }
        public async Task<bool> IsUserExits(string userName)
        {
            try
            {
                string commandText = @"Select 
                    Count([UserName]) 
                FROM 
                    GoodFoodUser 
                Where 
                    UserName=@userName";
                SqlParameter user_name = new SqlParameter("@userName", SqlDbType.VarChar);
                user_name.Value = userName;

                Object oValue = await SqlHelper.ExecuteScalarAsync(conStr, commandText, CommandType.Text, user_name);
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

        public async Task<GoodFoodUser> Register(string userName, string password)
        {
            try
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                string insertCommandText = @"INSERT 
                INTO 
                    GoodFoodUser (UserName, UserPassword, UserPasswordSalt)
                VALUES 
                    (@userName,@passwordHash,@passwordSalt)";

                SqlParameter user_name = new SqlParameter("@userName", userName);
                SqlParameter user_password = new SqlParameter("@passwordHash", passwordHash);
                SqlParameter user_password_salt = new SqlParameter("@passwordSalt", passwordSalt);

                Int32 rows = await SqlHelper.ExecuteNonQueryAsync(
                    conStr,
                    insertCommandText,
                    CommandType.Text,
                    user_name,
                    user_password,
                    user_password_salt);
                if (rows >= 1)
                {
                    var user = new GoodFoodUser
                    {
                        Username = userName
                    };
                    return user;
                }
                return null;
            }
            catch (System.Exception)
            {
                throw new Exception("Problem creating user");
            }
        }

        public async Task<List<GoodFoodUserDto>> GetAllUser()
        {
            try
            {
                List<GoodFoodUserDto> allusers = new List<GoodFoodUserDto>();
                string selectCommandText = @"SELECT UserName FROM GoodFoodUser";

                using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                    CommandType.Text))
                {
                    while (reader.Read())
                    {
                        var user = new GoodFoodUserDto
                        {
                            UserName = reader["UserName"].ToString()
                        };

                        allusers.Add(user);
                    }
                    await reader.CloseAsync();
                }
                return allusers;

            }
            catch (System.Exception)
            {
                throw new Exception("Problem to get users");
            }

        }

        public async Task<GoodFoodUser> GetUser(string userName)
        {
            try
            {
                var user = new GoodFoodUser();
                bool isUserExist = false;
                string selectCommandText = @"SELECT * FROM GoodFoodUser WHERE UserName=@userName";
                SqlParameter user_name = new SqlParameter("@userName", SqlDbType.VarChar);
                user_name.Value = userName;
                using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                    CommandType.Text, user_name))
                {
                    while (reader.Read())
                    {
                        isUserExist = true;
                        user.Username = (string)reader["UserName"];
                        user.Password = (byte[])reader["UserPassword"];
                        user.PasswordSalt = (byte[])reader["UserPasswordSalt"];
                        user.Id = (int)reader["UserId"];
                    }
                    await reader.CloseAsync();
                }
                return isUserExist ? user : null;
            }
            catch (System.Exception)
            {
                throw new Exception("Problem get user");
            }
        }
        public async Task<GoodFoodUser> VerifyUser(string userName, string password)
        {
            try
            {
                string selectCommandText = @"SELECT * FROM GoodFoodUser WHERE UserName=@userName";
                SqlParameter user_name = new SqlParameter("@userName", SqlDbType.VarChar);
                user_name.Value = userName;

                var userFromDB = new GoodFoodUser();
                bool isUserInDb = false;

                using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                    CommandType.Text, user_name))
                {
                    while (reader.Read())
                    {
                        isUserInDb = true;

                        var pass = reader["UserPassword"];
                        var salt = reader["UserPasswordSalt"];

                        if (!verifyPasswordHash(password, (byte[])pass, (byte[])salt))
                            throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                        userFromDB.Username = reader["UserName"].ToString();
                    }
                    await reader.CloseAsync();
                }
                return isUserInDb ? userFromDB : null;
            }
            catch (System.Exception)
            {
                throw new Exception("Problem get user");
            }
        }
        private bool verifyPasswordHash(string password, byte[] user_Password_Hash, byte[] user_Password_Salt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(user_Password_Salt))
            {
                var computerHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computerHash.Length; i++)
                {
                    if (computerHash[i] != user_Password_Hash[i]) return false;
                }
            }
            return true;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

    }
}