using System;
using System.Collections.Generic;
using System.Linq;

namespace fullvk.Menu
{

	class gMenu
	{

		public static int Menu(List<string> list, string header)
		{
			if (list.Contains("Действия с профилем"))
				list.Add("Выход");
			else
				list.Add("Назад");
			var backup = list.ToList();

			ConsoleColor selected = ConsoleColor.Magenta;
			ConsoleColor selectedHigh = ConsoleColor.DarkMagenta;

			TextConsole.PrintConsole.Header(header);
			int control = 3;

			for (int i = 0; i < list.Count; i++)
			{
				if (i == 0)
					Console.BackgroundColor = selected;

				list[i] = $"{TextConsole.Tab()}[{i + 1}] - {list.ElementAt(i)}";

				Console.WriteLine(list.ElementAt(i));

				Console.ResetColor();
			}
			Console.SetCursorPosition(0, 0);

			int position = control;
			int lastPosition = control;

			Console.SetCursorPosition(0, position);
			Console.CursorVisible = false;

			while (true)
			{
				if (position != lastPosition)
				{
					Console.SetCursorPosition(0, lastPosition);
					Console.BackgroundColor = ConsoleColor.Black;
					Console.WriteLine(list[lastPosition - control]);
					Console.SetCursorPosition(0, position);

					Console.SetCursorPosition(0, position);
					Console.BackgroundColor = selectedHigh;
					Console.WriteLine(list[position - control]);
					Console.SetCursorPosition(0, position);

					Console.SetCursorPosition(0, position);
				}

				switch (Console.ReadKey().Key)
				{
					case ConsoleKey.UpArrow:
						lastPosition = position;
						if (position > control)
							position--;
						else
							position = list.Count - 1 + control;

						break;
					case ConsoleKey.DownArrow:
						lastPosition = position;
						if (position != list.Count - 1 + control)
							position++;
						else
							position = control;
						break;

					case ConsoleKey.Enter:
						Console.CursorVisible = true;
						Console.ResetColor();
						if (backup.ElementAt(position - control).IndexOf($"Назад") == 0 || backup.ElementAt(position - control).IndexOf($"Выход") == 0)
							return -1;
						return position - control + 1;

					case ConsoleKey.D0:
						Console.CursorVisible = true;
						Console.ResetColor();
						return -1;
					case ConsoleKey.NumPad0:
						goto case (ConsoleKey.D0);
					case ConsoleKey.Backspace:
						goto case (ConsoleKey.D0);
				}

			}


		}

		/// <summary>
		/// Получить корректное название строки меню
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static string GetCurrentName(string name)
		{
			int q = name.IndexOf("] - ") + 1;
			return name.Substring(q, name.Length - q);
		}
	}
}
