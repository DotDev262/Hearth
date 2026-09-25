namespace Hearth.Core.Abstractions;

public interface IHotkeyService : IDisposable
{
    event EventHandler? HotkeyPressed;

    void Register();

    void Unregister();
}
