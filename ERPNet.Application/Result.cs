using ERPNet.Application.Enums;

namespace ERPNet.Application;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ErrorType? ErrorType { get; }
    public List<string>? Errors { get; }

    protected Result(bool isSuccess, string? error, ErrorType? errorType, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
        Errors = errors;
    }

    public static Result Success() => new(true, null, null);

    public static Result Failure(string error, ErrorType errorType) =>
        new(false, error, errorType);

    public static Result ValidationFailure(List<string> errors) =>
        new(false, "Error de validacion.", Enums.ErrorType.Validation, errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T? value, bool isSuccess, string? error, ErrorType? errorType, List<string>? errors = null)
        : base(isSuccess, error, errorType, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, null, null);

    public new static Result<T> Failure(string error, ErrorType errorType) =>
        new(default, false, error, errorType);

    public new static Result<T> ValidationFailure(List<string> errors) =>
        new(default, false, "Error de validacion.", Enums.ErrorType.Validation, errors);
}
