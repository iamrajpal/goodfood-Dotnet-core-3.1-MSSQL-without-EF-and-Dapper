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
using System.Text;
using Application.AesHelper;

namespace Infrastructure.Security
{
    public class UserAuth : IUserAuth
    {
        private readonly IConnectionString _connection;
        public string conStr = string.Empty;
        private readonly Random _random = new Random();
        public UserAuth(IConnectionString connection)
        {
            _connection = connection;
            conStr = _connection.GetConnectionString();
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> IsUserExits(string username)
        {
            string commandText = "Select Count([user_name]) FROM [dbo].[goodfooduser] Where user_name=@username";
            SqlParameter parameterUsername = new SqlParameter("@username", SqlDbType.VarChar);
            parameterUsername.Value = username;

            Object oValue = await SqlHelper.ExecuteScalarAsync(conStr, commandText, CommandType.Text, parameterUsername);
            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            return false;
        }

        public async Task<GoodFoodUserDto> Register(string username, string password)
        {
            if (await IsUserExits(username))
                throw new RestException(HttpStatusCode.BadRequest, new { Username = "Already exist" });

            var key = GenerateKey(32, true);
            var encryptedString = AesHelper.EncryptString(key, password);

            string insertCommandText = @"INSERT INTO [dbo].[goodfooduser] (user_name, user_password, user_password_key)
                values (@user_Name,@user_password,@user_password_key)";

            SqlParameter user_name = new SqlParameter("@user_Name", username);
            SqlParameter user_password = new SqlParameter("@user_password", encryptedString);
            SqlParameter user_password_key = new SqlParameter("@user_password_key", key);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, insertCommandText, CommandType.Text, user_name, user_password, user_password_key);
            if (rows >= 1)
            {
                var user = new GoodFoodUserDto
                {
                    Username = username
                };
                return user;
            }

            throw new Exception("Problem creating user");
        }

        public async Task<List<GoodFoodUserDto>> GetAllUser()
        {
            List<GoodFoodUserDto> allusers = new List<GoodFoodUserDto>();

            string selectCommandText = "dbo.getAllUsernames";
            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.StoredProcedure))
            {
                while (reader.Read())
                {
                    var user = new GoodFoodUserDto
                    {
                        Username = reader["user_name"].ToString()
                    };

                    allusers.Add(user);
                }
                await reader.CloseAsync();
            }
            return allusers;
        }

        public async Task<GoodFoodUser> GetUser(string username)
        {
            var user = new GoodFoodUser();
            bool isUserExist = false;
            string selectCommandText = "dbo.getUser";
            SqlParameter parameterUsername = new SqlParameter("@username", SqlDbType.VarChar);
            parameterUsername.Value = username;
            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.StoredProcedure, parameterUsername))
            {
                while (reader.Read())
                {
                    isUserExist = true;
                    user.Username = (string)reader["user_name"];
                    user.Password = (string)reader["user_password"];
                    user.PasswordKey = (string)reader["user_password_key"];
                    user.Id = (int)reader["user_id"];
                }
                await reader.CloseAsync();
            }
            return isUserExist ? user : null;
        }


        public async Task<string> VerifyUser(string username, string password)
        {
            string usernameFromDb = string.Empty;
            string selectCommandText = "dbo.getUser";
            SqlParameter user_name = new SqlParameter("@username", SqlDbType.VarChar);
            user_name.Value = username;

            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.StoredProcedure, user_name))
            {
                while (reader.Read())
                {

                    var pass = reader["user_password"].ToString();
                    var key = reader["user_password_key"].ToString();

                    var decryptedString = AesHelper.DecryptString(key, pass);

                    if (decryptedString != password)
                        throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                    usernameFromDb = reader["user_name"].ToString();
                }
                await reader.CloseAsync();
            }
            return usernameFromDb;
        }


        private string GenerateKey(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
    }
}
