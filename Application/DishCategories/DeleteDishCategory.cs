using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using MediatR;


namespace Application.DishCategories
{
    public class DeleteDishCategory
    {
        public class DeleteDishCategoryCommand : IRequest
        {
            public int DishCategoryId { get; set; }
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<DeleteDishCategoryCommand>
        {
            private readonly IUserAuth _userAuth;
            private readonly IDishCategory _dishCategory;
            private readonly IDishGenerator _dishGenerator;
            public Handler(IUserAuth userAuth, IDishCategory dishCategory, IDishGenerator dishGenerator)
            {
                _dishGenerator = dishGenerator;
                _dishCategory = dishCategory;
                _userAuth = userAuth;
            }

            public async Task<Unit> Handle(DeleteDishCategoryCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var dishCategory = await _dishCategory.GetDishCategory(request.DishCategoryId, user.Id);
                if (dishCategory == null)
                    throw new RestException(HttpStatusCode.NotFound, new { DishCategory = "Not found" });

                if (await _dishGenerator.IsDishCategoryUsedByDish(request.DishCategoryId))
                    throw new RestException(HttpStatusCode.BadRequest, new { DishCategory = "Used by dish" });

                var success = await _dishCategory.Delete(user.Id, request.DishCategoryId);
                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}