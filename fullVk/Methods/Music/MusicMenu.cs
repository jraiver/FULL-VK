using fullvk.Menu;
using fullvk.Methods.All;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VkNet;
using VkNet.Enums.SafetyEnums;
using static fullvk.MainData;
using static fullvk.TextConsole;

namespace fullvk.Methods.Music
{
	class MusicMenu
	{
		const string HeaderName = "Музыка";

		/// <summary>
		/// Отмена
		/// </summary>
		/// <param name="task">Задача</param>
		/// <param name="cts">Токен</param>
		
		public static void Menu()
		{
			int profileNum = ChoiseProfile();
			if (profileNum == -1)
				return;
			

			VkApi api = Profiles.GetUser(profileNum).API;

			while (true)
			{
				var menuList = new List<string>() { "Моя музыка", "Рекомендации", "Указать ссылку", "Последние", "Из сообщений", "Со стены" };
				int pos = gMenu.Menu(menuList.ToList(), HeaderName);

				switch (pos)
				{
					case 1:
						var subMenuList = new List<string>() { "Музыка", "Плейлисты", "Всё (плейлисты вначале)" };
						int subPos = gMenu.Menu(subMenuList, HeaderName);

						switch (subPos)
						{
							case 1:
								
								Prepare(new AnyData.Data()
								{
									api = api,
									audios = Get.GetList(new AnyData.Data() { api = api }),
									SubName = GlobalFunctions.WhoIs(api, null, NameCase.Gen),
									id = api.UserId
								});
								break;
							case 2:
								Prepare(new AnyData.Data()
								{
									api = api,
									audios = Get.GetPlaylists(api),
									SubName = GlobalFunctions.WhoIs(api, null, NameCase.Gen),
									id = api.UserId
								});
								break;
							case 3:
								var trackListFromPlaylists =  Get.GetPlaylists(api);
								var trackList = Get.GetList(new AnyData.Data() {api = api});
								var fullList = trackListFromPlaylists.Concat(trackList).ToArray();
								Prepare(new AnyData.Data()
								{
									api = api,
									audios = fullList,
									SubName = GlobalFunctions.WhoIs(api, null, NameCase.Gen),
									id = api.UserId
								});
								break;
								
							case -1:
								break;
						}

						
						break;
					case 2:
						AnyData.Data data = RecMenu(api);
						PrintConsole.Header(data.SubName);
						Prepare(data);
						break;

					case 3:
						GetFromUrl:
						PrintConsole.Header(HeaderName);
						PrintConsole.Print("[0] - Назад", MenuType.Back);
						PrintConsole.Print($"Введите ссылку:  ", MenuType.Input);

						string id = Console.ReadLine();

						if (string.Compare(id, "0") == 0)
							return;

						long? _id = GlobalFunctions.GetID(id, Profiles.GetUser(profileNum).token);

						if (_id == null)
							goto GetFromUrl;

						AnyData.Data url = new AnyData.Data()
						{
							api = api,
							audios = Get.GetList(new AnyData.Data() { id = _id, api = api }),
							SubName = GlobalFunctions.WhoIs(api, _id, NameCase.Gen),
							id = _id
						};

						LastChoise.Add(new KeyValuePair<long?, string>(url.id, url.SubName));

						Prepare(url);

						break;
					case 4:
						Last_Choise(api);
						break;
					case 5:
						var media = Get.FromMessage(api, HeaderName);

						if (media != null)
							Prepare(new AnyData.Data()
							{
								mType = MediaType.Audio,
								mediaList = media, api = api, SubName = $"Из беседы {(string)media[0].other}", type = Get.GetType.Recommendation
							}, true);
				
						break;
					case 6:
						GetDataFromBoard(api);
						break;
					case -1:
						return;
					
				}
			}
		}


	

		public static void GetDataFromBoard(VkApi api)
		{
			var result = Get.GetMusicFromBoard(api);

			Prepare(new AnyData.Data()
			{
				mType = MediaType.Audio,
				mediaList = result,
				api = api,
				SubName = "Со стены",
				type = Get.GetType.Recommendation
			}, true);


		}

		/// <summary>
		/// Последние ссылки
		/// </summary>
		static void Last_Choise(VkApi api)
		{
			while (true)
			{
				PrintConsole.Header("Последние ссылки");
				string menu = "";

				if (LastChoise.Count() < 1)
				{
					PrintConsole.Print("Список пуст.", MenuType.InfoHeader);
					BackLine.Continue();
					return;
				}

				var menuList = new List<string>();

				for (int i = 0; i < LastChoise.Count(); i++)
					menuList.Add($"{LastChoise.Get(i).Value}");

				menuList.Add("Очистить");

				int pos = gMenu.Menu(menuList, "Последние ссылки");

				switch (pos)
				{
					default:
						if (pos > -1 && pos <= LastChoise.Count())
						{
							int choise = gMenu.Menu(new List<string>() { "Со страницы", "Со стены" }, $"Музыка {gMenu.GetCurrentName(menuList[pos - 1])}");

							switch (choise)
							{
								case 1:
									Prepare(new AnyData.Data()
									{
										api = api,
										audios = Get.GetList(new AnyData.Data() { id = LastChoise.Get(pos - 1).Key, api = api }),
										SubName = GlobalFunctions.WhoIs(api, LastChoise.Get(pos - 1).Key),
										id = LastChoise.Get(pos - 1).Key
									});
									break;
								case 2:
									var result = Get.GetMusicFromBoard(api, LastChoise.Get(pos - 1).Key);

									Prepare(new AnyData.Data()
									{
										mType = MediaType.Audio,
										mediaList = result,
										api = api,
										SubName = $"Со стены {gMenu.GetCurrentName(menuList[pos - 1])}" ,
										type = Get.GetType.Recommendation
									}, true);
									break;
							}

							
						}
						else if (pos == LastChoise.Count())
							LastChoise.Clear();
						break;
					case 0:
						return;
					case -1:
						return;

				}
			}
		}

		static void Prepare(AnyData.Data data, bool MediaListReady = false)
		{
			ChoiseMedia.Media[] list;

			if (MediaListReady)
			{
				list = (ChoiseMedia.Media[])data.mediaList;
				goto MediaListReady;
			}

			else
				list = new ChoiseMedia.Media[data.audios.Length];

			for (int i = 0; i < data.audios.Length; i++)
			{
				list[i] = new ChoiseMedia.Media()
				{
					url = data.audios[i].url,
					name = $"{data.audios[i].artist} - {data.audios[i].name} ",
					duration = data.audios[i].duration
				};
			}

			MediaListReady:
			Get.Track[] trackList = null;

			while (true)
			{
				if (data.mType == null)
					data.mType = MediaType.Audio;

				var menuList = new List<string>() { "Все", "Выбрать"};
				int pos = gMenu.Menu(menuList, $"Музыка {data.SubName}");

				switch (pos)
				{
					case 1:
						if (!MediaListReady)
							data.mediaList = list;

						SubMenu(data);
						break;
					case 2:
						Console.Clear();

						var audioList = ChoiseMedia.PrintList(list);

						if (audioList == null)
							break;
						if (!MediaListReady)
						{
							trackList = new Get.Track[audioList.Count];

							ChoiseMedia.Media[] mList = new ChoiseMedia.Media[0];

							data.mediaList = new ChoiseMedia.Media[0];

							for (int i = 0; i < audioList.Count; i++)
							{
								trackList[i] = data.audios[audioList[i]];
								Array.Resize(ref mList, mList.Length + 1);
								mList[mList.Length - 1] = new ChoiseMedia.Media()
								{
									duration = data.audios[audioList[i]].duration,
									name = $"{data.audios[audioList[i]].artist} - {data.audios[audioList[i]].name}",
									url = data.audios[audioList[i]].url
								};
							}

							SubMenu(new AnyData.Data() { audios = trackList, api = data.api, type = data.type, mType = data.mType,SubName = data.SubName, mediaList = mList });
						}
						else
						{
							ChoiseMedia.Media[] mList = new ChoiseMedia.Media[audioList.Count];

							for (int q = 0; q < mList.Length; q++)
							{
								mList[q] = (data.mediaList as ChoiseMedia.Media[])[audioList[q]];
							}

							SubMenu(new AnyData.Data() { audios = trackList, api = data.api, type = data.type,SubName = data.SubName, mType = data.mType, mediaList = mList });
						}
						
						break;
					case -1:
						return;
				}
			}

		}

		static void SubMenu(AnyData.Data data)
		{
			if (data.mediaList == null)
			{
				Console.Clear();
				return;
			}

			if ((data.mediaList as ChoiseMedia.Media[]).Length == 0)
			{
				PrintConsole.Header(HeaderName);
				return;
			}

			while (true)
			{
				var menuList = new List<string>() { "Скачать", "Плейлист", "Список"};
				int pos = gMenu.Menu(menuList, $"{HeaderName}");

				switch (pos)
				{
					case 1:
						Download.DownloadStart(data.mediaList, MediaType.Audio);
						break;
					case 2:
						PlaylistDwnld.Get(data);
						break;
					case 3:
						TrackListTxt(data);
						break;
						
					case -1:
						PrintConsole.Header(HeaderName);
						return;
				}
			}
		}

		static void TrackListTxt(AnyData.Data music)
		{
			string names = "";
			for (int i = 0; i < music.mediaList.Length; i++)
			{
				if (music.mediaList[i].name.IndexOf("Неизвестен - Без названия") == 0)
					continue;
				names += $"{music.mediaList[i].name}\n";
			}

			string path = $"{Environment.CurrentDirectory}\\music_{music.SubName}.txt";

			File.WriteAllText(path, names);
			var proc = new System.Diagnostics.Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = path,
					UseShellExecute = true
				}
			};
			proc.Start();
		}

		static AnyData.Data RecMenu(VkApi api)
		{
			var menuList = new List<string>() {
				"Специально для Вас",
				"Новинки",
				"Новые альбомы",
				"Чарт ВКонтакте",
				"Рэп & Хип-Хоп",
				"Поп",
				"Рок",
				"Недавно прослушанные",
				"Музыка под настроение",
				"Выбор редакции",
				"Музыкальные подборки",
				"На любой случай",
				"Новые имена",
				"Похоже на прослушанное",
				"Сообщества"
			};


			while (true)
			{
				int pos = gMenu.Menu(menuList.ToList(), $"Рекомендации");

				switch (pos)
				{
					case -1:
						return null;
					case 0:
						goto case -1;
					default:
						return new AnyData.Data()
						{
							audios = Get.GetPopular(api, menuList.ElementAt(pos - 1)),
							SubName = menuList.ElementAt(pos - 1),
							type = Get.GetType.Recommendation,
							api = api
						};
				}

			}
		}

		public static PopularMusic.Audio[] PlayListMenu(PopularMusic.Playlist[] list, string name, VkApi api)
		{
			var menuList = new List<string>();

			for (int i = 0; i < list.Length; i++)
			{
				if (list[i].main_artists != null)
					menuList.Add($"{list[i].main_artists[0].name} - {list[i].title}");
				else
					menuList.Add(list[i].title);
			}

			while (true)
			{
				int pos = gMenu.Menu(menuList, name);

				switch (pos)
				{
					case 0:
						return null;
					case -1:
						return null;
					default:
						Prepare(new AnyData.Data() { audios = Get.GetPlaylist(list[pos - 1], api), api = api, type = Get.GetType.Recommendation, SubName = list[pos - 1].title });

						break;
				}

			}
		}

		static int GetKey()
		{
			string input = Console.ReadLine();

			if (input.Length < 1)
				return -1;

			foreach (char ch in input)
			{
				if (!char.IsDigit(ch))
					return -1;
			}

			int key = int.Parse(input);

			return key;
		}

	}
}

