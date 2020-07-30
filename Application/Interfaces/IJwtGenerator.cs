using Domain.Entities;

namespace Application.Interfaces
{
    public interface IJwtGenerator
    {
        string CreateToken(GoodFoodUser user);
    }
}