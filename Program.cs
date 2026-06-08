
using E_Commerce_System;
using E_Commerce_System_API.Serviece;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;

namespace E_Commerce_System_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Services
            builder.Services.AddControllers();

            //register services for dependency injection
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
             options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register JwtService for dependency injection
            builder.Services.AddScoped<JwtService>();

            // Register LoggingService for dependency injection
            builder.Services.AddScoped<LoggingService>();

            var jwtKey = builder.Configuration["Jwt:Key"]!;

            // Configure JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero   // No extra expiry buffer
                };
            });

            builder.Services.AddAuthorization();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer <token>"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Scheme = "bearer",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
               }
              });
            });

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddDataProtection();

            var app = builder.Build();

            // Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.EnablePersistAuthorization();  // ? keeps the token after page refresh
                });
            }

            // Global Error Handling
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new
                    {
                        Message = "Something went wrong."
                    });
                });
            });

            app.UseHttpsRedirection();

            app.UseAuthentication();   // ? must come BEFORE UseAuthorization

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
