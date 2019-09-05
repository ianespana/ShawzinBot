using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shapes;

using InputManager;
using Timer = System.Threading.Timer;

namespace ShawzinBot
{
    public class ActionManager
    {
        public const UInt32 WM_KEYDOWN = 0x0100;
        public const UInt32 WM_KEYUP = 0x0101;
        public const UInt32 WM_SETTEXT = 0x000C;

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };

        private struct WindowPlacement
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
            public Rectangle rcDevice;
        }

        /// <summary>Enumeration of the different ways of showing a window using
        /// ShowWindow</summary>
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string windowTitle);

        public static IntPtr FindWindow(string lpWindowName)
        {
            return FindWindow(null, lpWindowName);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern IntPtr GetForegroundWindow();


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("User32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindows);

        [DllImport("User32.dll")]
        private static extern Int32 SendMessage(IntPtr hWnd, UInt32 Msg, char wParam, UInt32 lParam);

		public static IntPtr warframeWindow = IntPtr.Zero; //TODO turn this into a non-global variable

        public static void SendNote(int noteId)
        {
            /*BringWindowToFront("new 3 - Notepad++");
            Process[] processes = Process.GetProcessesByName("new 3 - Notepad++");
            foreach (char key in "Hi!")
            {
                //Enum.TryParse<Keys>("" + key, out Keys tempKey);
                Keys tempKey = (Keys)char.ToUpper(key);
                foreach (Process proc in processes)
                {
                    PostMessage(proc.MainWindowHandle, WM_KEYDOWN, (int)tempKey, 0);
                }
            }*/

            IntPtr hWnd = GetForegroundWindow();
            if (!warframeWindow.Equals(IntPtr.Zero) && hWnd.Equals(warframeWindow))
			{
				KeyTap(Keys.D1); //TODO Replace with whatever key the note should be
            }
        }

		public static void KeyTap(Keys key)
		{
			Keyboard.KeyDown(key);
			Keyboard.KeyUp(key);
		}

		public static void KeyHold(Keys key, TimeSpan time) //Hold key for certain amount of time and release, untested
		{
			Keyboard.KeyDown(key);
			new Timer(state=>Keyboard.KeyUp(key), null, time, Timeout.InfiniteTimeSpan);
		}

        public static void BringWindowToFront(string windowName)
        {
            IntPtr wdwIntPtr = FindWindow(windowName, null);
			BringWindowToFront(wdwIntPtr);
        }
		public static void BringWindowToFront(IntPtr wdwIntPtr)
		{
			//get the hWnd of the process
			WindowPlacement placement = new WindowPlacement();
			GetWindowPlacement(wdwIntPtr, ref placement);

			// Check if window is minimized
			if (placement.showCmd == 2)
			{
				//the window is hidden so we restore it
				ShowWindow(wdwIntPtr, ShowWindowEnum.Restore);
			}

			//set user's focus to the window
			int focusResult = SetForegroundWindow(wdwIntPtr);
			Console.WriteLine(focusResult);
		}
	}
}
