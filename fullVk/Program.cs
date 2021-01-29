using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace fullvk
{
	class Program
	{
		#region Position

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy,
			int wFlags);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		#endregion

		[STAThread]
		static void Main()
		{
			Console.Title = Application.ProductName;
			CenterScreen();

			SystemClasses.LoadSave.ReadData();

			Menu.MainMenu.ViewMenu();

			//Methods.Dialogs.Menu.View();
		}

		static void CenterScreen()
		{
			Size resolution = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;

			Console.Title = Application.ProductName;
			var hWnd = GetConsoleWindow();
			var wndRect = new RECT();

			GetWindowRect(hWnd, out wndRect);
			var cWidth = wndRect.Right - wndRect.Left;
			var cHeight = wndRect.Bottom - wndRect.Top;

			var SWP_NOSIZE = 0x1;

			var HWND_TOPMOST = 0;

			SetWindowPos(hWnd, HWND_TOPMOST, resolution.Width / 2 - cWidth / 2, resolution.Height / 2 - cHeight / 2, 0,
				0, SWP_NOSIZE);
		}

	}
}