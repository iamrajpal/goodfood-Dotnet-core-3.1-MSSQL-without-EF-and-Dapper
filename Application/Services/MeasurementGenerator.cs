using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.SqlClientSetup;
using Domain.Entities;

namespace Application.Services
{
    public class MeasurementGenerator : IMeasurementGenerator
    {
        private readonly IConnectionString _connection;
        private readonly IUserAuth _userAuth;
        public string conStr = string.Empty;
        public MeasurementGenerator(IConnectionString connection, IUserAuth userAuth)
        {
            _connection = connection;
            _userAuth = userAuth;
            conStr = _connection.GetConnectionString();
        }
        public async Task<int> Create(string amount)
        {
            //need OUTPUT don't change or delete
            string insertCommandText = @"INSERT INTO [dbo].[measurements] (measurement_amount)
                OUTPUT INSERTED.measurement_id
                values (@amount)";

            SqlParameter measurement_amount = new SqlParameter("@amount", amount);

            var identityId = await SqlHelper.ExecuteScalarAsync(conStr, insertCommandText, CommandType.Text,
                measurement_amount);

            return (int)(identityId != null ? identityId : null);
        }

        public async Task<bool> IsmeasurementExitById(int measurementId)
        {
            string commandText = @"SELECT Count([measurement_id]) FROM [dbo].[measurements] 
                WHERE measurement_id=@measurementId";

            SqlParameter measurement_Id = new SqlParameter("@measurementId", SqlDbType.Int);
            measurement_Id.Value = measurementId;
           
            Object oValue = await SqlHelper.ExecuteScalarAsync(
                conStr,
                commandText,
                CommandType.Text,
                measurement_Id);

            Int32 count;
            if (Int32.TryParse(oValue.ToString(), out count))
                return count > 0 ? true : false;

            throw new Exception("Problem saving changes");
        }

        public async Task<bool> Update(int measurementId, string amount)
        {
            string updateCommandText = @"UPDATE [dbo].[measurements] SET measurement_amount = @amount 
                WHERE measurement_id = @measurementId";

            SqlParameter measurement_amount = new SqlParameter("@amount", amount);
            SqlParameter measurement_id = new SqlParameter("@measurementId", measurementId);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, updateCommandText, CommandType.Text,
                measurement_amount, measurement_id);

            return rows >= 1 ? true : false;
            throw new Exception("Problem saving changes");
        }

        public async Task<Measurement> GetMeasurement(int measurementId)
        {
            var measurement = new Measurement();

            string selectCommandText = "dbo.getMeasurementById";
            SqlParameter parametermeasurementId = new SqlParameter("@measurementId", SqlDbType.Int);
            parametermeasurementId.Value = measurementId;

            bool isIngredientExist = false;
            using (SqlDataReader reader = await SqlHelper.ExecuteReaderAsync(conStr, selectCommandText,
                CommandType.StoredProcedure, parametermeasurementId))
            {

                while (reader.Read())
                {
                    isIngredientExist = true;
                    measurement.Id = (int)reader["measurement_id"];
                    measurement.Amount = (string)reader["measurement_amount"];
                }
                await reader.CloseAsync();
            }
            return isIngredientExist ? measurement : null;
        }
    }
}