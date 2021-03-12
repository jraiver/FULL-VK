using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using fullvk.Methods.FindPosts;
using fullvk.Methods.Page;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using static fullvk.MainData;
using static fullvk.TextConsole;

namespace fullvk.Methods
{
	class FindPostsFunc
	{
		static CancellationTokenSource cts = null;
		private static Task STOP = null;

		public static void Search()
		{
			int profileNum = ChoiseProfile();
			if (profileNum == -1)
				return;

			string HeaderName = "Поиск постов пользователя";
			VkApi api = Profiles.GetUser(profileNum).GetApi();

			long? id;

			long? userId;

			while (true)
			{
				PrintConsole.Header(HeaderName);

				PrintConsole.Print("Введите ссылку на пользователя: ", MenuType.Input);
				userId = GlobalFunctions.GetID(Console.ReadLine(), api.Token);

				if (userId != null && userId > 0)
					break;
			}

			while (true)
			{
				PrintConsole.Header(HeaderName);

				PrintConsole.Print("Введите ссылку где искать: ", MenuType.Input);
				id = GlobalFunctions.GetID(Console.ReadLine(), api.Token);

				if (id != null)
					break;
			}

			Start(api, id, userId);

		}


		static void Start(VkApi api, long? id, long? userId)
		{
			string HeaderName = $"Поиск постов в [{GlobalFunctions.WhoIs(api, id)}]";
			string Whois = GlobalFunctions.WhoIs(api, userId, NameCase.Gen);
			string FoundPosts = ResFindPost.addcss;
			ulong found = 0;

			try
			{
				PrintConsole.Header(HeaderName);

				var Walls = api.Wall.Get(new WallGetParams
				{
					OwnerId = id
				});

				ulong offset = 0;

				cts = new CancellationTokenSource();
				STOP = Task.Run(() => General.Cancel(cts), cts.Token);

				for (; ; )
				{
					cts.Token.ThrowIfCancellationRequested();
					PrintConsole.Header(HeaderName, "Для отмены нажмите[SPACE]");

					PrintConsole.Print(
						$"Поиск постов от {Whois}\nПроверено {offset} из {Walls.TotalCount}\nНайдено {found} записей.",
						MenuType.InfoHeader);

					Walls = api.Wall.Get(new WallGetParams
					{
						OwnerId = id,
						Count = 100,
						Offset = offset
					});

					if (Walls.WallPosts.Count == 0)
						break;

					for (int h = 0; h < Walls.WallPosts.Count; h++)
					{
						cts.Token.ThrowIfCancellationRequested();
						if (Walls.WallPosts[h].SignerId == userId || Walls.WallPosts[h].FromId == userId)
						{
							var data = GetMoreInfoFromPost(Walls.WallPosts[h], userId, api);
							//FoundPosts += $"https://vk.com/wall{id}_{Walls.WallPosts[h].Id}\n";

							data.postUrl = $"https://vk.com/wall{id}_{Walls.WallPosts[h].Id}";

							data.text = Walls.WallPosts[h].Text;
							data.date = Walls.WallPosts[h].Date.ToString();
							FoundPosts += GetPostHtml(data);
							found++;
							BackLine.Clear();
							PrintConsole.Print($"Найдено {found} записей.");
						}
					}

					offset += 100;

				}

				HeaderName = $"Результат поиска постов {Whois}";
				PrintConsole.Header(HeaderName);

				PrintConsole.Print($"Найдено {found} постов.", MenuType.InfoHeader);
				if (found > 0)
					SaveResult(api, userId, id, FoundPosts);

				BackLine.Continue();
			}

			catch (Exception ex)
			{

				if (cts.Token.CanBeCanceled)
				{
					HeaderName = $"Результат поиска постов {Whois}";
					PrintConsole.Header(HeaderName);
					PrintConsole.Print("Выполнена остановка поиска.", MenuType.Warning);
					PrintConsole.Print($"Найдено {found} постов.", MenuType.InfoHeader);
					if (found > 0)
						SaveResult(api, userId, id, FoundPosts);
					BackLine.Continue();
				}
			}
		}

		static void SaveResult(VkApi api, long? userId, long? id, string FoundPosts)
		{
			string InsidePath =
				$"[{DateTime.Today.Date.Day}.{DateTime.Today.Date.Month}.{DateTime.Today.Date.Year}] {GlobalFunctions.WhoIs(api, userId)} в {GlobalFunctions.WhoIs(api, id)}";

			char[] blockChar = { '\\', '/', ':', '*', '?', '<', '>', '|', '"' };

			for (int i = 0; i < blockChar.Length; i++)
			{
				InsidePath = InsidePath.Replace(blockChar[i], ' ');
			}

			string path = $"{Environment.CurrentDirectory}\\Search Result\\";
			Directory.CreateDirectory(path);
			if (!File.Exists(path + "Style.css"))
			{
				using (StreamWriter sw = File.CreateText(path + "Style.css"))
				{
					sw.WriteLine(ResFindPost.css);
				}
			}
			path += $"{InsidePath}.html";



			using (StreamWriter sw = File.CreateText(path))
			{
				sw.WriteLine(FoundPosts);
			}

			Process.Start(path);
		}

		static WallPostData GetMoreInfoFromPost(Post wall, long? userId, VkApi api)
		{
			var user = api.Users.Get(new List<long>() { (long)userId }, ProfileFields.Photo100);
			WallPostData data = new WallPostData()
			{
				author = user[0].FirstName + " " + user[0].LastName,
				ava_url = user[0].Photo100.AbsoluteUri
			};

			foreach (var attach in wall.Attachments)
			{
				if (attach.Instance.ToString().IndexOf("audio") == 0)
					data.tracksCount++;
				else if (attach.Instance.ToString().IndexOf("video") == 0)
					data.videoCount++;
				else if (attach.Instance.ToString().IndexOf("photo") == 0)
					data.imgCount++;
			}

			return data;
		}

		static string GetPostHtml(WallPostData data)
		{
			string html = ResFindPost.html;

			html = html.Replace("{post_url}", data.postUrl);
			html = html.Replace("{Avatar}", data.ava_url);
			html = html.Replace("{Author}", data.author);
			html = html.Replace("{DatePost}", data.date);
			html = html.Replace("{TittlePost}", data.text);
			html = html.Replace("{imgCount}", data.imgCount.ToString());
			html = html.Replace("{videoCount}", data.videoCount.ToString());
			html = html.Replace("{tracksCount}", data.tracksCount.ToString());

			return html;
		}
	}

	class WallPostData
	{
		public string postUrl { get; set; }
		public string author { get; set; }
		public string date { get; set; }
		public string ava_url { get; set; }
		public long? imgCount { get; set; } = 0;
		public long? videoCount { get; set; } = 0;
		public long? tracksCount { get; set; } = 0;
		public string text { get; set; }
	}
}


