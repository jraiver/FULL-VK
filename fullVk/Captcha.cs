using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace fullvk
{
	class CaptchaProcessing
	{

		/// <summary>
		/// Ввод капчи
		/// </summary>
		/// <param name="CaptchaImageURL">Ссылка на картинку</param>
		public static string EnterCaptcha(string CaptchaImageURL)
		{
			Captcha.Print(CaptchaImageURL);

			TextConsole.PrintConsole.Print($"{TextConsole.Tab()}", TextConsole.MenuType.Back);
			TextConsole.PrintConsole.Print("Введите код с картинки: ", TextConsole.MenuType.Input);

			string capchaAnser = Console.ReadLine();

			if (capchaAnser.Length < 1)
				return null;

			return capchaAnser;
		}

		private class Captcha
		{
			public static void Print(string url)
			{
				TextConsole.PrintConsole.Header("Введите captcha", "Для пропуска введите пустую строку.");

				Point location = new Point(0, Console.CursorTop + 2);
				Size imageSize = new Size(30, 10); // desired image size in characters

				// draw some placeholders
				Console.SetCursorPosition(location.X + 3, location.Y);
				Console.SetCursorPosition(location.X + imageSize.Width, location.Y);
				Console.SetCursorPosition(location.X, location.Y + imageSize.Height - 1);
				Console.SetCursorPosition(location.X + imageSize.Width, location.Y + imageSize.Height - 1);
				Console.WriteLine();

				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures), url);

				var request = WebRequest.Create(url);
				var response = request.GetResponse();
				Bitmap loadedBitmap = null;
				using (var responseStream = response.GetResponseStream())
				{
					loadedBitmap = new Bitmap(responseStream);
				}

				using (Graphics g = Graphics.FromHwnd(GetConsoleWindow()))
				{
					using (Image image = (Image)loadedBitmap)
					{
						Size fontSize = GetConsoleFontSize();

						// translating the character positions to pixels
						Rectangle imageRect = new Rectangle(
							location.X * fontSize.Width,
							location.Y * fontSize.Height,
							imageSize.Width * fontSize.Width,
							imageSize.Height * fontSize.Height);
						g.DrawImage(image, imageRect);
					}
				}
			}

			private static Size GetConsoleFontSize()
			{
				// getting the console out buffer handle
				IntPtr outHandle = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE,
					FILE_SHARE_READ | FILE_SHARE_WRITE,
					IntPtr.Zero,
					OPEN_EXISTING,
					0,
					IntPtr.Zero);
				int errorCode = Marshal.GetLastWin32Error();
				if (outHandle.ToInt32() == INVALID_HANDLE_VALUE)
				{
					throw new IOException("Unable to open CONOUT$", errorCode);
				}

				ConsoleFontInfo cfi = new ConsoleFontInfo();
				if (!GetCurrentConsoleFont(outHandle, false, cfi))
				{
					throw new InvalidOperationException("Unable to get font information.");
				}

				return new Size(cfi.dwFontSize.X, cfi.dwFontSize.Y);
			}

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern IntPtr GetConsoleWindow();

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern IntPtr CreateFile(
				string lpFileName,
				int dwDesiredAccess,
				int dwShareMode,
				IntPtr lpSecurityAttributes,
				int dwCreationDisposition,
				int dwFlagsAndAttributes,
				IntPtr hTemplateFile);

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern bool GetCurrentConsoleFont(
				IntPtr hConsoleOutput,
				bool bMaximumWindow,
				[Out][MarshalAs(UnmanagedType.LPStruct)]ConsoleFontInfo lpConsoleCurrentFont);

			[StructLayout(LayoutKind.Sequential)]
			internal class ConsoleFontInfo
			{
				internal int nFont;
				internal Coord dwFontSize;
			}

			[StructLayout(LayoutKind.Explicit)]
			internal struct Coord
			{
				[FieldOffset(0)]
				internal short X;

				[FieldOffset(2)]
				internal short Y;
			}

			private const int GENERIC_READ = unchecked((int)0x80000000);
			private const int GENERIC_WRITE = 0x40000000;
			private const int FILE_SHARE_READ = 1;
			private const int FILE_SHARE_WRITE = 2;
			private const int INVALID_HANDLE_VALUE = -1;
			private const int OPEN_EXISTING = 3;
		}
	}
}
