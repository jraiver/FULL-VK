using System;
using System.Collections.Generic;
using static fullvk.TextConsole;

namespace fullvk.Methods.All
{
	class ChoiseMedia
	{
		public class Media : ICloneable
		{
			public string url { get; set; }
			public string name { get; set; }
			public string duration { get; set; }
			public bool selected { get; set; }
			public object other { get; set; }
			public string subName { get; set; }

			/// <summary>
			/// Клонировать
			/// </summary>
			/// <returns></returns>
			public object Clone()
			{
				return MemberwiseClone();
			}

		}

		/// <summary>
		/// Меню
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static List<int> PrintList(Media[] media, string header = "Выбрать файлы")
		{
			ConsoleColor selected = ConsoleColor.Magenta;
			ConsoleColor high = ConsoleColor.Green;
			ConsoleColor selectedHigh = ConsoleColor.DarkMagenta;

			int control = 5;

			int position = control;
			int lastPosition = control;

			Console.CursorVisible = false;

			SelectedAll:
			Console.ResetColor();

			PrintConsole.Header(header, 
				"[A] - Выделить всё; [SPACE] - Установить/Снять выделение; [ENTER] - Продолжить; [0] - Выход;\n");

			for (int i = 0; i < media.Length; i++)
			{
				if (i == 0)
				{
					Console.BackgroundColor = selected;
					Console.WriteLine(PrintObject(media[i]));
					continue;
				}

				if (media[i].url != null)
				{
					if (media[i].selected)
						Console.BackgroundColor = high;
					else
						Console.BackgroundColor = ConsoleColor.Black;

					Console.WriteLine(PrintObject(media[i]));
				}

				Console.ResetColor();
			}

			Console.SetCursorPosition(0, 0);
			Console.SetCursorPosition(0, position);
		

			while (true)
			{
				if (position != lastPosition)
				{
					if (!media[lastPosition - control].selected)
					{
						Console.SetCursorPosition(0, lastPosition);
						Console.ResetColor();
						Console.WriteLine(PrintObject(media[lastPosition - control]));
						Console.SetCursorPosition(0, position);
					}
					else
					{
						Console.SetCursorPosition(0, lastPosition);
						Console.BackgroundColor = high;
						Console.WriteLine(PrintObject(media[lastPosition - control]));
						Console.SetCursorPosition(0, position);
					}

					if (!media[position - control].selected)
					{
						Console.SetCursorPosition(0, position);
						Console.BackgroundColor = selected;
						Console.WriteLine(PrintObject(media[position - control]));
						Console.SetCursorPosition(0, position);
					}
					else
					{
						Console.SetCursorPosition(0, position);
						Console.BackgroundColor = selectedHigh;
						Console.WriteLine(PrintObject(media[position - control]));
						Console.SetCursorPosition(0, position);
					}

					Console.SetCursorPosition(0, position);
				}

				switch (Console.ReadKey().Key)
				{
					case ConsoleKey.UpArrow:
						lastPosition = position;
						if (position > control)
							position--;
						else
							position = media.Length - 1 + control;

						break;
					case ConsoleKey.DownArrow:
						lastPosition = position;
						if (position != media.Length - 1 + control)
							position++;
						else
							position = control;
						break;
					case ConsoleKey.Spacebar:
						if (media[position - control].selected)
						{
							Console.SetCursorPosition(0, position);
							Console.ResetColor();
							Console.WriteLine(PrintObject(media[position - control]));
							Console.SetCursorPosition(0, position);
							media[position - control].selected = false;
						}
						else
						{
							Console.SetCursorPosition(0, position);
							Console.BackgroundColor = high;
							Console.WriteLine(PrintObject(media[position - control]));
							Console.SetCursorPosition(0, position);
							media[position - control].selected = true;
						}

						break;
					case ConsoleKey.Enter:

						List<int> index = new List<int>();

						for (int i = 0; i < media.Length; i++)
						{
							if (media[i].selected)
								index.Add(i);
						}
						Console.ResetColor();
						Console.Clear();
						return index;

					case ConsoleKey.A:
						for (int i = 0; i < media.Length; i++)
						{
							media[i].selected = !media[i].selected;
						}
						goto SelectedAll;
						
					case ConsoleKey.D0:
						Console.ResetColor();
						return null;
					case ConsoleKey.NumPad0:
						goto case (ConsoleKey.D0);
				}

			}

		}

		static string PrintObject(Media media)
		{
			if (media.subName == null)
				media.subName = string.Empty;

			string name = $"{media.name} {media.subName}";

			if (name.Length < Console.WindowWidth / 4 * 3)
				return name + $"[{media.duration}]";
			else
				return $"{media.name.Substring(0, Console.WindowWidth / 4 * 3 - media.duration.Length - 3 - media.subName.Length)}... [{media.duration}] {media.subName}";
		}

	}
}
