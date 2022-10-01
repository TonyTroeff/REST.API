namespace Utilities;

using System.Text;

public class OperationResult
{
    private readonly List<Error> _errors = new();

    public IReadOnlyCollection<Error> Errors => this._errors.AsReadOnly();

    public bool IsSuccessful => !this.Errors.Any();

    public bool AddError(Error error)
    {
        if (error is null) return false;

        this._errors.Add(error);
        return true;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var error in this._errors)
            sb.AppendLine(error.Message);

        return sb.ToString();
    }
}

public class OperationResult<T> : OperationResult
{
    public T Data { get; set; }
}