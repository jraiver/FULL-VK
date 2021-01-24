using fullvk.Methods.Music;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using fullvk.Menu;
using fullvk.Methods.All;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using static fullvk.TextConsole;
using VkNet;

namespace fullvk.Methods.Page
{
	class Backup
	{

		public static long? ChoiseUser(VkApi api, string header)
		{
			while (true)
			{
				PrintConsole.Header(header);
				PrintConsole.Print(" [0] - назад", MenuType.InfoHeader);
				PrintConsole.Print("Введите ссылку на пользователя: ", MenuType.Input);
				var line = Console.ReadLine();
				if (string.Compare(line, "0") == 0)
					break;
				var userId = GlobalFunctions.GetID(line, api.Token);
				if (userId != null)
					return userId;
			}

			return null;

		}

		/// <summary>
		/// Сохранить данные со страницы
		/// </summary>
		/// <param name="user">Пользователь</param>
		/// 

		public static void Menu(User user)
		{
			while (true)
			{
				var menuList = new List<string>() { "Группы", "Музыка" };
				int pos = gMenu.Menu(menuList, "Создать бэкап страницы");

				switch (pos)
				{
					case 1:
						Groups.Backup(user);
						break;
					case 2:
						Music.Backup(user);
						break;
					case -1:
						return;
				}
			}
		}

		/// <summary>
		/// Группы
		/// </summary>
		public class Groups
		{
			public class Data
			{
				public string name { get; set; }
				public long id { get; set; }
			}

			/// <summary>
			/// Получить список групп
			/// </summary>
			/// <param name="user"></param>
			/// <returns>Список групп Group[]</returns>
			public static object[] GetGroups(User user)
			{
				long? userId;
				string header = "Сохранить список групп";
				while (true)
				{
					var menuList = new List<string>() { $"{user.first_name} {user.last_name}", "Выбрать пользователя" };
					int pos = gMenu.Menu(menuList, header);

					switch (pos)
					{
						case 1:
							userId = user.API.UserId;
							goto Start;
						case 2:
							userId = ChoiseUser(user.API, header);
							if (userId != null)
								goto Start;
							break;
						case -1:
							return null;
					}
				}

			Start:
				PrintConsole.Header(header);
				VkCollection<VkNet.Model.Group> groups = null;
				Data[] list = new Data[0];

				for (int q = 0; ;)
				{
					groups = user.API.Groups.Get(new GroupsGetParams()
					{
						UserId = userId,
						Extended = true,
						Count = 1000,
						Offset = q
					});

					if (groups.Count == 0)
						break;

					Array.Resize(ref list, list.Length + groups.Count);
					PrintConsole.Print($"Получено {list.Length}", MenuType.Refresh);

					for (int i = 0; i < groups.Count; i++)
						list[i] = new Data()
						{
							name = groups[i].Name,
							id = groups[i].Id
						};

					q += 1000;

				}

				return new object[] { list, userId };
			}

			/// <summary>
			/// Соранить список групп
			/// </summary>
			/// <param name="user">Пользователь</param>
			/// <returns>True - успешно</returns>
			/// 
			public static bool Backup(User user)
			{
				PrintConsole.Header("Сохранить список групп");
				var data = GetGroups(user);

				SaveData("Groups", data[0], data[1].ToString());

				return true;
			}
		}

		/// <summary>
		/// Музыка
		/// </summary>
		public class Music
		{
			public class Track
			{
				public long? id { get; set; }
				public long? owner_id { get; set; }
			}

			/// <summary>
			/// Сохранить список музыки
			/// </summary>
			/// <param name="user"></param

			public static bool Backup(User user)
			{
				PrintConsole.Header("Сохранить список аудиозаписей");
				var data = GetMusic(user);

				SaveData("Music", data[0], data[1].ToString());

				return true;
			}

			public static object[] GetMusic(User user, long? id = null)
			{
				long? userId;
				string header = "Сохранить список аудиозаписей";
				while (true)
				{
					var menuList = new List<string>() { $"{user.first_name} {user.last_name}", "Выбрать пользователя" };
					int pos = gMenu.Menu(menuList, header);

					switch (pos)
					{
						case 1:
							userId = user.API.UserId;
							goto Start;
						case 2:
							userId = ChoiseUser(user.API, header);
							if (userId != null)
								goto Start;
							break;
						case -1:
							return null;
					}
				}

			Start:
				PrintConsole.Header(header);
				PrintConsole.Header($"{header}\n");

				var list = Get.GetList(new AnyData.Data()
				{ api = user.API, id = userId, type = Get.GetType.Profile, audios = null });

				Track[] toWrite = new Track[list.Length];

				for (int i = 0; i < list.Length; i++)
				{
					toWrite[i] = new Track()
					{
						id = list[i].id,
						owner_id = list[i].owner_id
					};
					PrintConsole.Print($"Получено {i + 1} аудиозаписей", MenuType.Refresh);
				}

				return new object[] { toWrite, userId };
			}
		}

		static void SaveData(string name, object content, string userId)
		{
			var data = JsonConvert.SerializeObject(content, Formatting.None, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			});

			string path = $"{Environment.CurrentDirectory}\\Backup\\{userId}\\";

			if (!File.Exists(path))
			{
				Directory.CreateDirectory(path);
				using (File.Create(path + $"{name}.back"))
				{

				}

			}

			File.WriteAllText(path + $"{name}.back", data, Encoding.UTF8);
		}
	}
}
