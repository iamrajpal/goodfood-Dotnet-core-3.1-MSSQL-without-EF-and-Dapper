using Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Infrastructure.Security;
using MediatR;
using Application.GoodFoodUsers;
using API.Middleware;
using Application.Services;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod()
                    .WithOrigins("http://localhost:3000", "http://localhost:5000").AllowCredentials();
                });
            });
            services.AddMediatR(typeof(GetUser.Handler).Assembly);
            services.AddControllers();

            services.AddScoped<IConnectionString, GetDBConnectionString>();
            services.AddScoped<IUserAuth, UserAuth>();
            services.AddScoped<IRecipeGenerator, RecipeGenerator>();
            services.AddScoped<IIngredientGenerator, IngredientGenerator>();
            services.AddScoped<IRecipeIngredientGenerator, RecipeIngredientGenerator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMiddleware<ErrorHandlingMiddleware>();

            // app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("CorsPolicy");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
