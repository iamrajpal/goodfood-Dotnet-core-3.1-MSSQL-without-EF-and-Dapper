using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using MediatR;


namespace Application.DishCategories
{
    public class UpdateDishCategory
    {
        public class UpdateDishCategoryCommand : IRequest
        {
            public int DishCategoryId { get; set; }
            public string Title { get; set; }
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<UpdateDishCategoryCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IDishCategory _dishCategory;

            public Handler(
                IUserAuth userAuth,
                IDishCategory dishCategory)
            {
                _dishCategory = dishCategory;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(UpdateDishCategoryCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var dishCategory = await _dishCategory.GetDishCategory(request.DishCategoryId, user.Id);
                if (dishCategory == null)
                    throw new RestException(HttpStatusCode.NotFound, new { DishCategory = "Not found" });

                var updateDishCategory = new DishCategory
                {
                    Title = request.Title ?? dishCategory.Title,
                };

                var success = await _dishCategory.Update(user.Id, request.DishCategoryId, updateDishCategory);                
                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }


    }
}