namespace API.Extensions;

using Microsoft.AspNetCore.Mvc;
using Utilities;

public static class ControllerExtensions
{
    public static ActionResult Error(this ControllerBase controller, OperationResult operationResult)
    {
        if (controller is null) throw new ArgumentNullException(nameof(controller));
        if (operationResult is null) throw new ArgumentNullException(nameof(operationResult));

        return controller.Problem(operationResult.ToString(), controller.Request.Path, StatusCodes.Status400BadRequest, "Your actions was not executed successfully.");
    }
}