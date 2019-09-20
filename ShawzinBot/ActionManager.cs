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
using System.Windows.Input;
using System.Windows.Shapes;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Keyboard = InputManager.Keyboard;
using Timer = System.Threading.Timer;

namespace ShawzinBot
{
    public class ActionManager
    {
        public const UInt32 WM_KEYDOWN = 0x0100;
        public const UInt32 WM_KEYUP = 0x0101;
        public const UInt32 WM_SETTEXT = 0x000C;

        private static IntPtr warframeWindow = IntPtr.Zero;

        // Dictionary of note IDs and a series of ints. In order: Scale, Fret, Key, Vibrato
        private static Dictionary<int, int[]> shawzinNotes = new Dictionary<int, int[]>
        {
            { 48, new[] {0,0,1,0} }, // C3
            { 49, new[] {0,0,2,0} }, // C#3
            { 50, new[] {0,0,3,0} }, // D3
            { 51, new[] {0,1,1,0} }, // D#3
            { 52, new[] {0,1,2,0} }, // E3
            { 53, new[] {0,1,3,0} }, // F3
            { 54, new[] {0,2,1,0} }, // F#3
            { 55, new[] {0,2,2,0} }, // G3
            { 56, new[] {0,2,3,0} }, // G#3
            { 57, new[] {0,3,1,0} }, // A3
            { 58, new[] {0,3,2,0} }, // A#3
            { 59, new[] {0,3,3,0} }, // B3
            { 60, new[] {8,1,3,0} }, // C4
            { 61, new[] {4,2,1,0} }, // C#4
            { 62, new[] {8,2,1,0} }, // D4
            { 63, new[] {1,2,2,0} }, // D#4
            { 64, new[] {8,2,2,0} }, // E4
            { 65, new[] {1,2,3,0} }, // F4
            { 66, new[] {1,3,1,0} }, // F#4
            { 67, new[] {8,2,3,0} }, // G4
            { 68, new[] {6,2,3,0} }, // G#4
            { 69, new[] {8,3,1,0} }, // A4
            { 70, new[] {1,3,3,0} }, // A#4
            { 71, new[] {4,3,2,1} }, // B4
            { 72, new[] {4,3,2,0} }, // C5
            { 73, new[] {4,3,3,0} }, // C#5
            { 74, new[] {8,3,3,0} }, // D5
            { 75, new[] {7,3,3,0} }, // D#5
        };

        private static Dictionary<int, Keys> shawzinFrets = new Dictionary<int, Keys>
        {
            { 0, Keys.None }, // Sky Fret
            { 1, Keys.Left }, // Sky Fret
            { 2, Keys.Down }, // Earth Fret
            { 3, Keys.Right }, // Water Fret
        };

        private static Dictionary<int, Keys> shawzinStrings = new Dictionary<int, Keys>
        {
            { 1, Keys.D1 }, // 1st String
            { 2, Keys.D2 }, // 2nd String
            { 3, Keys.D3 }, // 3rd String
        };


        private static Dictionary<int, Keys> shawzinSpecial = new Dictionary<int, Keys>
        {
            { 0, Keys.Space }, // Vibrato
            { 1, Keys.Tab }, // Scale change
        };

        private static int scaleSize = 9;

        private static int activeScale = 0;

        private static bool vibratoActive = false;

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

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindows);

        [DllImport("user32.dll")]
        private static extern Int32 SendMessage(IntPtr hWnd, UInt32 Msg, char wParam, UInt32 lParam);

        /// <summary>
        /// Play a MIDI note inside Warframe.
        /// </summary>
        /// <param name="note"> The note to be played.</param>
        /// <param name="enableVibrato"> Should we use vibrato to play unplayable notes?.</param>
        /// <param name="transposeNotes"> Should we transpose unplayable notes?.</param>
        public static void PlayNote(NoteOnEvent note, bool enableVibrato, bool transposeNotes)
        {
            var hWnd = GetForegroundWindow();
            if (warframeWindow.Equals(IntPtr.Zero) || !hWnd.Equals(warframeWindow)) return;

            var noteId = (int) note.NoteNumber;
            if (!shawzinNotes.ContainsKey(noteId))
            {
                if (transposeNotes)
                {
                    if (noteId < shawzinNotes.Keys.First())
                    {
                        noteId = shawzinNotes.Keys.First() + noteId % 12;
                    }
                    else if (noteId > shawzinNotes.Keys.Last())
                    {
                        noteId = shawzinNotes.Keys.Last() - 11 + noteId % 12;
                    }
                }
                else
                {
                    return;
                }
            }

            var shawzinNote = shawzinNotes[noteId];
            PlayNote(noteId, enableVibrato, transposeNotes);
        }

        /// <summary>
        /// Play a MIDI note inside Warframe.
        /// </summary>
        /// <param name="noteId"> The MIDI ID of the note to be played.</param>
        /// <param name="enableVibrato"> Should we use vibrato to play unplayable notes?.</param>
        /// <param name="transposeNotes"> Should we transpose unplayable notes?.</param>
        public static void PlayNote(int noteId, bool enableVibrato, bool transposeNotes)
        {
            var shawzinNote = shawzinNotes[noteId];
            SetScale(shawzinNote[0]);
            var stringKey = shawzinStrings[shawzinNote[2]];
            var fretKey = shawzinFrets[shawzinNote[1]];

            var vibratoKey = shawzinSpecial[0];

            if (shawzinNote[3] == 1 && enableVibrato)
            {
                KeyHold(vibratoKey, TimeSpan.FromMilliseconds(100));
                //Keyboard.KeyDown(vibratoKey);
            }

            Keyboard.KeyDown(fretKey);
            KeyTap(stringKey);
            Keyboard.KeyUp(fretKey);
            //Keyboard.KeyUp(vibratoKey);
        }

        public static void SetScale(int scaleIndex)
        {
            var scaleDifference = 0;

            if (scaleIndex < activeScale)
            {
                scaleDifference = scaleSize - (activeScale - scaleIndex);
            }
            else if (scaleIndex > activeScale)
            {
                scaleDifference = scaleIndex - activeScale;
            }

            for (var i = 0; i < scaleDifference; i++)
            {
                KeyTap(shawzinSpecial[1]);
            }

            activeScale = scaleIndex;
        }

        /// <summary>
        /// Tap a key.
        /// </summary>
        /// <param name="key"> The key to be tapped.</param>
        public static void KeyTap(Keys key)
		{
			Keyboard.KeyDown(key);
			Keyboard.KeyUp(key);
		}

        /// <summary>
        /// Hold key for certain amount of time and release. (UNTESTED)
        /// </summary>
        /// <param name="key"> The key to be held.</param>
        /// <param name="time"> The amount of time the key should be held for.</param>
        public static void KeyHold(Keys key, TimeSpan time)
		{
			Keyboard.KeyDown(key);
            new Timer(state=>Keyboard.KeyUp(key), null, time, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Bring a window to the front and activate it.
        /// </summary>
        /// <param name="windowName"> The name of the window we're looking for.</param>
        public static void BringWindowToFront(string windowName)
        {
            IntPtr wdwIntPtr = FindWindow(windowName, null);
			BringWindowToFront(wdwIntPtr);
        }

        /// <summary>
        /// Bring a window to the front and activate it.
        /// </summary>
        /// <param name="wdwIntPtr"> The pointer to the window we're looking for.</param>
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

        public static void OnSongPlay()
        {
            warframeWindow = FindWindow("Warframe");
            BringWindowToFront(warframeWindow);
        }
    }
}
