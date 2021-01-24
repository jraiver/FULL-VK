using fullvk.Methods.All;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Text;
using Newtonsoft.Json;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using static fullvk.TextConsole;

namespace fullvk
{
	class GlobalFunctions
	{
		/// <summary>
		/// Корректная длительность
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public static string CurrentDuration(int duration)
		{
			if (duration == 0)
				return "Error parse duration";
			string current = $"{TimeSpan.FromSeconds(duration).Days}:{TimeSpan.FromSeconds(duration).Hours}:{TimeSpan.FromSeconds(duration).Minutes}:{TimeSpan.FromSeconds(duration).Seconds}";

			string[] words = current.Split(new char[] { ':' });
			int toParse = 0;
			current = string.Empty;

			for (int i = 0; i < words.Length; i++)
			{
				toParse = int.Parse(words[i]);
				if (toParse > 0 && toParse < 10)
				{
					current += $"0{words[i]}:";
				}
				else if (toParse > 9)
					current += $"{words[i]}:";
				else if (current != string.Empty)
				{
					current += "00:";
				}
			}

			current = current.Substring(0, current.Length - 1);
			if (current.Length == 2)
				current = $"00:{current}";

			return current;
		}

		/// <summary>
		/// Вернуться в меню
		/// </summary>
		public static bool getBack(string line)
		{
			if (String.Compare(line, "0") == 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Получить ID
		/// </summary>
		public static long? GetID(string uri, string vkToken)
		{
			if (string.Compare(uri, "0") == 0)
				return 0;

			string[] CheckLine = new string[]
			{
				"vk.com/id",
				"vk.com/group",
				"vk.com/public",
				"vk.com/club",
				"vk.com/"
			};

			int i = 0;
			string url = null;

			for (i = 0; i < CheckLine.Length; i++)
			{
				if (uri.IndexOf(CheckLine[i]) > -1)
				{
					url = uri.Substring(uri.IndexOf(CheckLine[i]) + CheckLine[i].Length,
						uri.Length - uri.IndexOf(CheckLine[i]) - CheckLine[i].Length);

					if (i != CheckLine.Length - 1)
					{
						try
						{
							long? _id = long.Parse(url);
							if (i != 0)
								return _id * -1;
							return _id;
						}
						catch (FormatException ex)
						{
							goto ReCheckAliance;
						}

						
					}

					ReCheckAliance:

					url = uri.Substring(
						uri.IndexOf(CheckLine[CheckLine.Length - 1]) + CheckLine[CheckLine.Length - 1].Length,
						uri.Length - uri.IndexOf(CheckLine[CheckLine.Length - 1]) -
						CheckLine[CheckLine.Length - 1].Length);

					return GlobalFunctions.GetResponceId(url, vkToken);
				}

			}

			if (url == null)
				return null;

			return null;
		}

		public static string WhoIs(VkApi api, long? id, NameCase nameCase = null)
		{
			if (nameCase == null)
				nameCase = NameCase.Nom;

			if (id == null)
				id = api.UserId;

			if (id > 0)
			{
				Int64.TryParse(id.ToString(), out long _id);

				var user = api.Users.Get(new List<long>() { _id }, nameCase: nameCase).FirstOrDefault();

				if (user != null)
					return user.FirstName + " " + user.LastName;
			}
			else if (id < 0)
			{
				id = id * -1;
				var group = api.Groups.GetById(null,id.ToString(), null).FirstOrDefault();
				return group.Name;
			}

			return "null";
		}

		static long? GetResponceId(string url, string token)
		{
			using (WebClient client = new WebClient() { Encoding = Encoding.UTF8 })
			{
				string result = client.DownloadString(
					$"https://api.vk.com/method/utils.resolveScreenName?screen_name={url}&access_token={token}&v=5.102");

				return Classes.GetIdFromAlias.GetFromResponce(result);
			}
		}

		/// <summary>
		/// Получение ID профиля после авторизации через VK.NET с токеном
		/// </summary>
		public static long? GetIdNet(VkNet.VkApi api)
		{
			WebClient client = new WebClient() { Encoding = Encoding.UTF8 };
			var response = client.DownloadString($"https://api.vk.com/method/users.get?access_token={api.Token}&v=5.101");

			var user = JsonConvert.DeserializeObject<VkNet.Model.User>(JObject.Parse(response)["response"][0].ToString());

			return user.Id;
		}

		/// <summary>
		/// Вывод сообщения "продолжить?"
		/// </summary>
		/// /// <param name="clear">Очистить консоль перед выводом (false)</param>
		public static bool ContinueQuestion(bool clear = false)
		{
			if (clear)
				Console.Clear();

			PrintConsole.Print("[1] - Продолжить");
			PrintConsole.Print("\n[0] - Назад", MenuType.Back);

			ConsoleKeyInfo cki;

			while (true)
			{
				cki = Console.ReadKey(true);
				if (cki.Key == ConsoleKey.D1 || cki.Key == ConsoleKey.NumPad1)
					return true;
				else if (cki.Key == ConsoleKey.D0 || cki.Key == ConsoleKey.NumPad0)
					return false;
			}
		}

	}
}
