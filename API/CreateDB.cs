using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Interfaces;
using Infrastructure.Security;

namespace API
{
    public class CreateDB
    {
        public static async Task CreateAndSeedData(string conStr)
        {
            using (SqlConnection conn = new SqlConnection(conStr))
            {

                bool tableExit = false;
                string selectCommand = @"SELECT object_id FROM sys.tables WHERE name = 'goodfooduser'";

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

                    string createTableCommandText = @"CREATE TABLE goodfooduser(
                    user_id INT IDENTITY(1,1) PRIMARY KEY,
                    user_name VARCHAR(50) NOT NULL,
                    user_password VARBINARY(MAX) NOT NULL,
                    user_password_salt VARBINARY(MAX) NOT NULL,
                    UNIQUE(user_name)
                    )";
                    using (SqlCommand cmd = new SqlCommand(createTableCommandText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    createTableCommandText = @"CREATE TABLE recipe_category(
                    recipe_category_id INT IDENTITY(1,1) PRIMARY KEY,
                    recipe_category_title VARCHAR(90) NOT NULL,
                    user_id INT NOT NULL,
                    FOREIGN KEY (user_id)
                    REFERENCES goodfooduser(user_id) 
                    ON DELETE NO ACTION ON UPDATE NO ACTION,
                    UNIQUE(recipe_category_title)
                    )";
                    using (SqlCommand cmd = new SqlCommand(createTableCommandText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }

                    createTableCommandText = @"CREATE TABLE recipes(
                    recipe_id INT IDENTITY(1,1) PRIMARY KEY,
                    recipe_title VARCHAR(90) NOT NULL,
                    recipe_description VARCHAR(MAX),
                    recipe_slug NVARCHAR(50),
                    recipe_category_id INT NOT NULL,
                    user_id INT NOT NULL,
                    FOREIGN KEY (user_id)
                    REFERENCES goodfooduser(user_id) 
                    ON DELETE NO ACTION ON UPDATE NO ACTION,
                    FOREIGN KEY (recipe_category_id)
                    REFERENCES recipe_category(recipe_category_id) 
                    ON DELETE NO ACTION ON UPDATE NO ACTION
                    )";
                    using (SqlCommand cmd = new SqlCommand(createTableCommandText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }


                    createTableCommandText = @"CREATE TABLE ingredients(
                    ingredient_id INT IDENTITY(1,1) PRIMARY KEY,
                    ingredient_name VARCHAR(90) NOT NULL,
                    ingredient_description VARCHAR(max),
                    ingredient_slug NVARCHAR(50),
                    user_id INT NOT NULL,
                    FOREIGN KEY (user_id)
                    REFERENCES goodfooduser(user_id) 
                    ON DELETE NO ACTION ON UPDATE NO ACTION
                    )";
                    using (SqlCommand cmd = new SqlCommand(createTableCommandText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }

                    createTableCommandText = @"CREATE TABLE recipe_ingredients(
                    recipe_id INT,
                    ingredient_id INT,
                    amount VARCHAR(10),
                    PRIMARY KEY(recipe_id, ingredient_id),
                    FOREIGN KEY (recipe_id)
                    REFERENCES recipes(recipe_id)
                    ON DELETE CASCADE ON UPDATE CASCADE,
                    FOREIGN KEY (ingredient_id)
                    REFERENCES ingredients(ingredient_id)
                    ON DELETE CASCADE ON UPDATE CASCADE
                    )";
                    using (SqlCommand cmd = new SqlCommand(createTableCommandText, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                conn.Close();
            }
        }
    }
}