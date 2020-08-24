using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace API
{
    public class CreateDB
    {

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public static async Task CreateAndSeedData(string conStr)
        {
            using (SqlConnection conn = new SqlConnection(conStr))
            {

                bool tableExit = false;
                string createTableCommandText = "";
                string insertCommandText = "";
                string selectCommand = @"SELECT object_id FROM sys.tables WHERE name = 'GoodFoodUser'";

                using (SqlCommand cmd = new SqlCommand(selectCommand, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                    tableExit = reader.HasRows;
                    await reader.CloseAsync();
                    conn.Close();
                }

                if (!tableExit)
                {
                    conn.Open();
                    createTableCommandText = @"CREATE TABLE GoodFoodUser(
                    UserId INT IDENTITY(1,1) NOT NULL,
                    UserName VARCHAR(50) NOT NULL,
                    UserPassword VARBINARY(MAX) NOT NULL,
                    UserPasswordSalt VARBINARY(MAX) NOT NULL,
                    UNIQUE(UserName),
                    CONSTRAINT PK_User PRIMARY KEY(UserId)
                    )";
                    using (SqlCommand cmd = new SqlCommand(createTableCommandText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Create some users
                    List<string> userNames = new List<string>();
                    userNames.Add("Rajpal");
                    userNames.Add("John");
                    userNames.Add("Adam");

                    foreach (string username in userNames)
                    {
                        byte[] passwordHash, passwordSalt;
                        CreatePasswordHash("Pa$$word", out passwordHash, out passwordSalt);

                        insertCommandText = @"INSERT INTO GoodFoodUser (UserName, UserPassword, UserPasswordSalt)
                            values (@userName,@passwordHash,@passwordSalt)";

                        SqlParameter user_name = new SqlParameter("@userName", username);
                        SqlParameter user_password = new SqlParameter("@passwordHash", passwordHash);
                        SqlParameter user_password_salt = new SqlParameter("@passwordSalt", passwordSalt);

                        using (SqlCommand cmd = new SqlCommand(insertCommandText, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add(user_name);
                            cmd.Parameters.Add(user_password);
                            cmd.Parameters.Add(user_password_salt);
                            Int32 rows = await cmd.ExecuteNonQueryAsync();
                            if (rows < 1)
                                throw new Exception("Problem creating user");
                        }

                    }
                    conn.Close();
                }

                // check and create recipe_category table
                tableExit = false;
                selectCommand = @"SELECT object_id FROM sys.tables WHERE name = 'DishCategory'";
                using (SqlCommand cmd = new SqlCommand(selectCommand, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                    tableExit = reader.HasRows;
                    await reader.CloseAsync();
                    conn.Close();
                }
                if (!tableExit)
                {
                    conn.Open();
                    createTableCommandText = @"CREATE TABLE DishCategory(
                    DishCategoryId INT IDENTITY(1,1) NOT NULL,
                    DishCategoryTitle VARCHAR(90) NOT NULL,
                    UserId INT NOT NULL,
                    CONSTRAINT FK_DishCategoryUser FOREIGN KEY (UserId)
                    REFERENCES GoodFoodUser(UserId) 
                    ON DELETE NO ACTION ON UPDATE NO ACTION,
                    UNIQUE(DishCategoryTitle),
                    CONSTRAINT PK_DishCategory PRIMARY KEY(DishCategoryId)
                    )";
                    using (SqlCommand cmd = new SqlCommand(createTableCommandText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    conn.Close();
                }

                // check and create recipes table
                tableExit = false;
                selectCommand = @"SELECT object_id FROM sys.tables WHERE name = 'Dish'";
                using (SqlCommand cmd = new SqlCommand(selectCommand, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                    tableExit = reader.HasRows;
                    await reader.CloseAsync();
                    conn.Close();
                }

                if (!tableExit)
                {
                    conn.Open();

                    createTableCommandText = @"CREATE TABLE Dish(
                    DishId INT IDENTITY(1,1) NOT NULL,
                    DishTitle VARCHAR(90) NOT NULL,
                    DishDescription VARCHAR(MAX),
                    DishSlug NVARCHAR(50),
                    DishCategoryId INT NOT NULL,
                    UserId INT NOT NULL,                    
                    CONSTRAINT FK_Dish_GoodFoodUser FOREIGN KEY (UserId) REFERENCES GoodFoodUser(UserId) 
                    ON DELETE NO ACTION ON UPDATE NO ACTION,
                    CONSTRAINT FK_Dish_DishCategory FOREIGN KEY (DishCategoryId) REFERENCES DishCategory(DishCategoryId) 
                    ON DELETE NO ACTION ON UPDATE NO ACTION,
                    CONSTRAINT PK_Dish PRIMARY KEY (DishId)
                    )";
                    using (SqlCommand cmd = new SqlCommand(createTableCommandText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    conn.Close();
                }

                // check and create ingredients table
                tableExit = false;
                selectCommand = @"SELECT object_id FROM sys.tables WHERE name = 'Ingredients'";
                using (SqlCommand cmd = new SqlCommand(selectCommand, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                    tableExit = reader.HasRows;
                    await reader.CloseAsync();
                    conn.Close();
                }

                if (!tableExit)
                {
                    conn.Open();
                    createTableCommandText = @"CREATE TABLE Ingredients(
                    IngredientId INT IDENTITY(1,1) NOT NULL,
                    IngredientName VARCHAR(90) NOT NULL,
                    IngredientDescription VARCHAR(max),
                    IngredientSlug NVARCHAR(50),
                    UserId INT NOT NULL,
                    CONSTRAINT FK_Ingredients_GoodFoodUser FOREIGN KEY (UserId) REFERENCES GoodFoodUser(UserId) 
                    ON DELETE NO ACTION ON UPDATE NO ACTION,
                    CONSTRAINT PK_Ingredients PRIMARY KEY (IngredientId)
                    )";
                    using (SqlCommand cmd = new SqlCommand(createTableCommandText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    conn.Close();
                }

                // check and create Recipe table
                tableExit = false;
                selectCommand = @"SELECT object_id FROM sys.tables WHERE name = 'Recipe'";
                using (SqlCommand cmd = new SqlCommand(selectCommand, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                    tableExit = reader.HasRows;
                    await reader.CloseAsync();
                    conn.Close();
                }

                if (!tableExit)
                {
                    conn.Open();

                    createTableCommandText = @"CREATE TABLE Recipe(
                    DishId INT NOT NULL,
                    IngredientId INT NOT NULL,
                    Amount VARCHAR(10),
                    CONSTRAINT PK_Recipe PRIMARY KEY(DishId, IngredientId),
                    CONSTRAINT FK_Recipe_Dish FOREIGN KEY (DishId) REFERENCES Dish(DishId)
                    ON DELETE CASCADE ON UPDATE CASCADE,
                    CONSTRAINT FK_Recipe_Ingredient FOREIGN KEY (IngredientId) REFERENCES ingredients(IngredientId)
                    ON DELETE CASCADE ON UPDATE CASCADE
                    )";
                    using (SqlCommand cmd = new SqlCommand(createTableCommandText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    conn.Close();
                }

            }
        }
    }
}