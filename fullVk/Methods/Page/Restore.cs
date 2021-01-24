using fullvk.Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using static fullvk.TextConsole;

namespace fullvk.Methods.Page
{
	class Restore
	{

		static CancellationTokenSource cts = null;
		static Task STOP = null;


		/// <summary>
		/// Восстановить из бэкапа
		/// </summary>
		/// <param name="user"></param>
		public static void Menu(User user)
		{
			while (true)
			{
				var menuList = new List<string>() { "Группы", "Музыка" };
				int pos = gMenu.Menu(menuList, "Восстановить из бэкапа");

				switch (pos)
				{
					case 1:
						Groups.Restore(user, cts);

						break;
					case 2:
						Music.Restore(user, cts);
						break;
					case -1:
						return;
				}
			}
		}

		/// <summary>
		/// Музыка
		/// </summary>
		class Music
		{
			/// <summary>
			/// Восстановить музыку
			/// </summary>
			/// <param name="user"></param>
			/// <returns></returns>
			public static bool Restore(User user, CancellationTokenSource cancellationToken)
			{
				if (!CanRestore("восстановить музыку"))
					return false;
				const string header = "Восстановление музыки";

				PrintConsole.Header(header);

				string customPath = General.ChoisePath(header);
				if (customPath != null && customPath.Length == 0)
					return false;

				var data = JsonConvert.DeserializeObject<Backup.Music.Track[]>(Read(user.id, "Music", customPath));
				int error = 0;

				cancellationToken = new CancellationTokenSource();
				STOP = Task.Run(() => General.Cancel(cancellationToken), cancellationToken.Token);

				int totalRestore = 0;
				try
				{
					for (totalRestore = 0; totalRestore < data.Length; totalRestore++)
					{
						cancellationToken.Token.ThrowIfCancellationRequested();
						PrintConsole.Header(header, "Для остановки нажмите [SPACE]\n");
						PrintConsole.Print($"Восстановлено {totalRestore} из {data.Length}");
						PrintConsole.Print($"Не удалось восстановить {error}");

						try
						{
							user.API.Audio.Add(data[totalRestore].id.Value, data[totalRestore].owner_id.Value);
						}
						catch (Exception ex)
						{
							error++;
						}
					}

					return true;
				}
				catch (Exception ex)
				{
					STOP.Dispose();
					if (cts.Token.CanBeCanceled)
					{
						PrintConsole.Header(header, $"Восстановлено {totalRestore} треков\nНе удалось восстановить {error}");
						BackLine.Continue();
						return true;
					}
					else
					{
						PrintConsole.Header(header);
						PrintConsole.Print(ex.Message, MenuType.Warning);
						BackLine.Continue();
						return false;
					}
				}
			}

		}

		class Groups
		{

			/// <summary>
			/// Восстановить список групп
			/// </summary>
			/// <param name="user"></param>
			/// <returns></returns>

			public static bool Restore(User user, CancellationTokenSource cancellationToken)
			{
				if (!CanRestore("восстановить список сообществ"))
					return false;

				const string header = "Восстановление сообществ";

				PrintConsole.Header(header);


				//string userid = ChoiseBackup(MainData.Profiles.GetUser(0).API);


				string customPath = General.ChoisePath(header);
				if (customPath != null && customPath.Length == 0)
					return false;


				Backup.Groups.Data[] groups = JsonConvert.DeserializeObject<Backup.Groups.Data[]>(Read(user.id, "Groups", customPath));

				cancellationToken = new CancellationTokenSource();
				STOP = Task.Run(() => General.Cancel(cancellationToken), cancellationToken.Token);

				try
				{
					for (int i = 0; i < groups.Length; i++)
					{
						cancellationToken.Token.ThrowIfCancellationRequested();
						PrintConsole.Header("Восстановление сообществ", "Для остановки нажмите [SPACE]\n");
						PrintConsole.Print($"Восстановлено {i} из {groups.Length} сообществ.", MenuType.InfoHeader);
						try
						{
							user.API.Groups.Join(groups[i].id);
						}
						catch (Exception ex)
						{
						}

						BackLine.Clear();

					}

					PrintConsole.Print($"Восстановлено {groups.Length} сообществ.");
					BackLine.Continue();

					return true;
				}
				catch (Exception ex)
				{
					STOP.Dispose();
					if (cts.Token.CanBeCanceled)
					{
						PrintConsole.Header(header, $"Восстановлено {groups.Length} сообществ.");
						BackLine.Continue();
						return true;
					}
					else
					{
						PrintConsole.Header(header);
						PrintConsole.Print(ex.Message, MenuType.Warning);
						BackLine.Continue();
						return false;
					}

				}

			}
		}

		/// <summary>
		/// Чтение бэкапа
		/// </summary>
		/// <param name="id"></param>
		/// <param name="p">тип</param>
		/// <returns></returns>
		static string Read(string id, string p, string path = null)
		{
			if (path == null)
				path = $"{Environment.CurrentDirectory}\\Backup\\{id}\\{p}.back";

			if (!File.Exists(path))
				return null;

			string backup = File.ReadAllText(path);

			return backup;
		}

		static bool CanRestore(string header)
		{
			while (true)
			{
				int pos = gMenu.Menu(new List<string>() { "Да" }, $"Вы действительно хотите {header}?");

				switch (pos)
				{
					case 1:
						return true;
					case -1:
						return false;
				}
			}
		}
	}
}
