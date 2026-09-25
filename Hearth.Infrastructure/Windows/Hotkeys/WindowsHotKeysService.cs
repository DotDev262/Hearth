using System.ComponentModel;
using System.Runtime.InteropServices;
using Hearth.Core.Abstractions;

namespace Hearth.Infrastructure.Windows.Hotkeys;

public sealed class WindowsHotkeyService : IHotkeyService
{
    private const int HotkeyId = 1;

    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint HotkeyModifiers = ModAlt | ModControl;
    private const uint VkSpace = 0x20;

    private const uint WmHotkey = 0x0312;
    private const uint WmQuit = 0x0012;

    private Thread? _messageThread;
    private uint _threadId;
    private volatile bool _running;

    public event EventHandler? HotkeyPressed;

    public void Register()
    {
        if (_running)
            return;

        _running = true;

        _messageThread = new Thread(MessageLoop)
        {
            IsBackground = true,
            Name = "Hearth.HotkeyMessageLoop"
        };

        _messageThread.Start();
    }

    public void Unregister()
    {
        if (!_running)
            return;

        _running = false;

        if (_threadId != 0)
        {
            PostThreadMessage(
                _threadId,
                WmQuit,
                UIntPtr.Zero,
                IntPtr.Zero);
        }

        if (_messageThread is not null &&
            _messageThread != Thread.CurrentThread)
        {
            _messageThread.Join(TimeSpan.FromSeconds(2));
        }

        _messageThread = null;
        _threadId = 0;
    }

    public void Dispose()
    {
        Unregister();
    }

    private void MessageLoop()
    {
        _threadId = GetCurrentThreadId();

        if (!RegisterHotKey(
                IntPtr.Zero,
                HotkeyId,
                HotkeyModifiers,
                VkSpace))
        {
            var error = Marshal.GetLastWin32Error();

            _running = false;
            _threadId = 0;

            throw new Win32Exception(
                error,
                "Failed to register Ctrl+Alt+Space hotkey.");
        }

        try
        {
            while (true)
            {
                var result = GetMessage(
                    out var message,
                    IntPtr.Zero,
                    0,
                    0);

                if (result == -1)
                {
                    var error = Marshal.GetLastWin32Error();

                    throw new Win32Exception(
                        error,
                        "Failed while reading the Windows message queue.");
                }

                if (result == 0)
                {
                    // WM_QUIT received.
                    break;
                }

                if (message.MessageId == WmHotkey &&
                    message.WParam == (UIntPtr)HotkeyId)
                {
                    HotkeyPressed?.Invoke(
                        this,
                        EventArgs.Empty);
                }

                TranslateMessage(ref message);
                DispatchMessage(ref message);
            }
        }
        finally
        {
            UnregisterHotKey(
                IntPtr.Zero,
                HotkeyId);

            _running = false;
            _threadId = 0;
        }
    }

    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern bool RegisterHotKey(
        IntPtr hWnd,
        int id,
        uint fsModifiers,
        uint vk);

    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern bool UnregisterHotKey(
        IntPtr hWnd,
        int id);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetMessage(
        out Message lpMsg,
        IntPtr hWnd,
        uint minFilter,
        uint maxFilter);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(
        ref Message lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(
        ref Message lpMsg);

    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern bool PostThreadMessage(
        uint threadId,
        uint message,
        UIntPtr wParam,
        IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [StructLayout(LayoutKind.Sequential)]
    private struct Message
    {
        public IntPtr HWnd;
        public uint MessageId;
        public UIntPtr WParam;
        public IntPtr LParam;
        public uint Time;
        public Point Point;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }
}
