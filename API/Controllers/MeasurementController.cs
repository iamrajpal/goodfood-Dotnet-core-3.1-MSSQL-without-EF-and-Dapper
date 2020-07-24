using System.Threading.Tasks;
using Application.Measurements;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MeasurementController : BaseController
    {
        [HttpPost("{username}/create")]
        public async Task<ActionResult<Unit>> Create(string username, CreateMeasurement.CreateMeasurementCommand command)
        {
            command.Username = username;
            return await Mediator.Send(command);
        }
    }
}