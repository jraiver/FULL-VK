using fullvk.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using fullvk.Methods.All;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace fullvk.Methods.Video
{
	class Menu
	{
		public static void View()
		{
			int profileNum = MainData.ChoiseProfile();
			if (profileNum == -1)
				return;

			string HeaderName = "Видеозаписи";

			VkApi api = MainData.Profiles.GetUser(profileNum).API;

			while (true)
			{
				var menuList = new List<string>()
				{
					"Мои видеозаписи", "Видеозаписи друзей", "Ввести ссылку пользователя/паблика",
					"Ввести ссылку на видео", "Из сообщений"
				};
				int pos = gMenu.Menu(menuList.ToList(), HeaderName);

				switch (pos)
				{
					case 1:
						Get.GetList(api.UserId, api);
						break;
					case 2:
						var friendsList = api.Friends.Get(new FriendsGetParams()
						{
							Fields = ProfileFields.FirstName
						});
						var friends = new List<string>();

						for (int i = 0; i < friendsList.Count; i++)
							friends.Add($"{friendsList[i].FirstName} {friendsList[i].LastName}");

						int selected = gMenu.Menu(friends.ToList(), "Выберите друга");
						if (selected < 0)
							return;
						Get.GetList((long?) friendsList[selected - 1].Id, api,
							friendsList[selected - 1].FirstName + " " + friendsList[selected - 1].LastName);
						break;
					case 3:
						TextConsole.PrintConsole.Header(HeaderName);
						TextConsole.PrintConsole.Print("Введите ссылку: ", TextConsole.MenuType.Input);

						string url = Console.ReadLine();

						long? id = GlobalFunctions.GetID(url, api.Token);
						Get.GetList(id, api);
						break;
					case 4:
						List<long?> video;

						while (true)
						{
							TextConsole.PrintConsole.Header(HeaderName);
							TextConsole.PrintConsole.Print("Введите ссылку: ", TextConsole.MenuType.Input);
							video = FromUrl(Console.ReadLine());
							if (video != null)
								break;
						}

						var data = api.Video.Get(new VideoGetParams()
						{
							Videos = new List<VkNet.Model.Attachments.Video>()
								{new VkNet.Model.Attachments.Video() {OwnerId = video[0], Id = video[1]}},
							OwnerId = video[0]
						});
						if (data.Count < 1)
						{
							TextConsole.PrintConsole.Print("Не удалось получить ссылку на видеозапись.",
								TextConsole.MenuType.InfoHeader);
							TextConsole.BackLine.Continue();
							break;
						}

						ChoiseMedia.Media[] toDownload = new ChoiseMedia.Media[1];

						toDownload[0] = new ChoiseMedia.Media()
						{
							url = Get.GetMaxQuality(data[0].Files),
							name = data[0].Title,
							duration = GlobalFunctions.CurrentDuration((int) data[0].Duration)
						};

						Download.DownloadStart(toDownload, MediaType.Video);

						break;
					case 5:
						var videos = Get.FromMessage(api, HeaderName);
						Download.DownloadStart(videos, MediaType.Video);
						break;

					case -1:
						return;

				}
			}
		}

		

		/// <summary>
		/// Получить id из ссылки
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		static List<long?> FromUrl(string url)
		{
			string txt = url;

			string re1 = "(video)"; 
			string re2 = "(\\d+)"; 
			string re3 = "(_)";
			string re4 = "(\\d+)"; 

			Regex r = new Regex(re1 + re2 + re3 + re4, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			Match m = r.Match(txt);

			if (m.Success)
			{
				txt = m.Value.Substring(m.Value.IndexOf("video") + 5, m.Value.Length - 5);

				string ownerid = txt.Substring(0, txt.IndexOf("_"));
				string videoid = txt.Substring(txt.IndexOf("_") + 1, txt.Length - txt.IndexOf("_") - 1);

				List<long?> data = new List<long?>()
				{
					long.Parse(ownerid),
					long.Parse(videoid)
				};

				return data;
			}
			else return null;
		}

		
	}
}
