﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Companies.API.Data;
using Companies.API.Extensions;
using Companies.API.Mappings;
using Companies.API.Middleware;
using Companies.API.Repositorys;
using Companies.API.Services;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Companies.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;

                options.Filters.Add(
                    new ProducesResponseTypeAttribute(
                        StatusCodes.Status400BadRequest)); 
                options.Filters.Add(
                    new ProducesResponseTypeAttribute(
                        StatusCodes.Status406NotAcceptable));
                options.Filters.Add(
                    new ProducesResponseTypeAttribute(
                        StatusCodes.Status500InternalServerError));
            })
            .AddNewtonsoftJson();
                //.AddXmlDataContractSerializerFormatters();

            builder.Services.AddDbContext<APIContext>(options =>
              options.UseSqlServer(builder.Configuration.GetConnectionString("APIContext") 
              ?? throw new InvalidOperationException("Connection string 'APIContext' not found.")));

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc("CompaniesOpenAPISpecification", new()
                {
                    Title = "Companies API",
                    Version = "1",
                    Description = "Through this API you can get information about companies and their employees",
                    Contact = new()
                    {
                        Email = "jonathan.krall@lexicon.se",
                        Name = "Jonathan Krall",
                        Url = new Uri("https://www.lexicon.se")
                    },
                    License = new()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }

                });
                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

                setupAction.IncludeXmlComments(xmlPath);

            });
            builder.Services.AddAutoMapper(typeof(CompanyMappings));
            builder.Services.AddRepositories();
            
            var app = builder.Build();

            //app.UseExceptionHandler();

            app.UseConfigureExceptionHandler();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI( setupAction =>
                {
                    setupAction.SwaggerEndpoint("/swagger/CompaniesOpenAPISpecification/swagger.json",
                        "Company API");
                    setupAction.RoutePrefix = "swagger";
                });
                await app.SeedDataAsync();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseDemo();

            //app.Map("/hej", builder =>
            //{
            //    builder.Run(async context =>
            //    {
            //        await context.Response.WriteAsync("Application return if route was hej!");
            //    });
            //});


            app.MapControllers();

            app.Run();
        }
    }
}
