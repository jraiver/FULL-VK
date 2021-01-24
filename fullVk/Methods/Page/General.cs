using System;
using fullvk.Menu;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace fullvk.Methods.Page
{
	class General
	{
		public static string ChoisePath(string header)
		{
			Start:
			int ChoisePath = gMenu.Menu(new List<string>() { "Восстановить", "Выбрать файл бэкапа" }, header);
			string customPath = null;

			switch (ChoisePath)
			{
				case 1:
					return customPath;
				case 2:
					OpenFileDialog ofd = new OpenFileDialog();
					if (ofd.ShowDialog() == DialogResult.OK)
					{
						customPath = ofd.FileName;
						return customPath;
					}
					else
						goto Start;
				case -1:
					return "";
			}

			return null;
		}

		/// <summary>
		/// Отмена
		/// </summary>
		/// <param name="task">Задача</param>
		/// <param name="cts">Токен</param>
		public static void Cancel(CancellationTokenSource cts)
		{
			while (true)
			{
				switch (Console.ReadKey().Key)
				{
					case ConsoleKey.Spacebar:
						cts.Cancel();
						return;
				}
			}
		}

	}
}
