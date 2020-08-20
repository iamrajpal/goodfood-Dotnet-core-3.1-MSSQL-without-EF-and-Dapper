using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.DishCategories
{
    public class GetDishCategories
    {
        public class GetDishCategoriesQuery : IRequest<List<DishCategory>>
        {
            public string Username { get; set; }           
        }
        public class Handler : IRequestHandler<GetDishCategoriesQuery, List<DishCategory>>
        {
            private readonly IUserAuth _userAuth;
            private readonly IDishCategory _dishCategory;
            public Handler(IUserAuth userAuth, IDishCategory dishCategory)
            {
                _dishCategory = dishCategory;
                _userAuth = userAuth;
            }

            public async Task<List<DishCategory>> Handle(GetDishCategoriesQuery request,
                CancellationToken cancellationToken)
            {

                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.NotFound, new { User = "Not found" });

                var dishCategories = await _dishCategory.GetDishCategories(user.Id);

                return dishCategories;
            }
        }
    }
}