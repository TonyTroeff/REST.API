namespace Utilities;

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

    public override string ToString() => string.Join(Environment.NewLine, this._errors.Select(e => e.Message));
}

public class OperationResult<T> : OperationResult
{
    public T Data { get; set; }
}