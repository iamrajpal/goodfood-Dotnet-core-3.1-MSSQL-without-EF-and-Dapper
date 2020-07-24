using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Errors;
using Application.Interfaces;
using MediatR;

namespace Application.Ingredeients
{
    public class GetIngredients
    {
        public class GetIngredientsQuery : IRequest<List<IngredientDto>>
        {
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<GetIngredientsQuery, List<IngredientDto>>
        {
            private readonly IUserAuth _userAuth;
            private readonly IIngredientGenerator _ingredientGenerator;
            public Handler(IUserAuth userAuth, IIngredientGenerator ingredientGenerator)
            {
                _ingredientGenerator = ingredientGenerator;
                _userAuth = userAuth;
            }

            public async Task<List<IngredientDto>> Handle(GetIngredientsQuery request,
                CancellationToken cancellationToken)
            {
                var user = await _userAuth.GetUser(request.Username);
                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { User = "Not pass" });

                var ingredients =await _ingredientGenerator.GetIngredients(user.Id);

                return ingredients;
            }
        }
    }
}