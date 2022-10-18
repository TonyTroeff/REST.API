namespace API.Extensions;

using API.ContentNegotiation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;
using Utilities;

public static class ControllerExtensions
{
    public static ActionResult Error(this ControllerBase controller, OperationResult operationResult)
    {
        if (controller is null) throw new ArgumentNullException(nameof(controller));
        if (operationResult is null) throw new ArgumentNullException(nameof(operationResult));

        var statusCode = operationResult.Errors.Any(e => e.IsNotExpected) ? StatusCodes.Status500InternalServerError : StatusCodes.Status400BadRequest;
        return controller.Problem(operationResult.ToString(), controller.Request.Path, statusCode, "Your actions was not executed successfully.");
    }

    public static ActionResult ValidationError(this ControllerBase controller, ValidationResult validationResult)
    {
        if (controller is null) throw new ArgumentNullException(nameof(controller));
        if (validationResult is null) throw new ArgumentNullException(nameof(validationResult));
        
        var problemDetailsFactory = controller.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        var modelStateDictionary = new ModelStateDictionary();
        foreach (var validationError in validationResult.Errors) modelStateDictionary.AddModelError(validationError.PropertyName, validationError.ErrorMessage);

        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(controller.HttpContext, modelStateDictionary, StatusCodes.Status422UnprocessableEntity, "Your input could not fulfil some of our validation rules");
        return controller.UnprocessableEntity(problemDetails);
    }

    public static string AbsoluteActionUrl(this ControllerBase controller, string actionName, string controllerName, object values)
    {
        if (controller is null) throw new ArgumentNullException(nameof(controller));

        var request = controller.HttpContext.Request;
        return controller.Url.Action(actionName, controllerName, values, request.Scheme, request.Host.Value);
    }
}