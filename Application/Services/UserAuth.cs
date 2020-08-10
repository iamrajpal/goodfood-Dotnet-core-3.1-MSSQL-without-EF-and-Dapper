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
        public UserAuth(IConnectionString connection)
        {
            _connection = connection;
            conStr = _connection.GetConnectionString();
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

        public async Task<GoodFoodUser> Register(string username, string password)
        {
            if (await IsUserExits(username))
                throw new RestException(HttpStatusCode.BadRequest, new { Username = "Already exist" });

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            string insertCommandText = @"INSERT INTO [dbo].[goodfooduser] (user_name, user_password, user_password_salt)
                values (@user_Name,@passwordHash,@passwordSalt)";

            SqlParameter user_name = new SqlParameter("@user_Name", username);
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
            SqlParameter user_name = new SqlParameter("@username", SqlDbType.VarChar);
            user_name.Value = username;
            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.StoredProcedure, user_name))
            {
                while (reader.Read())
                {
                    isUserExist = true;
                    user.Username = (string)reader["user_name"];
                    user.Password = (byte[])reader["user_password"];
                    user.PasswordSalt = (byte[])reader["user_password_salt"];
                    user.Id = (int)reader["user_id"];
                }
                await reader.CloseAsync();
            }
            return isUserExist ? user : null;
        }
        public async Task<GoodFoodUser> VerifyUser(string username, string password)
        {
            string selectCommandText = "dbo.getUser";
            SqlParameter user_name = new SqlParameter("@username", SqlDbType.VarChar);
            user_name.Value = username;
            var userFromDB = new GoodFoodUser();
            bool isUserInDb = false;

            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.StoredProcedure, user_name))
            {
                while (reader.Read())
                {
                    isUserInDb = true;

                    var pass = reader["user_password"];
                    var salt = reader["user_password_salt"];

                    if (!verifyPasswordHash(password, (byte[])pass, (byte[])salt))
                        throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                    userFromDB.Username = reader["user_name"].ToString();
                }
                await reader.CloseAsync();
            }
            return isUserInDb ? userFromDB : null;
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