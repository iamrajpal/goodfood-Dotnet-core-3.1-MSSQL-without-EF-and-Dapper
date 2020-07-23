using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Infrastructure.SqlClientSetup
{
    
    static class SqlHelper
    {
        // Set the connection, command, and then execute the command with non query.  
        public async static Task<Int32> ExecuteNonQueryAsync(String connectionString, String commandText,
            CommandType commandType, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(commandText, conn))
                {
                    // There're three command types: StoredProcedure, Text, TableDirect. The TableDirect   
                    // type is only for OLE DB.    
                    cmd.CommandType = commandType;
                    cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // Set the connection, command, and then execute the command and only return one value.  
        public async static Task<Object> ExecuteScalarAsync(String connectionString, String commandText,
            CommandType commandType, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(commandText, conn))
                {
                    cmd.CommandType = commandType;
                    cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    return await cmd.ExecuteScalarAsync();
                }
            }
        }

        // Set the connection, command, and then execute the command with query and return the reader.  
        public async static Task<SqlDataReader> ExecuteReaderAsync(String connectionString, String commandText,
            CommandType commandType, params SqlParameter[] parameters)
        {
            SqlConnection conn = new SqlConnection(connectionString);

            using (SqlCommand cmd = new SqlCommand(commandText, conn))
            {
                cmd.CommandType = commandType;
                cmd.Parameters.AddRange(parameters);

                conn.Open();
                // When using CommandBehavior.CloseConnection, the connection will be closed when the   
                // IDataReader is closed.  
                SqlDataReader reader =await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                return reader;
            }
        }
    }
}