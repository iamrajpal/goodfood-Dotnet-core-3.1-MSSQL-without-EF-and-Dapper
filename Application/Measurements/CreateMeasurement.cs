using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Measurements
{
    public class CreateMeasurement
    {
        public class CreateMeasurementCommand : IRequest
        {
            public string Amount { get; set; }
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<CreateMeasurementCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IMeasurementGenerator _measurementGenerator;
            public Handler(IUserAuth userAuth, IMeasurementGenerator measurementGenerator)
            {
                _measurementGenerator = measurementGenerator;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(CreateMeasurementCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });
                
                var success = await _measurementGenerator.Create(request.Amount);

                if (success > 0) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}