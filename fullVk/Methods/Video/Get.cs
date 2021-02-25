using fullvk.Menu;
using System;
using System.Collections.Generic;
using fullvk.Methods.All;
using VkNet;
using VkNet.Model.RequestParams;
using VkNet.Enums.SafetyEnums;

namespace fullvk.Methods.Video
{
	class Get
	{
		public static void GetList(long? id, VkApi api, string resource = "")
		{
			TextConsole.PrintConsole.Header($"Видеозаписи {resource}");
			TextConsole.PrintConsole.Print("Получение списка видеозаписей", TextConsole.MenuType.InfoHeader);

			var videos = api.Video.Get(new VideoGetParams()
			{
				OwnerId = id
			});

			ChoiseMedia.Media[] list = new ChoiseMedia.Media[0];

			for (int i = 0; i < videos.Count; i++)
			{
				if (videos[i].Files.External != null)
					continue;
				Array.Resize(ref list, list.Length + 1);

				list[list.Length - 1] = new ChoiseMedia.Media()
				{
					url = GetMaxQuality(videos[i].Files),
					name = $"{videos[i].Title}",
					duration = GlobalFunctions.CurrentDuration((Int32)videos[i].Duration)
				};
				if (list[list.Length - 1].url == null)
					Array.Resize(ref list, list.Length - 1);
			}

			Prepare(list, resource);

		}

		/// <summary>
		/// Получить ссылку на максимальное качество
		/// </summary>
		/// <param name="files">Класс ссылок в видеофайле</param>
		/// <returns></returns>
		public static string GetMaxQuality(VkNet.Model.VideoFiles files)
		{
			if (files == null)
				return null;

			if (files.Mp4_1080 != null)
				return files.Mp4_1080.AbsoluteUri;
			else if (files.Mp4_720 != null)
				return files.Mp4_720.AbsoluteUri;
			else if (files.Mp4_480 != null)
				return files.Mp4_480.AbsoluteUri;
			else if (files.Mp4_360 != null)
				return files.Mp4_360.AbsoluteUri;
			else if (files.Mp4_240 != null)
				return files.Mp4_240.AbsoluteUri;
			else
				return null;
		}

		static void Prepare(ChoiseMedia.Media[] media, string resource = "")
		{
			ChoiseMedia.Media[] selected = new ChoiseMedia.Media[0];
			while (true)
			{
				var menuList = new List<string>() {"Все", "Выбрать"};
				int pos = gMenu.Menu(menuList, $"Видеозаписи {resource}");

				ChoiseMedia.Media[] copy = new ChoiseMedia.Media[media.Length];

				for (int i = 0; i < media.Length; i++)
				{
					copy[i] = new ChoiseMedia.Media()
					{
						url = media[i].url,
						name = media[i].name,
						duration = media[i].duration
					};
				}

				switch (pos)
				{
					case 1:
						Download.DownloadStart(copy, MediaType.Video);
						break;
					case 2:
						var videolist = ChoiseMedia.PrintList(copy);
						Console.ResetColor();
						if (videolist == null)
							return;
						selected = new ChoiseMedia.Media[videolist.Count];
						for (int i = 0; i < videolist.Count; i++)
							selected[i] = copy[i].Clone() as ChoiseMedia.Media;

						Download.DownloadStart(selected, MediaType.Video);

						break;
					case -1:
						return;
				}
			}
		}

		/// <summary>
		/// Получить видео из сообщений
		/// </summary>
		/// <param name="api"></param>
		/// <param name="HeaderName"></param>
		public static ChoiseMedia.Media[] FromMessage(VkApi api, string HeaderName)
		{
			long userID = GetConversationID.Get(api, HeaderName);
			if (userID < 0)
				return null;

			TextConsole.PrintConsole.Header(HeaderName);
			TextConsole.PrintConsole.Print("Получаем список видеозаписей...",
				TextConsole.MenuType.InfoHeader);

			string nextFrom = "";

			var Attachments = Dialogs.Get.GetDialogsAttachments(api, HeaderName, MediaType.Video);

			ChoiseMedia.Media[] AttachmentsList = new ChoiseMedia.Media[0];

			for (int i = 0; i < Attachments.Count; i++)
			{
				Array.Resize(ref AttachmentsList, AttachmentsList.Length + 1);
				AttachmentsList[AttachmentsList.Length - 1] = new ChoiseMedia.Media()
				{
					url = Get.GetMaxQuality((Attachments[i].Attachment.Instance as VkNet.Model.Attachments.Video).Files),
					duration = GlobalFunctions.CurrentDuration((int)(Attachments[i].Attachment.Instance as VkNet.Model.Attachments.Video).Duration),
					name = (Attachments[i].Attachment.Instance as VkNet.Model.Attachments.Video).Title
				};

				if (AttachmentsList[AttachmentsList.Length - 1].url == null)
					Array.Resize(ref AttachmentsList, AttachmentsList.Length - 1);

			}

			var list = ChoiseMedia.PrintList(AttachmentsList);

			if (list == null)
				return null;

			ChoiseMedia.Media[] to_Download = new ChoiseMedia.Media[0];
			for (int i = 0; i < list.Count; i++)
			{
				Array.Resize(ref to_Download, to_Download.Length + 1);
				to_Download[to_Download.Length - 1] = AttachmentsList[list[i]];
			}

			return to_Download;
		}
	}
}
