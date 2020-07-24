using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Interfaces
{
    public interface IIngredientGenerator
    {
        Task<bool> Create(int userId, Ingredients recipe);
        Task<bool> IsIngredientExits(string IngredientName, int userId);
    }
}