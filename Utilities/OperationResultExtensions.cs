namespace Utilities;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public static class OperationResultExtensions
{
#nullable enable
    public static bool ValidateNotNull<TValue>(this OperationResult operationResult, [NotNullWhen(true)] TValue? value, [CallerFilePath] string? filePath = null, [CallerMemberName] string? memberName = null, [CallerLineNumber] int line = -1, [CallerArgumentExpression("value")] string? expression = null)
    {
        if (operationResult is null) throw new ArgumentNullException(nameof(operationResult));
        if (value is not null) return true;

        var error = new Error { Message = $"{FormatErrorMessage(filePath, memberName, line, expression)} should not be null." };
        operationResult.AddError(error);
        return false;
    }
#nullable restore
    
    public static bool ValidateNotNullOrWhitespace(this OperationResult operationResult, string value, [CallerFilePath] string filePath = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int line = -1, [CallerArgumentExpression("value")] string expression = null)
    {
        if (operationResult is null) throw new ArgumentNullException(nameof(operationResult));
        if (string.IsNullOrWhiteSpace(value) == false) return true;

        var error = new Error { Message = $"{FormatErrorMessage(filePath, memberName, line, expression)} should not be null, empty or whitespace." };
        operationResult.AddError(error);
        return false;
    }
    
    public static bool ValidateNotDefault<TValue>(this OperationResult operationResult, TValue value, [CallerFilePath] string filePath = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int line = -1, [CallerArgumentExpression("value")] string expression = null)
        where TValue : struct, IEquatable<TValue>
    {
        if (operationResult is null) throw new ArgumentNullException(nameof(operationResult));
        if (value.Equals(default) == false) return true;

        var error = new Error { Message = $"{FormatErrorMessage(filePath, memberName, line, expression)} should not be default." };
        operationResult.AddError(error);
        return false;
    }

    public static void AddException(this OperationResult operationResult, Exception exception)
    {
        if (operationResult is null) throw new ArgumentNullException(nameof(operationResult));
        if (exception is null) return;

        var error = new Error { Message = exception.Message };
        operationResult.AddError(error);
    }

    public static TOperationResult AppendErrors<TOperationResult>(this TOperationResult principal, OperationResult other)
        where TOperationResult : OperationResult
    {
        if (principal is null) throw new ArgumentNullException(nameof(principal));

        foreach (var error in (other?.Errors).OrEmptyIfNull().IgnoreNullValues()) principal.AddError(error);
        return principal;
    }

    public static OperationResult<TData> WithData<TData>(this OperationResult<TData> operationResult, TData data)
    {
        if (operationResult is null) throw new ArgumentNullException(nameof(operationResult));

        operationResult.Data = data;
        return operationResult;
    }

    private static string FormatErrorMessage(string filePath, string memberName, int line, string argumentExpression) => $"{filePath} ({memberName};{line}) - Expression [{argumentExpression}]";
}