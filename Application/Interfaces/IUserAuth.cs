using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using Domain;

namespace Application.Interfaces
{
    public interface IUserAuth
    {
        Task<GoodFoodUserDto> Register(string username, string password);
        Task<GoodFoodUserDto> Login(string username, string password);
        Task<bool> UserExits(string username);
        Task<List<GoodFoodUserDto>> GetAllUser();
        Task<GoodFoodUser> GetUser(string username);
    }
}