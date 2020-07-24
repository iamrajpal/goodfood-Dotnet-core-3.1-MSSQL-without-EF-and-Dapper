using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IMeasurementGenerator
    {
        Task<int> Create(string amount);
    }
}