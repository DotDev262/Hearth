namespace Hearth.Core.Commands;

public sealed record CommandResult
{
    public bool Success { get; init; }

    public string? Output { get; init; }

    public string? Error { get; init; }

    public static CommandResult Ok(string? output = null)
        => new()
        {
            Success = true,
            Output = output
        };

    public static CommandResult Fail(string error)
        => new()
        {
            Success = false,
            Error = error
        };
}
