using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IMeasurementGenerator
    {
        Task<bool> Create(IngredientMeasurements measurement);
    }
}