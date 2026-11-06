using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Genesis
{
    /// <summary>
    /// Everything the Windows player needs to survive a launch that does not
    /// come from the folder the game was installed in.
    ///
    /// A double click, a desktop shortcut, a link in a browser and the file
    /// manager all start the program differently: the working directory can be
    /// anywhere, the window can start without the foreground and a second copy
    /// can be started by accident. Each of those produced a broken launch:
    /// the game refused to open, opened hidden in the background, or reported
    /// that another instance was already running.
    ///
    /// This class fixes the three causes at their root and never blocks a
    /// launch. Every entry point fails open: when something cannot be done the
    /// game keeps running instead of dying on a broken check.
    /// </summary>
    public static class GenesisWindowsSupport
    {
        /// <summary>Name of the mutex that marks the running copy.</summary>
        private const string MutexName = "Local\\ZendaGenesisSingleInstance";

        /// <summary>Restores a minimized window before it is focused.</summary>
        private const int ShowRestore = 9;

        /// <summary>Shows a window that was created hidden.</summary>
        private const int ShowNormal = 5;

        /// <summary>The nearest monitor is used when a point is not on one.</summary>
        private const uint MonitorDefaultToNearest = 2;

        /// <summary>The width of the primary screen.</summary>
        private const int SystemMetricsScreenWidth = 0;

        /// <summary>The height of the primary screen.</summary>
        private const int SystemMetricsScreenHeight = 1;

        /// <summary>
        /// The show command of a window placement that restores the window to
        /// its normal size and shows it.
        /// </summary>
        private const int PlacementShowNormal = 1;

        /// <summary>
        /// The shape of the game, in design points. It matches the 390x700
        /// column the Sanctum draws, so the window keeps the proportions of
        /// the game when it is not maximized.
        /// </summary>
        public const int DesignWidth = 390;

        /// <summary>The height of the game window in design points.</summary>
        public const int DesignHeight = 700;

        /// <summary>The narrowest window the game accepts.</summary>
        private const int MinimumWindowWidth = 240;

        /// <summary>The shortest window the game accepts.</summary>
        private const int MinimumWindowHeight = 420;

        private static Mutex instanceMutex;
        private static bool claimed;
        private static bool raiseReported;

        /// <summary>
        /// The screen the game was launched on. It is remembered so the
        /// window does not follow the pointer to the other screen while the
        /// game loads.
        /// </summary>
        private static IntPtr launchMonitor;

        /// <summary>True when this process owns the game and no other copy may take over.</summary>
        public static bool IsPrimaryInstance { get; private set; } = true;

        private delegate bool EnumWindowsProc(IntPtr window, IntPtr parameter);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool IsZoomed(IntPtr window);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X, Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInfo
        {
            public int Size;
            public NativeRect Monitor;
            public NativeRect Work;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowPlacement
        {
            public int Length;
            public int Flags;
            public int ShowCommand;
            public NativePoint MinPosition;
            public NativePoint MaxPosition;
            public NativeRect NormalPosition;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr window, uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(NativePoint point, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out NativePoint point);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr window, out NativeRect rect);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr window, ref WindowPlacement placement);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr window, ref WindowPlacement placement);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowTextW")]
        private static extern int GetWindowTextW(IntPtr window, char[] text, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowTextLengthW")]
        private static extern int GetWindowTextLengthW(IntPtr window);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr window, int command);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr window);

        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint attach, uint attachTo, bool doAttach);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", EntryPoint = "SwitchToThisWindow", SetLastError = true)]
        private static extern void SwitchToThisWindow(IntPtr window, bool altTab);

        /// <summary>
        /// Points the working directory at the folder that holds the game.
        ///
        /// Windows starts a program with the working directory of whoever
        /// launched it. A link, a shortcut or the file manager can leave it in a
        /// system folder that must not be written to, and anything the game
        /// resolves against the current directory then goes to the wrong place.
        /// </summary>
        public static bool NormalizeWorkingDirectory()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            try
            {
                string folder = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(folder)) return false;
                string current = Directory.GetCurrentDirectory();
                if (string.Equals(current, folder, StringComparison.OrdinalIgnoreCase)) return true;
                Directory.SetCurrentDirectory(folder);
                Debug.Log("[Genesis] working directory moved from " + current + " to " + folder + ".");
                return true;
            }
            catch (Exception error)
            {
                Debug.LogWarning("[Genesis] the working directory could not be changed. " + error.Message);
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// Claims the single instance lock. Returns false only when another live
        /// copy already owns it. Any failure inside the check is reported as
        /// success, because a broken lock must never stop the game from starting.
        /// </summary>
        public static bool TryClaimInstance()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            try
            {
                if (claimed) return IsPrimaryInstance;
                Mutex mutex = new Mutex(true, MutexName, out bool createdNew);
                if (createdNew)
                {
                    instanceMutex = mutex;
                    claimed = true;
                    IsPrimaryInstance = true;
                    return true;
                }
                // The name already exists. The previous owner may have died
                // without releasing it, in which case the mutex is abandoned
                // and this process takes it back instead of being blocked.
                try
                {
                    if (mutex.WaitOne(0))
                    {
                        instanceMutex = mutex;
                        claimed = true;
                        IsPrimaryInstance = true;
                        return true;
                    }
                }
                catch (AbandonedMutexException)
                {
                    instanceMutex = mutex;
                    claimed = true;
                    IsPrimaryInstance = true;
                    return true;
                }
                mutex.Dispose();
                IsPrimaryInstance = false;
                return false;
            }
            catch (Exception error)
            {
                Debug.LogWarning("[Genesis] single instance check failed, continuing without it. " + error.Message);
                IsPrimaryInstance = true;
                return true;
            }
#else
            IsPrimaryInstance = true;
            return true;
#endif
        }

        /// <summary>
        /// Brings the window of the copy that is already running to the front.
        /// Returns false when no window was found, so the caller keeps running
        /// instead of dying on a stale check.
        /// </summary>
        public static bool TryFocusRunningInstance()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            try
            {
                string product = Application.productName;
                if (string.IsNullOrEmpty(product)) product = "ZendaGenesis";
                uint self = CurrentProcessId();
                IntPtr found = IntPtr.Zero;
                EnumWindows(delegate (IntPtr window, IntPtr parameter)
                {
                    if (found != IntPtr.Zero) return true;
                    if (!IsWindowVisible(window)) return true;
                    uint owner;
                    GetWindowThreadProcessId(window, out owner);
                    if (owner == 0 || owner == self) return true;
                    if (!BelongsToGame(window, owner, product)) return true;
                    found = window;
                    return false;
                }, IntPtr.Zero);
                if (found == IntPtr.Zero)
                {
                    Debug.Log("[Genesis] a copy is already running but its window was not found. This copy keeps running.");
                    return false;
                }
                // Only a window that was minimized has to be restored. SW_RESTORE
                // also shrinks a maximized window back to its normal size, and
                // the copy that owns the window may well have opened itself
                // maximized. Handing the window over must not undo that.
                if (IsIconic(found)) ShowWindow(found, ShowRestore);
                if (!SetForegroundWindow(found)) SwitchToWindow(found);
                Debug.Log("[Genesis] a copy is already running. Its window was brought to the front.");
                return true;
            }
            catch (Exception error)
            {
                Debug.LogWarning("[Genesis] the running copy could not be focused. " + error.Message);
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// Raises the window of this process and gives it the focus.
        ///
        /// A launch through a link does not always hand the foreground to the
        /// new process, and the window can even start minimized. The game then
        /// looked like it had opened hidden in the background with nothing on
        /// it, while it was in fact running perfectly.
        /// </summary>
        public static bool EnsureWindowVisible()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            try
            {
                IntPtr window = FindOwnWindow();
                if (window == IntPtr.Zero)
                {
                    ReportRaise(false, false);
                    return false;
                }
                // A window that was created hidden or minimized has to be
                // shown before anything else can be done with it.
                if (!IsWindowVisible(window)) ShowWindow(window, ShowNormal);
                // SW_RESTORE would also shrink a maximized window back to its
                // normal size, so a window the player maximized by hand is
                // left exactly as it is.
                if (!IsZoomed(window)) ShowWindow(window, ShowRestore);
                if (GetForegroundWindow() == window)
                {
                    ReportRaise(true, true);
                    return true;
                }
                if (SetForegroundWindow(window) && GetForegroundWindow() == window)
                {
                    ReportRaise(true, true);
                    return true;
                }
                // Windows refuses the foreground to a process that does not own
                // it, and a program started from a desktop link is exactly
                // that case. The window then opened behind the program the
                // player was looking at, so the game looked like it never
                // started. Borrowing the input queue of the foreground thread
                // is the only reliable way to take the focus.
                if (ForceForeground(window))
                {
                    ReportRaise(true, true);
                    return true;
                }
                // Last resort: the undocumented switch that ignores the
                // foreground lock altogether.
                SwitchToThisWindow(window, true);
                bool raised = GetForegroundWindow() == window;
                ReportRaise(raised, false);
                return raised;
            }
            catch (Exception error)
            {
                Debug.LogWarning("[Genesis] the window could not be raised. " + error.Message);
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// Gives the window the shape of a phone.
        ///
        /// The game is a mobile game that runs on every platform: the phone,
        /// the browser and the desktop all show the same portrait column. A
        /// wide landscape window would spread that single column over a wide
        /// empty screen and would make the game look like a desktop program,
        /// so the desktop window keeps the proportions of the design and is
        /// scaled up until it fills the height of the monitor.
        ///
        /// The window stays resizable and the player may maximize it at any
        /// time. Restoring it brings the phone shape back, because that shape
        /// is written as the normal placement of the window.
        /// </summary>
        public static bool ApplyMobileWindow()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            try
            {
                IntPtr window = FindOwnWindow();
                if (window == IntPtr.Zero) return false;
                NativeRect rect;
                if (!TryGetPhoneRect(window, out rect)) return false;
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                WindowPlacement placement = new WindowPlacement();
                placement.Length = Marshal.SizeOf(typeof(WindowPlacement));
                if (!GetWindowPlacement(window, ref placement)) return false;
                placement.NormalPosition = rect;
                // The window opens in that shape. It is never opened
                // maximized: a maximized window is a wide landscape screen
                // and the game is a portrait phone game.
                placement.ShowCommand = PlacementShowNormal;
                placement.Flags = 0;
                if (!SetWindowPlacement(window, ref placement)) return false;
                // The window is shown and restored. A window that is left
                // minimized only shows an entry in the task bar and nothing
                // on the desktop, which reads as a game that never opened.
                if (!IsWindowVisible(window)) ShowWindow(window, ShowNormal);
                if (IsIconic(window) || IsZoomed(window)) ShowWindow(window, ShowRestore);
                Debug.Log("[Genesis] the window is a phone shaped " + width + "x" + height
                    + " window at " + rect.Left + "," + rect.Top + ".");
                return true;
            }
            catch (Exception error)
            {
                Debug.LogWarning("[Genesis] the window could not be shaped like a phone. " + error.Message);
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// Keeps the window on the screen the game was launched from while the
        /// game loads.
        ///
        /// Unity restores the window of the last session when the player
        /// starts, and that restore happens after the game has placed its own
        /// window. The game therefore opened on the screen of the previous
        /// session, which on a two screen desk is often the other one. From
        /// the screen the player was working on there was nothing to see:
        /// the task bar showed a small entry and the desktop showed nothing,
        /// and clicking that entry minimized the window for good.
        ///
        /// This puts the window back on the screen the player launched it
        /// from, restores a minimized window and gives the phone shape back
        /// when something else changed it.
        /// </summary>
        public static bool KeepMobileWindow()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            try
            {
                IntPtr window = FindOwnWindow();
                if (window == IntPtr.Zero) return false;
                // A minimized window is a window the player cannot see.
                if (IsIconic(window)) ShowWindow(window, ShowRestore);
                IntPtr current = MonitorFromWindow(window, MonitorDefaultToNearest);
                bool wrongScreen = launchMonitor != IntPtr.Zero && current != launchMonitor;
                bool wrongShape = false;
                if (!wrongScreen)
                {
                    NativeRect wanted;
                    NativeRect actual;
                    if (TryGetPhoneRect(window, out wanted) && GetWindowRect(window, out actual))
                    {
                        wrongShape = Math.Abs((actual.Right - actual.Left) - (wanted.Right - wanted.Left)) > 4
                            || Math.Abs((actual.Bottom - actual.Top) - (wanted.Bottom - wanted.Top)) > 4;
                    }
                }
                if (!wrongScreen && !wrongShape) return false;
                return ApplyMobileWindow();
            }
            catch (Exception error)
            {
                Debug.LogWarning("[Genesis] the window could not be kept on its screen. " + error.Message);
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// Works out the phone shaped rectangle of the window on the screen
        /// the game was launched from. The screen is chosen once and then
        /// remembered, because the pointer moves while the game loads and
        /// the window must not follow it to the other screen halfway through
        /// the start up.
        /// </summary>
        private static bool TryGetPhoneRect(IntPtr window, out NativeRect rect)
        {
            rect = new NativeRect();
            IntPtr monitor = launchMonitor;
            if (monitor == IntPtr.Zero)
            {
                // The window opens on the screen the player is looking at.
                // Unity remembers the screen of the last session and reopens
                // the game there, which put the game on the other monitor of
                // a two screen desk. From the screen the player was working
                // on, the game looked like it never started: only a small
                // entry in the task bar and nothing on the desktop.
                monitor = ChooseLaunchMonitor(window);
                if (monitor == IntPtr.Zero) monitor = MonitorFromWindow(window, MonitorDefaultToNearest);
                if (monitor == IntPtr.Zero) return false;
                launchMonitor = monitor;
            }
            MonitorInfo info = new MonitorInfo();
            info.Size = Marshal.SizeOf(typeof(MonitorInfo));
            if (!GetMonitorInfo(monitor, ref info)) return false;
            int workWidth = info.Work.Right - info.Work.Left;
            int workHeight = info.Work.Bottom - info.Work.Top;
            if (workWidth <= 0 || workHeight <= 0) return false;
            // The window keeps the shape of the game and grows until it
            // fills the screen. The mobile view then uses the whole window
            // instead of a narrow column inside a wide screen.
            float scale = Mathf.Min(workWidth / (float)DesignWidth, workHeight / (float)DesignHeight);
            int width = Mathf.Max(MinimumWindowWidth, Mathf.RoundToInt(DesignWidth * scale));
            int height = Mathf.Max(MinimumWindowHeight, Mathf.RoundToInt(DesignHeight * scale));
            int left = info.Work.Left + (workWidth - width) / 2;
            int top = info.Work.Top + (workHeight - height) / 2;
            rect = new NativeRect
            {
                Left = left,
                Top = top,
                Right = left + width,
                Bottom = top + height
            };
            return true;
        }

        /// <summary>
        /// Chooses the screen the game window opens on.
        ///
        /// The pointer names the screen the player is working on, because the
        /// game is started by a click on a shortcut, an icon or a link. The
        /// window that was in front is the second answer, and the primary
        /// screen is the last one.
        /// </summary>
        private static IntPtr ChooseLaunchMonitor(IntPtr ownWindow)
        {
            NativePoint cursor;
            if (GetCursorPos(out cursor))
            {
                IntPtr monitor = MonitorFromPoint(cursor, MonitorDefaultToNearest);
                if (monitor != IntPtr.Zero) return monitor;
            }
            IntPtr foreground = GetForegroundWindow();
            if (foreground != IntPtr.Zero && foreground != ownWindow)
            {
                IntPtr monitor = MonitorFromWindow(foreground, MonitorDefaultToNearest);
                if (monitor != IntPtr.Zero) return monitor;
            }
            int screenWidth = GetSystemMetrics(SystemMetricsScreenWidth);
            int screenHeight = GetSystemMetrics(SystemMetricsScreenHeight);
            if (screenWidth <= 0 || screenHeight <= 0) return IntPtr.Zero;
            return MonitorFromPoint(new NativePoint { X = screenWidth / 2, Y = screenHeight / 2 },
                MonitorDefaultToNearest);
        }

        /// <summary>
        /// Writes the result of the window raise to the log once, so a launch
        /// that stays in the background can be diagnosed instead of guessed.
        /// </summary>
        private static void ReportRaise(bool raised, bool alreadyInFront)
        {
            if (raiseReported) return;
            raiseReported = true;
            if (raised) Debug.Log("[Genesis] the window was brought to the front.");
            else if (alreadyInFront) Debug.Log("[Genesis] the window was already in front.");
            else Debug.LogWarning("[Genesis] the window could not be brought to the front. The game is running behind the other windows.");
        }

        /// <summary>
        /// Finds the top level window of this process. A visible window with a
        /// title is preferred, but a hidden one is still returned so it can be
        /// shown again.
        /// </summary>
        private static IntPtr FindOwnWindow()
        {
            uint self = CurrentProcessId();
            IntPtr fallback = IntPtr.Zero;
            IntPtr best = IntPtr.Zero;
            EnumWindows(delegate (IntPtr window, IntPtr parameter)
            {
                uint owner;
                GetWindowThreadProcessId(window, out owner);
                if (owner != self) return true;
                if (GetWindowTitle(window) == null) return true;
                if (IsWindowVisible(window)) { best = window; return false; }
                if (fallback == IntPtr.Zero) fallback = window;
                return true;
            }, IntPtr.Zero);
            return best != IntPtr.Zero ? best : fallback;
        }

        /// <summary>
        /// Takes the focus by attaching to the thread that owns the foreground
        /// window for a moment.
        /// </summary>
        private static bool ForceForeground(IntPtr window)
        {
            IntPtr foreground = GetForegroundWindow();
            if (foreground == IntPtr.Zero) return false;
            uint owner;
            uint foregroundThread = GetWindowThreadProcessId(foreground, out owner);
            uint ownThread = GetCurrentThreadId();
            if (foregroundThread == 0 || foregroundThread == ownThread) return false;
            bool attached = AttachThreadInput(ownThread, foregroundThread, true);
            try
            {
                BringWindowToTop(window);
                SetForegroundWindow(window);
                SetActiveWindow(window);
            }
            finally
            {
                if (attached) AttachThreadInput(ownThread, foregroundThread, false);
            }
            return GetForegroundWindow() == window;
        }

        private static uint CurrentProcessId()
        {
            using (System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess())
                return (uint)process.Id;
        }

        /// <summary>
        /// Decides whether a window belongs to the game. The executable name is
        /// the reliable signal; the window title is only used as a fallback so a
        /// renamed executable still works.
        /// </summary>
        private static bool BelongsToGame(IntPtr window, uint ownerProcess, string product)
        {
            string processName = GetProcessName(ownerProcess);
            if (!string.IsNullOrEmpty(processName) && SameName(processName, product)) return true;
            return SameName(GetWindowTitle(window), product);
        }

        private static string GetProcessName(uint processId)
        {
            try
            {
                using (System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById((int)processId))
                    return process.ProcessName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetWindowTitle(IntPtr window)
        {
            try
            {
                int length = GetWindowTextLengthW(window);
                if (length <= 0) return null;
                char[] buffer = new char[length + 1];
                int copied = GetWindowTextW(window, buffer, buffer.Length);
                return copied <= 0 ? null : new string(buffer, 0, copied);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Compares two names ignoring case, spaces and punctuation.</summary>
        private static bool SameName(string left, string right)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right)) return false;
            return string.Equals(Normalize(left), Normalize(right), StringComparison.Ordinal);
        }

        private static string Normalize(string value)
        {
            var builder = new System.Text.StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (char.IsLetterOrDigit(character)) builder.Append(char.ToLowerInvariant(character));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Last resort focus call. Windows blocks SetForegroundWindow when the
        /// caller does not own the foreground, and this undocumented export is
        /// the only reliable way to raise a window from another process.
        /// </summary>
        private static void SwitchToWindow(IntPtr window)
        {
            try
            {
                SwitchToThisWindow(window, true);
            }
            catch (Exception)
            {
                // Nothing else to try. The new copy still closes, the old one
                // simply keeps its own focus state.
            }
        }
    }
}
