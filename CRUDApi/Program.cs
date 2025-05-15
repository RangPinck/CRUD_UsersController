
using CRUDApi.Interfaces;
using CRUDApi.Models;
using CRUDApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace CRUDApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CRUDApi", Version = "v1" });
                c.EnableAnnotations();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter correct authorization token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new List<string>()
                    }
                });
            });

            builder.Services.AddDbContext<CrudApiDbContext>(options =>
            {
                options.UseNpgsql(
                     builder.Configuration.GetValue<string>("ConnectionStrings:ConnectionDatabase")
                );
            });

            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IHealthRepository, HealthRepository>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            var validIssuer = builder.Configuration.GetValue<string>("JWT:Issuer");
            var validAudience = builder.Configuration.GetValue<string>("JWT:Audience");
            var symmetricSecurityKey = builder.Configuration.GetValue<string>("JWT:SigningKey");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = validIssuer,
                        ValidAudience = validAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(symmetricSecurityKey!))
                    };

                    options.SaveToken = true;
                    options.IncludeErrorDetails = true;

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();

                            var httpContext = context.HttpContext;
                            var statusCode = StatusCodes.Status401Unauthorized;

                            var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                            var problemDetails = factory.CreateProblemDetails(httpContext, statusCode);

                            var result = new ObjectResult(problemDetails) { StatusCode = statusCode };
                            await result.ExecuteResultAsync(new ActionContext
                            {
                                HttpContext = httpContext,
                                RouteData = httpContext.GetRouteData(),
                                ActionDescriptor = new ActionDescriptor()
                            });
                        },

                        OnForbidden = async context =>
                        {
                            var httpContext = context.HttpContext;
                            var statusCode = StatusCodes.Status403Forbidden;

                            var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                            var problemDetails = factory.CreateProblemDetails(httpContext, statusCode);

                            var result = new ObjectResult(problemDetails) { StatusCode = statusCode };
                            await result.ExecuteResultAsync(new ActionContext
                            {
                                HttpContext = httpContext,
                                RouteData = httpContext.GetRouteData(),
                                ActionDescriptor = new ActionDescriptor()
                            });
                        }
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            builder.Services.AddOpenApi();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(swagger =>
                {
                    swagger.SwaggerEndpoint("/swagger/v1/swagger.json", "CRUDApi v1");
                });
            }

            using (var scope = app.Services.CreateAsyncScope())
            {
                var service = scope.ServiceProvider;

                try
                {
                    string adminName = builder.Configuration.GetValue<string>("StandardAdmin:Name");
                    string adminLogin = builder.Configuration.GetValue<string>("StandardAdmin:Login");
                    string adminPassword = builder.Configuration.GetValue<string>("StandardAdmin:Password");
                    string adminGender = builder.Configuration.GetValue<string>("StandardAdmin:Gender");

                    var context = service.GetRequiredService<CrudApiDbContext>();

                    await DbInitializer.InitializeAsync(context, (adminLogin, adminName, adminPassword, int.Parse(adminGender)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
