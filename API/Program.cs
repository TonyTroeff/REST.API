using System.Reflection;
using API.Configuration;
using API.Models.Configuration;
using Core.Configuration;
using Data.Configuration;
using Microsoft.AspNetCore.Mvc;
using Utilities;

var builder = WebApplication.CreateBuilder(args);

// You can use this file in order to define local-only configurations relevant to your development setup only. An additional .gitignore rule is added for this file.
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);

builder.Services.AddControllers(
        options =>
        {
            // If an unsupported media type is requested - return 406 NotAcceptable response.
            options.ReturnHttpNotAcceptable = true;
        })
    .ConfigureApiBehaviorOptions(
        options =>
        {
            options.ClientErrorMapping[StatusCodes.Status400BadRequest] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1" };
            options.ClientErrorMapping[StatusCodes.Status401Unauthorized] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.2" };
            options.ClientErrorMapping[StatusCodes.Status402PaymentRequired] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.3" };
            options.ClientErrorMapping[StatusCodes.Status403Forbidden] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.4" };
            options.ClientErrorMapping[StatusCodes.Status404NotFound] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.5" };
            options.ClientErrorMapping[StatusCodes.Status405MethodNotAllowed] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.6" };
            options.ClientErrorMapping[StatusCodes.Status406NotAcceptable] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.7" };
            options.ClientErrorMapping[StatusCodes.Status407ProxyAuthenticationRequired] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.8" };
            options.ClientErrorMapping[StatusCodes.Status408RequestTimeout] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.9" };
            options.ClientErrorMapping[StatusCodes.Status409Conflict] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.10" };
            options.ClientErrorMapping[StatusCodes.Status410Gone] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.11" };
            options.ClientErrorMapping[StatusCodes.Status411LengthRequired] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.12" };
            options.ClientErrorMapping[StatusCodes.Status412PreconditionFailed] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.13" };
            options.ClientErrorMapping[StatusCodes.Status413PayloadTooLarge] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.14" };
            options.ClientErrorMapping[StatusCodes.Status414UriTooLong] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.15" };
            options.ClientErrorMapping[StatusCodes.Status415UnsupportedMediaType] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.16" };
            options.ClientErrorMapping[StatusCodes.Status416RangeNotSatisfiable] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.17" };
            options.ClientErrorMapping[StatusCodes.Status417ExpectationFailed] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.18" };
            options.ClientErrorMapping[StatusCodes.Status418ImATeapot] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.19" };
            options.ClientErrorMapping[StatusCodes.Status421MisdirectedRequest] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.20" };
            options.ClientErrorMapping[StatusCodes.Status422UnprocessableEntity] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.21" };
            options.ClientErrorMapping[StatusCodes.Status426UpgradeRequired] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.22" };
            
            options.ClientErrorMapping[StatusCodes.Status500InternalServerError] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.1" };
            options.ClientErrorMapping[StatusCodes.Status501NotImplemented] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.2" };
            options.ClientErrorMapping[StatusCodes.Status502BadGateway] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.3" };
            options.ClientErrorMapping[StatusCodes.Status503ServiceUnavailable] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.4" };
            options.ClientErrorMapping[StatusCodes.Status504GatewayTimeout] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.5" };
            options.ClientErrorMapping[StatusCodes.Status505HttpVersionNotsupported] = new ClientErrorData { Link = "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.6" };

            // If you use data annotations for validation of your input model - preserve this segment here. It will return "422 Unprocessable entity" instead of "400 Bad request" when a validation rule is not fulfilled. 
            /*
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

                return new ObjectResult(problemDetails) { StatusCode = data.StatusCode };
            };
            */
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

builder.Services.ConfigureApiModelValidators();
builder.Services.ConfigureServices();
builder.Services.ConfigureContentNegotiation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            // Hide the "Schema" section since the correlation between Swagger and FluentValidation is simply missing.
            options.DefaultModelsExpandDepth(-1);
        });
}

app.UseCors();
app.UseHttpsRedirection();

// We will not be talking about authentication and authorization strategies and best practices at the seminar as this is a completely different (and complex) topic that deserves a separate seminar on its own. 
// app.UseAuthorization();

app.MapControllers();

await app.RunAsync();