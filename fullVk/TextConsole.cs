using System;

namespace fullvk
{
	class TextConsole
	{
		public const int SPACE = 3;

		public enum MenuType
		{
			Menu,
			Refresh,
			Header,
			InfoHeader,
			Input,
			Error,
			Warning,
			Back,
			Custom,
			Track,
			Default,
		};

		public class PrintConsole
		{
			public static void Print(string message)
			{
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.WriteLine(message);
				Console.ResetColor();
			}

			public static void Input()
			{
				Print("Выберите действие: ", MenuType.Input);
			}

			public static void Header(string message, string info = null)
			{
				Console.Clear();
				if (message.Length >= Console.WindowWidth)
				{
					message = message.Substring(0, Console.WindowWidth - 3) + "...";
				}
				Print(message, MenuType.Header);
				if (info != null)
					Print(info, MenuType.InfoHeader);
			}

			public static void Print(string message, MenuType type = MenuType.Default, ConsoleColor color = ConsoleColor.Gray)
			{
				switch (type)
				{
					case MenuType.Refresh:
						BackLine.Clear();
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine(message);
						Console.ForegroundColor = ConsoleColor.White;
						break;
					case MenuType.Header:
						Console.Clear();
						int newCount = 0;
						if (Console.WindowWidth - message.Length > 0)
						{
							newCount = Console.WindowWidth - message.Length / 2;
						}
						else
						{
							int le = Console.WindowWidth - message.Length * 1;
							le = message.Length - le - 9;
							message = message.Substring(0, le + 9) + "...";
							newCount = Console.WindowWidth - message.Length / 2;
						}

						string border = "";
						for (int i = 0; i < newCount; i++)
							border += "=";

						string tab = "      ";

						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine(border);
						Console.WriteLine(tab + message.ToUpper());
						Console.WriteLine(border);
						break;
					case MenuType.Input:
						Console.ForegroundColor = ConsoleColor.DarkMagenta;
						Console.WriteLine(" " + message);
						BackLine.Back(message.Length + 1);
						break;
					case MenuType.InfoHeader:
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine(message);
						break;
					case MenuType.Error:
						float countE = message.Length * 2;
						if (countE > Console.BufferWidth)
							countE = Console.BufferWidth - 1;
						string borderE = "";
						for (int i = 0; i < countE; i++)
							borderE += "=";
						string tabE = "";
						for (int i = 0; i < (countE - message.Length) / 2; i++)
							tabE += " ";
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine(borderE);
						Console.WriteLine(tabE + message.ToUpper());
						Console.WriteLine(borderE + "\n");
						Console.ReadKey();
						break;
					case MenuType.Warning:
						Console.ForegroundColor = ConsoleColor.DarkRed;
						Console.WriteLine("[!]" + message);
						break;
					case MenuType.Menu:
						Console.WriteLine($"{Tab()}{message.Replace("\n", "\n" +Tab())}");
						Print("[0] - Назад", MenuType.Back);
						break;
						
					case MenuType.Back:
						Console.ForegroundColor = ConsoleColor.DarkGray;
						Console.WriteLine($"\n{Tab()}{message.Replace("\n", "\n" +Tab())}\n");
						break;
					case MenuType.Custom:
						Console.ForegroundColor = color;
						Console.WriteLine(message);
						break;

					case MenuType.Track:
						if (message.Length > Console.WindowWidth / 4 * 3)
						{
							Console.WriteLine(message.Substring(0, Console.WindowWidth / 4 * 3) + "...");
						}
						else 
							Console.WriteLine(message);
						break;
						
					default:
							Console.ForegroundColor = ConsoleColor.Gray;
							Console.WriteLine(message);
						break;
				}

				Console.ResetColor();
			}

		}

		public class BackLine
		{
			public static void Clear()
			{
				Console.SetCursorPosition(0, Console.CursorTop - 1);
				Console.Write(new string(' ', Console.WindowWidth));
				Console.SetCursorPosition(0, Console.CursorTop - 1);
			}

			public static void Continue()
			{
				PrintConsole.Print("Для продолжения нажмите любую клавишу...", MenuType.Custom, ConsoleColor.DarkGray);
				Back(40);
				Console.ReadKey();
			}

			public static void Back(int count = 15)
			{
				Console.SetCursorPosition(count, Console.CursorTop - 1);
			}
		}

		public static string Tab(int space = SPACE)
		{
			string tab = "";
			if (space < 1)
				return "";

			for (int i = 0; i < space; i++)
				tab += " ";

			return tab;
		}
	}
}
