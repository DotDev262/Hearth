using Hearth.Infrastructure.Windows.Hotkeys;

using var hotkeys = new WindowsHotkeyService();

hotkeys.HotkeyPressed += (_, _) =>
{
    Console.WriteLine("Ctrl+Alt+Space pressed!");
};

hotkeys.Register();

Console.WriteLine("Hearth is running.");
Console.WriteLine("Press Ctrl+Alt+Space to test the global hotkey.");
Console.WriteLine("Press Enter to exit.");

Console.ReadLine();
