using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Security
{
    public class GetDBConnectionString : IConnectionString
    {
        private readonly IConfiguration _configuration;

        public GetDBConnectionString(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GetConnectionString()
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");
            return conStr;
        }
    }
}