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
            string insertCommandText = @"INSERT INTO [dbo].[measurements] (measurement_amount)
                OUTPUT INSERTED.measurement_id
                values (@amount)";

            SqlParameter measurement_amount = new SqlParameter("@amount", amount);

            var identityId = await SqlHelper.ExecuteScalarAsync(conStr, insertCommandText, CommandType.Text,
                measurement_amount);

            return (int)(identityId != null ? identityId : null);
        }
       
    }
}