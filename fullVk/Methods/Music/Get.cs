using fullvk.Methods.All;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Management;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace fullvk.Methods.Music
{
	class Get
	{
		public enum Type
		{
			Profile,
			Group,
			Popular,
			Recommendation
		}

		public class Track : PopularMusic.Audio
		{
			public string name { get; set; }
			public bool? HQ { get; set; }
			public string duration { get; set; }

		}

		/// <summary>
		/// Отмена
		/// </summary>
		/// <param name="task"></param>
		/// <param name="cts"></param>
		static ChoiseMedia.Media[] Cancel(Task<ChoiseMedia.Media[]> task, CancellationTokenSource cts)
		{
			task.Start();
			Console.ReadKey();
			cts.Cancel();
			task.Wait();
			return task.Result;
		}

		/// <summary>
		/// Музыка со стены
		/// </summary>
		/// <param name="api"></param>
		public static ChoiseMedia.Media[] GetMusicFromBoard(VkApi api, long? pageId = null)
		{
			string HeaderName = "Музыка со стены";

			//long? pageId = 279952765;
			//pageId = -11884643;

			if (pageId == null)
			{
				while (true)
				{
					TextConsole.PrintConsole.Header(HeaderName);
					TextConsole.PrintConsole.Print("Введите ссылку на страницу: ", TextConsole.MenuType.Input);

					pageId = GlobalFunctions.GetID(Console.ReadLine(), api.Token);

					if (pageId != null && pageId > 0)
					{
						MainData.LastChoise.Add(new KeyValuePair<long?, string>(pageId, GlobalFunctions.WhoIs(api, pageId)));
						break;
					}
				}

			}

			CancellationTokenSource cts = new CancellationTokenSource();
			Task<ChoiseMedia.Media[]> fromboard = null;
			fromboard = new Task<ChoiseMedia.Media[]>(() => Get.FromBoard(api, cts.Token, pageId), cts.Token);


			return Cancel(fromboard, cts);

		}

		/// <summary>
		/// Музыка со стены
		/// </summary>
		/// <param name="api"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="pageId"></param>
		/// <returns></returns>
		static ChoiseMedia.Media[] FromBoard(VkApi api, CancellationToken cancellationToken, long? pageId)
		{
			ChoiseMedia.Media[] audioList = new ChoiseMedia.Media[0];
			try
			{
				string HeaderName = "Музыка со стены ";
				HeaderName += GlobalFunctions.WhoIs(api, pageId, NameCase.Gen);

				TextConsole.PrintConsole.Header(HeaderName);

				var Walls = api.Wall.Get(new WallGetParams
				{
					OwnerId = pageId,
					Count = 1
				});

				if (Walls.TotalCount == 0)
				{
					TextConsole.PrintConsole.Print("Записей не найдено.", TextConsole.MenuType.InfoHeader);
					TextConsole.BackLine.Continue();
					return null;
				}

				ulong offset = 0;

				for (; ; )
				{
					cancellationToken.ThrowIfCancellationRequested();
					TextConsole.PrintConsole.Header(HeaderName);
					TextConsole.PrintConsole.Print("Для остановки нажмите любую кнопку", TextConsole.MenuType.Warning);
					if (offset > Walls.TotalCount)
						offset = Walls.TotalCount;

					TextConsole.PrintConsole.Print(
						$"Проверено {offset} из {Walls.TotalCount}\nНайдено {audioList.Length} треков.",
						TextConsole.MenuType.InfoHeader);

					Walls = api.Wall.Get(new WallGetParams
					{
						OwnerId = pageId,
						Count = 100,
						Offset = offset
					});

					if (Walls.WallPosts.Count == 0)
					{
						TextConsole.PrintConsole.Header(HeaderName);
						TextConsole.PrintConsole.Print($"Найдено {audioList.Length} треков.", TextConsole.MenuType.InfoHeader);
						TextConsole.PrintConsole.Print($"Для продолжения нажмите любую кнопку.");
						break;
					}

					for (int h = 0; h < Walls.WallPosts.Count; h++)
					{
						cancellationToken.ThrowIfCancellationRequested();
						if (Walls.WallPosts[h].Attachments.Count > 0)
						{
							for (int w = 0; w < Walls.WallPosts[h].Attachments.Count; w++)
							{
								if (String.Compare(Walls.WallPosts[h].Attachments[w].Type.Name, "Audio") == 0)
								{
									Array.Resize(ref audioList, audioList.Length + 1);
									audioList[audioList.Length - 1] = new ChoiseMedia.Media()
									{
										name =
											$"{(Walls.WallPosts[h].Attachments[w].Instance as Audio).Artist} - {(Walls.WallPosts[h].Attachments[w].Instance as Audio).Title}",
										url = GetCurrentUrl((Walls.WallPosts[h].Attachments[w].Instance as Audio).Url),
										duration = GlobalFunctions.CurrentDuration((Walls.WallPosts[h].Attachments[w].Instance as Audio).Duration)
									};
								}
							}
						}
					}

					offset += 100;

				}

				return audioList;
			}
			catch (Exception ex)
			{
				if (cancellationToken.CanBeCanceled)
				{
					return audioList;
				}

				return null;
			}
		}

		public static Track[] GetList(AnyData.Data data)
		{
			TextConsole.PrintConsole.Header("Музыка", "Получаем список треков...");

			switch (data.type)
			{
				case Type.Profile:
					var audios = GetAudio(data.id, data.api);
					TextConsole.PrintConsole.Print($"Получено {audios.Count} треков.\n",
						TextConsole.MenuType.InfoHeader);

					if (audios == null)
						return null;

					return ToList(audios);

				case Type.Popular:
					var popular = ToList(data.api.Audio.GetPopular());
					TextConsole.PrintConsole.Print($"Получено {popular.Length} треков.\n",
						TextConsole.MenuType.InfoHeader);

					if (popular == null)
						return null;
					return popular;
				case Type.Group:
					var audiosFromGroup = GetAudio(data.id, data.api);
					TextConsole.PrintConsole.Print($"Получено {audiosFromGroup.Count} треков.\n",
						TextConsole.MenuType.InfoHeader);

					if (audiosFromGroup == null)
						return null;

					return ToList(audiosFromGroup);

				case Type.Recommendation:
					var rec = ToList(data.api.Audio.GetRecommendations(null, data.id));
					TextConsole.PrintConsole.Print($"Получено {rec.Length} треков.\n", TextConsole.MenuType.InfoHeader);

					if (rec == null)
						return null;
					return rec;
			}

			return null;
		}
		
		/// <summary>
		/// Получить категории
		/// </summary>
		/// <param name="api"></param>
		public static PopularMusic.Rootobject GetCategoriesInRecommended(VkApi api)
		{
			var parameters = new VkParameters();
			parameters.Add("v", "5.103");
			parameters.Add("lang", "ru");
			parameters.Add("extended", "1");
			parameters.Add("access_token", api.Token);
			parameters.Add("count", "100");
			parameters.Add("fields", "first_name_gen, photo_100");
			parameters.Add("name_case", "gen");

			var resp = api.Invoke("audio.getCatalog", parameters);

			return JsonConvert.DeserializeObject<PopularMusic.Rootobject>(resp);
		}

		/// <summary>
		/// Получить список трэков из выбранного блока рекомендаций 
		/// </summary>
		/// <param name="api"></param>
		/// <param name="item">Блок</param>
		/// <returns>Список треков</returns>
		public static Track[] GetTrackListFromRec(VkApi api, PopularMusic.Item item)
		{
			var json = GetById(api, item.id);

			var data = JsonConvert.DeserializeObject<PopularMusic.Rootobject>(json);

			if (data.response.block.playlists != null)
				MusicMenu.PlayListMenu(data.response.block.playlists, data.response.block.title, api);
			else
				return ToList(data.response.block.audios);

			return null;
		}

		
		/// <summary>
		/// Получить список аудиозаписей по ID каталога
		/// </summary>
		/// <param name="api"></param>
		/// <param name="id"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		static string GetById(VkApi api, string id, long count = 1000)
		{
			var parameters = new VkParameters();
			parameters.Add("count", count);
			parameters.Add("extended", 1);
			parameters.Add("block_id", id);
			parameters.Add("https", 1);
			parameters.Add("start_from", "");
			parameters.Add("lang", "ru");
			parameters.Add("access_token", api.Token);
			parameters.Add("v", "5.87");

			return api.Invoke("audio.getCatalogBlockById", parameters);
		}

		/// <summary>
		/// Преобразовать список
		/// </summary>
		/// <param name="audios"></param>
		/// <returns></returns>
		static Track[] ToList(IEnumerable<Audio> audios)
		{
			Track[] trackList = new Track[0];

			foreach (var track in audios)
			{
				Array.Resize(ref trackList, trackList.Length + 1);

				trackList[trackList.Length - 1] = new Track()
				{
					name = track.Title,
					artist = track.Artist,
					url = GetCurrentUrl(track.Url),
					duration = GlobalFunctions.CurrentDuration(track.Duration),
					HQ = track.IsHq
				};
			}

			return trackList;
		}

		/// <summary>
		/// Преобразовать список
		/// </summary>
		/// <param name="audios"></param>
		/// <returns></returns>
		static Track[] ToList(VkCollection<Audio> audios)
		{
			Track[] trackList = new Track[0];

			for (int i = 0; i < (int)audios.Count; i++)
			{
				Array.Resize(ref trackList, trackList.Length + 1);

				trackList[trackList.Length - 1] = new Track()
				{
					name = audios[i].Title,
					artist = audios[i].Artist,
					url = GetCurrentUrl(audios[i].Url),
					duration = GlobalFunctions.CurrentDuration(audios[(int)i].Duration),
					HQ = audios[i].IsHq,
					id = audios[i].Id,
					owner_id = audios[i].OwnerId
				};
			}

			return trackList;
		}
		/// <summary>
		/// Преобразовать список
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		static Track[] ToList(PopularMusic.Audio[] list)
		{
			Track[] trackList = new Track[0];

			for (int i = 0; i < list.Length; i++)
			{
				if (list[i].url == null || list[i].url.Length < 1)
					continue;

				Array.Resize(ref trackList, trackList.Length + 1);

				trackList[trackList.Length - 1] = new Track()
				{
					name = list[i].title,
					artist = list[i].artist,
					url = GetCurrentUrl(new Uri(list[i].url)),
					duration = GlobalFunctions.CurrentDuration(list[i].duration),
					HQ = false
				};
			}

			return trackList;
		}

		/// <summary>
		/// Преобразовать список из плейлиста 
		/// </summary>
		/// <param name="pl"></param>
		/// <param name="api"></param>
		/// <returns></returns>
		public static Track[] GetPlaylist(PopularMusic.Playlist pl, VkApi api)
		{
			var data = api.Audio.GetPlaylistById(pl.owner_id.Value, pl.id.Value);
			var audios = api.Audio.Get(new AudioGetParams()
			{
				OwnerId = data.OwnerId,
				PlaylistId = data.Id
			});

			return ToList(audios);
		}

		/// <summary>
		/// Поулчить треки из плейлиста
		/// </summary>
		/// <param name="pl"></param>
		/// <param name="api"></param>
		/// <returns></returns>
		public static Track[] GetPlaylist(AudioPlaylist pl, VkApi api)
		{
			TextConsole.PrintConsole.Header($"Получение треков из плейлиста: {pl.Title}");
			try
			{
				var audios = api.Audio.Get(new AudioGetParams()
				{
					OwnerId = pl.Original.OwnerId,
					PlaylistId = pl.Original.PlaylistId,
					AccessKey = pl.AccessKey
				});

				return ToList(audios);
			}
			catch (Exception ex)
			{
				if (ex.GetType().FullName.IndexOf("VkNet.Exception.CannotBlacklistYourselfException") > -1)
				{
					TextConsole.PrintConsole.Print($"Не удаётся получить доступ к плейлисту: [{pl.Title}]", TextConsole.MenuType.Warning);
				}

				return null;
			}

		}

		/// <summary>
		/// Получтиь преобразованный список аудиозаписей 
		/// </summary>
		/// <param name="api"></param>
		/// <param name="id"></param>
		/// <returns>Track[]</returns>
		public static Track[] GetAudio(VkApi api, long? id)
		{
			return ToList(GetAudio(id, api));
		}

		/// <summary>
		/// Получить список аудиозаписей
		/// </summary>
		/// <param name="id"></param>
		/// <param name="vkApi"></param>
		/// <param name="count">Количество</param>
		/// <returns></returns>
		public static VkCollection<Audio> GetAudio(long? id, VkApi vkApi, int count = 1000)
		{
			try
			{
				if (id == null)
					id = vkApi.UserId;

				int offset = 0;

				VkNet.Model.Attachments.Audio[] trackList = new VkNet.Model.Attachments.Audio[0];

				while (true)
				{
					var next = vkApi.Audio.Get(new AudioGetParams
					{
						OwnerId = id,
						Count = count,
						Offset = offset
					});
					if (count == 0)
						return next;
					if (next.Count == 0)
						break;
					Array.Resize(ref trackList, trackList.Length + next.Count);
					next.CopyTo(trackList, trackList.Length - next.Count);
					offset += count;
				}

				return new VkCollection<Audio>(ulong.Parse(trackList.Length.ToString()), new VkCollection<Audio>(ulong.Parse(trackList.Length.ToString()), trackList));
			}
			catch (VkNet.Exception.CannotBlacklistYourselfException ex)
			{
				Console.Clear();
				TextConsole.PrintConsole.Print(ex.Message, TextConsole.MenuType.Error);
				return null;
			}

			catch (VkNet.Exception.AudioAccessDeniedException ex)
			{
				Console.Clear();
				TextConsole.PrintConsole.Print(ex.Message, TextConsole.MenuType.Error);
				return null;
			}
			catch (Exception ex)
			{
				Console.Clear();
				TextConsole.PrintConsole.Print("SYSTEM ERROR", TextConsole.MenuType.Error);
				return null;
			}


		}

		/// <summary>
		/// Получить ссылку для скачивания
		/// </summary>
		/// <param name="Url"></param>
		/// <returns></returns>
		static string GetCurrentUrl(Uri Url)
		{
			if (Url == null)
				return null;

			if (Url.LocalPath.IndexOf(".mp3") > -1)
				return Url.ToString();

			var segments = Url.Segments.ToList();

			segments.RemoveAt((segments.Count - 1) / 2);
			segments.RemoveAt(segments.Count - 1);

			segments[segments.Count - 1] = segments[segments.Count - 1].Replace("/", ".mp3");

			return new Uri($"{Url.Scheme}://{Url.Host}{string.Join("", segments)}{Url.Query}").ToString();
		}

		/// <summary>
		/// Получить из сообщений
		/// </summary>
		/// <param name="api"></param>
		/// <param name="HeaderName"></param>
		public static ChoiseMedia.Media[] FromMessage(VkApi api, string HeaderName)
		{
			long ConvID = GetConversationID.Get(api, HeaderName);
			if (ConvID < 0)
				return null;

			//var q = api.Messages.GetConversationsById(new List<long>() { ConvID }, new List<string>() { }, true);

			TextConsole.PrintConsole.Header(HeaderName);
			TextConsole.PrintConsole.Print("Получаем список аудиозаписей...",
				TextConsole.MenuType.InfoHeader);

			string nextFrom = "";

			var Attachments = Dialogs.Get.GetDialogsAttachments(api, HeaderName, MediaType.Audio);
			ChoiseMedia.Media[] AttachmentsList = new ChoiseMedia.Media[0];

			for (int i = 0; i < Attachments.Count; i++)
			{
				Array.Resize(ref AttachmentsList, AttachmentsList.Length + 1);
				AttachmentsList[AttachmentsList.Length - 1] = new ChoiseMedia.Media()
				{
					url = GetCurrentUrl((Attachments[i].Attachment.Instance as Audio).Url),
					duration = GlobalFunctions.CurrentDuration(
						(int)(Attachments[i].Attachment.Instance as Audio).Duration),
					name = $"{(Attachments[i].Attachment.Instance as Audio).Artist} - {(Attachments[i].Attachment.Instance as Audio).Title}"
				};

				if (AttachmentsList[AttachmentsList.Length - 1].url == null)
					Array.Resize(ref AttachmentsList, AttachmentsList.Length - 1);

			}

			if (AttachmentsList.Length > 0)
			{
				var q = api.Messages.GetConversationsById(new List<long>() { ConvID }, new List<string>() { "first_name_ins", "last_name_ins" }, true);

				if (q.Items.First().ChatSettings != null)
					AttachmentsList[0].other = q.Items.First().ChatSettings.Title;
				else
				{
					AttachmentsList[0].other = "c " + q.Profiles.First().FirstNameIns + " " +
											   q.Profiles.First().LastNameIns;
				}
			}



			return AttachmentsList;
		}

		/// <summary>
		/// Выбор плейлистов
		/// </summary>
		/// <param name="api"></param>
		/// <param name="id"></param>
		public static Get.Track[] GetPlaylists(VkApi api, long? id = null)
		{
			if (id == null)
				id = api.UserId;

			var playlists = api.Audio.GetPlaylists((long)id);

			ChoiseMedia.Media[] listPL = new ChoiseMedia.Media[playlists.Count];

			for (int i = 0; i < playlists.Count; i++)
			{
				listPL[i] = new ChoiseMedia.Media()
				{
					name = playlists[i].Title,
					duration = "PLAYLIST",
					url = "null"
				};
			}

			var choised = ChoiseMedia.PrintList(listPL, "Выбрать плейлисты");

			Get.Track[] result = new Get.Track[0];

			foreach (var index in choised)
			{
				var trackList = Get.GetPlaylist(playlists[index], api);
				if (trackList == null)
					continue;

				for (int i = 0; i < trackList.Length; i++)
				{
					if (trackList[i].url == null)
						continue;
					Array.Resize(ref result, result.Length + 1);
					result[result.Length - 1] = new Get.Track()
					{
						url = GetCurrentUrl(new Uri(trackList[i].url)),
						name = trackList[i].artist + " - " + trackList[i].name,
						duration = trackList[i].duration
					};
				}
			}


			return result;
		}
	}
}
