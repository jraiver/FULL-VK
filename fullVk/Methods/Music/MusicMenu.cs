using fullvk.Menu;
using fullvk.Methods.All;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Flurl.Util;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
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


			VkApi api = Profiles.GetUser(profileNum).GetApi();
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
								var trackListFromPlaylists = Get.GetPlaylists(api);
								var trackList = Get.GetList(new AnyData.Data() { api = api });
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
						var response = Get.GetCategoriesInRecommended(api).response;

						//Удаляем ненужные категории
						var temp = response.items;
						for (int i = 0; i < response.items.Length; i++)
						{
							if (response.items[i].source.IndexOf("recoms_communities") != 0 && response.items[i].source.IndexOf("recoms_friends") != 0)
							{
								Array.Resize(ref temp, temp.Length + 1);
								temp[temp.Length - 1] = response.items[i];
							}
						}

						response.items = temp;

						while (true)
						{
							var recCat = new List<string>()
							{
								$"Категории [{response.items.Length}]", $"Группы [{response.groups.Length}]", $"Пользователи [{response.profiles.Length}]"
							};
							int recCatPos = gMenu.Menu(recCat, "Рекомендации");

							switch (recCatPos)
							{
								case 1:
									var menuRec = new List<string>() { };
									for (int i = 0; i < response.items.Length; i++)
										menuRec.Add($"{response.items[i].title} [{response.items[i].count}]");
									while (true)
									{
										int cPos = gMenu.Menu(menuRec.ToList(), "Категории");

										switch (cPos)
										{
											default:
												var result = Get.GetTrackListFromRec(api, response.items[cPos - 1]);

												if (result != null && result.Length > 0)
												{
													Prepare(new AnyData.Data()
													{
														api = api,
														audios = result,
														SubName = response.items[cPos - 1].title
													});
												}

												break;
											case -1:
												return;
										}
									}
								case 2:
									var menuGroups = new List<string>() { };
									for (int i = 0; i < response.groups.Length; i++)
										menuGroups.Add($"{response.groups[i].name}");

									while (true)
									{
										int cPos = gMenu.Menu(menuGroups.ToList(), "Группы");
										switch (cPos)
										{
											default:
												Prepare(new AnyData.Data()
												{
													api = api,
													audios = Get.GetAudio(api, response.groups[cPos - 1].id * -1),
													SubName = response.groups[cPos - 1].name,
													id = response.groups[cPos - 1].id
												});

												break;
											case -1:
												return;
										}
									}
								case 3:
									var menuUsers = new List<string>() { };
									for (int i = 0; i < response.profiles.Length; i++)
										menuUsers.Add($"{response.profiles[i].first_name} {response.profiles[i].last_name}");

									while (true)
									{
										int cPos = gMenu.Menu(menuUsers.ToList(), "Пользователи");
										switch (cPos)
										{
											default:
												Prepare(new AnyData.Data()
												{
													api = api,
													audios = Get.GetAudio(api, response.profiles[cPos - 1].id),
													SubName = $"{response.profiles[cPos - 1].first_name} {response.profiles[cPos - 1].last_name}" ,
													id = response.profiles[cPos - 1].id
												});

												break;
											case -1:
												return;
										}
									}
								case -1:
									return;
							}
						}
					case 3:
					GetFromUrl:
						PrintConsole.Header(HeaderName);
						PrintConsole.Print("[0] - Назад", MenuType.Back);
						PrintConsole.Print($"Введите ссылку:  ", MenuType.Input);

						string id = Console.ReadLine();

						if (string.Compare(id, "0") == 0)
							return;

						long? _id = GlobalFunctions.GetID(id, Profiles.GetUser(profileNum).GetToken());

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
								mediaList = media,
								api = api,
								SubName = $"Из беседы {(string)media[0].other}",
								type = Get.Type.Recommendation
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
				type = Get.Type.Recommendation
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
										SubName = $"Со стены {gMenu.GetCurrentName(menuList[pos - 1])}",
										type = Get.Type.Recommendation
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

				var menuList = new List<string>() { "Все", "Выбрать" };
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

							SubMenu(new AnyData.Data() { audios = trackList, api = data.api, type = data.type, mType = data.mType, SubName = data.SubName, mediaList = mList });
						}
						else
						{
							ChoiseMedia.Media[] mList = new ChoiseMedia.Media[audioList.Count];

							for (int q = 0; q < mList.Length; q++)
							{
								mList[q] = (data.mediaList as ChoiseMedia.Media[])[audioList[q]];
							}

							SubMenu(new AnyData.Data() { audios = trackList, api = data.api, type = data.type, SubName = data.SubName, mType = data.mType, mediaList = mList });
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
				var menuList = new List<string>() { "Скачать", "Плейлист", "Список" };
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
						Prepare(new AnyData.Data() { audios = Get.GetPlaylist(list[pos - 1], api), api = api, type = Get.Type.Recommendation, SubName = list[pos - 1].title });

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

