using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserAuth
    {
        Task<GoodFoodUserDto> Register(string username, string password);
        Task<bool> IsUserExits(string username);
        Task<List<GoodFoodUserDto>> GetAllUser();
        Task<GoodFoodUser> GetUser(string username);
        Task<string> VerifyUser(string username, string password);
    }
}