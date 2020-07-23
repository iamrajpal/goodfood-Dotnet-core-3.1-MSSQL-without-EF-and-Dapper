using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Errors;
using Application.Interfaces;
using Domain;
using Infrastructure.SqlClientSetup;

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
       
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExits(string username)
        {
            String commandText = "Select Count([user_name]) FROM [dbo].[goodfooduser] Where user_name=@username";
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
            if (await UserExits(username))
                throw new RestException(HttpStatusCode.BadRequest, new { Username = "Already exist" });

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            string insertCommandText = @"INSERT INTO [dbo].[goodfooduser] (user_name, user_password_hash, user_password_salt)
                values (@user_Name,@user_password_hash,@user_password_salt)";

            SqlParameter user_name = new SqlParameter("@user_Name", username);
            SqlParameter user_password_hash = new SqlParameter("@user_password_hash", passwordHash);
            SqlParameter user_password_salt = new SqlParameter("@user_password_salt", passwordSalt);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, insertCommandText, CommandType.Text, user_name, user_password_hash, user_password_salt);
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

            string selectCommandText = "dbo.getAllExistUsers";
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
            string selectCommandText = "dbo.getExistUserWithHash";
            SqlParameter parameterUsername = new SqlParameter("@username", SqlDbType.VarChar);
            parameterUsername.Value = username;
            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.StoredProcedure, parameterUsername))
            {
                while (reader.Read())
                {
                    user.Username = reader["user_name"].ToString();
                    var pass = ObjectToByteArray(reader["user_password_hash"]);
                    user.PasswordHash = pass;
                    var salt = ObjectToByteArray(reader["user_password_salt"]);
                    user.PasswordSalt = salt;
                }
                await reader.CloseAsync();
            }

            return user;
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

        byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

    }
}