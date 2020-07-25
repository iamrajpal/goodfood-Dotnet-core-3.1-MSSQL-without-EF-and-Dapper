using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IMeasurementGenerator
    {
        Task<int> Create(string amount);
        Task<bool> Update(int measurementId, string amount);
        Task<bool> IsmeasurementExitById(int measurementId);
        Task<Measurement> GetMeasurement(int measurementId);
    }
}