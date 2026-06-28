namespace HobbyApp.Application.Common.Models;

/// <summary>
/// Lightweight result wrapper used by Application services to convey success
/// or a set of human-readable errors without throwing for expected failures.
/// </summary>
public sealed class Result<T>
{
    public bool Succeeded { get; private init; }

    public T? Value { get; private init; }

    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static Result<T> Success(T value) =>
        new() { Succeeded = true, Value = value };

    public static Result<T> Failure(params string[] errors) =>
        new() { Succeeded = false, Errors = errors };

    public static Result<T> Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors.ToArray() };
}
