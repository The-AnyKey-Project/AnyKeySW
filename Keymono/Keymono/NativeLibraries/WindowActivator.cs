using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace KeyboardHookTest
{
    class WindowActivator
    {


        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        private enum WindowState
        {
            Unknown = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3
        }

        public string ProcessName
        {
            get; set;
        }

        public string Filename
        {
            get; set;
        }

        public string Arguments
        {
            get; set;
        }

        public bool ActivateOldProcess
        {
            get; set;

        }

        private Process Process;
        private bool isRunning = false;
        private IntPtr lastActiveWindow = IntPtr.Zero;
        
        public WindowActivator()
        {
            this.ProcessName = null;
            this.Filename = "";
            this.Arguments = null;
            this.Process = null;

        }

        public bool Activate()
        {
            bool result = false;

            if (ActivateOldProcess && GetProcessesByName(this.ProcessName))
            {
                isRunning = true;
                ToggleWindow(this.Process.MainWindowHandle);
            }
            else
            {
                if (!isRunning)       // Prozess muss noch gestartet werden.
                {

                    this.Process = Process.Start(this.Filename, this.Arguments);

                    try
                    {
                        this.Process.WaitForInputIdle();

                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                    isRunning = ProcessExists(this.Process.Id);
                    if (!isRunning)
                    {
                        Activate();
                    }
                    else
                    {
                        this.ProcessName = this.Process.ProcessName;
                    }
                }
                else
                {
                    if (ProcessExists(this.Process.Id))
                    {
                        if (IsWindow(this.Process.MainWindowHandle))
                        {
                            ToggleWindow(this.Process.MainWindowHandle);
                        }
                        else
                        {
                            throw new Exception("MainWindowHandle not found!");
                        }
                    }
                    else
                    {
                        isRunning = false;
                        Activate();
                        ;
                    }
                }
            }
            return result;
        }

        private void ToggleWindow(IntPtr Handle)
        {
            IntPtr activeWindow = GetForegroundWindow();

            Console.WriteLine("Active Window : " + activeWindow.ToString());
            Console.WriteLine("Current Handle: " + Handle.ToString());
            Console.WriteLine("Last Handle   : " + lastActiveWindow.ToString());

            if (activeWindow != Handle)
            {
                if (GetWindowState(Handle) == WindowState.Minimized)
                    ShowWindow(Handle, SW_RESTORE);
                SetForegroundWindow(Handle);
                lastActiveWindow = activeWindow;
            }
            else
            {
                SetForegroundWindow(lastActiveWindow);
                lastActiveWindow = Handle;
            }

        }

        private WindowState GetWindowState(IntPtr Handle)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            GetWindowPlacement(Handle, ref placement);
            return (WindowState)placement.showCmd;
            /*
            switch (placement.showCmd)
            {
                case 1:
                    return WindowState.Normal;
                case 2:
                    return WindowState.Minimized;
                case 3:
                    return WindowState.Maximized;
            }
            return WindowState.Unknown;
            */
        }

        private bool ProcessExists(int id)
        {
            return Process.GetProcesses().Any(x => x.Id == id);
        }
        private bool GetProcessesByName(string ProcessName)
        {
            foreach (Process p in Process.GetProcessesByName(ProcessName))
            {
                this.Process = p;
                return true;
            }
            return false;
        }

    }
}
