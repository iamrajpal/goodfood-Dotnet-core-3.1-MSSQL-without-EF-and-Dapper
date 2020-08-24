using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.DishCategories
{
    public class CreateDishCategory
    {
        public class CreateDishCategoryCommand : IRequest
        {
            public string Title { get; set; }
        }
         public class CommandValidator : AbstractValidator<CreateDishCategoryCommand>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Title).NotEmpty();
            }
        }
        public class Handler : IRequestHandler<CreateDishCategoryCommand>
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

            public async Task<Unit> Handle(CreateDishCategoryCommand request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetCurrentUser();
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                if (await _dishCategory.IsDishCategoryExits(request.Title, user.Id))
                    throw new RestException(HttpStatusCode.BadRequest, new { DishCategory = "Already exist" });

                var toCreateDish = new DishCategory
                {
                    Title = request.Title
                };

                bool createdDish = await _dishCategory.Create(user.Id, toCreateDish);
                if(createdDish) return Unit.Value;

                throw new Exception("Problem creating dish category");
            }
        }
    }
}