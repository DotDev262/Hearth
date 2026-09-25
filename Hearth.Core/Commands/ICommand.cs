namespace Hearth.Core.Commands;

public interface ICommands{
    string Id { get; }
    string Name { get; }

    Task<CommandResult> ExecuteAsync(
        CommandContext context, CancellationToken cancellationToken
    );
}
