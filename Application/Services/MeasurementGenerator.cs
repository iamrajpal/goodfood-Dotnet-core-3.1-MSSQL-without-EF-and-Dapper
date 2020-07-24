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
        public async Task<bool> Create(IngredientMeasurements measurement)
        {
            string insertCommandText = @"INSERT INTO [dbo].[measurements] (measurement_amount, measurement_description)
                values (@amount, @description)";

            SqlParameter measurement_amount = new SqlParameter("@amount", measurement.Amount);
            SqlParameter measurement_description = new SqlParameter("@description", measurement.Description);

            Int32 rows = await SqlHelper.ExecuteNonQueryAsync(conStr, insertCommandText, CommandType.Text,
                measurement_amount, measurement_description);

            if (rows >= 1) return true;

            return false;
        }
       
    }
}