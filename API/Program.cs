using System.Net;
using System.Reflection;
using API.Configuration;
using Core.Configuration;
using Data.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Utilities;

var builder = WebApplication.CreateBuilder(args);

// You can use this file in order to define local-only configurations relevant to your development setup only. An additional .gitignore rule is added for this file.
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(
        options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                if (actionContext.ModelState.IsValid) throw new InvalidOperationException("The model state should be invalid.");
                if (actionContext is not ActionExecutingContext actionExecutingContext) throw new InvalidOperationException("The action context is not an action executing context and this response factory cannot work correctly.");

                var resolvedParameters = actionExecutingContext.ActionArguments;
                var definedParameters = actionContext.ActionDescriptor.Parameters;

                (string Title, int StatusCode) data;
                if (resolvedParameters.Count != definedParameters.Count) data = ("You input is not well structured.", StatusCodes.Status400BadRequest);
                else data = ("Your input could not fulfil some of our validation rules", StatusCodes.Status422UnprocessableEntity);
                
                var problemDetailsFactory = actionContext.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>(); 
                var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(actionContext.HttpContext, actionContext.ModelState, data.StatusCode, data.Title);

                return new ObjectResult(problemDetails) { StatusCode = data.StatusCode, ContentTypes = { "application/json+problem" } };
            };
        });

// Configure the used database.
var databaseConfiguration = builder.Configuration.GetSection(DatabaseConfiguration.Section).Get<DatabaseConfiguration>();
builder.Services.SetupDatabase(databaseConfiguration);

// Configure the CORS policy.
var corsConfiguration = builder.Configuration.GetSection(CorsConfiguration.Section).Get<CorsConfiguration>();
builder.Services.AddCors(
    options =>
    {
        options.AddDefaultPolicy(
            policyBuilder =>
            {
                policyBuilder.WithHeaders(corsConfiguration.AllowedHeaders.OrEmptyIfNull().IgnoreNullValues().ToArray());
                policyBuilder.WithMethods(corsConfiguration.AllowedMethods.OrEmptyIfNull().IgnoreNullValues().ToArray());
                policyBuilder.WithOrigins(corsConfiguration.AllowedOrigins.OrEmptyIfNull().IgnoreNullValues().ToArray());

                if (corsConfiguration.AllowCredentials) policyBuilder.AllowCredentials();
                else policyBuilder.DisallowCredentials();
            });
    });

// Configure Automapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

// We will not be talking about authentication and authorization strategies and best practices at the seminar as this is a completely different (and complex) topic that deserves a separate seminar on its own. 
// app.UseAuthorization();

app.MapControllers();

await app.RunAsync();