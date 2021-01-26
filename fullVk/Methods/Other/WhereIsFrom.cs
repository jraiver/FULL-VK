using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Utils;
using static fullvk.TextConsole;

namespace fullvk.Methods
{
	internal class WhereIsFrom
	{

		static WebClient api = new WebClient();

		public static void Start()
		{
			int profile = MainData.ChoiseProfile();
			if (profile < 0)
				return;
			while (true)
			{
				Console.Clear();
				PrintConsole.Print("Откуда пользователь", MenuType.Header);
				PrintConsole.Print("[0] - Назад", MenuType.Back);
				PrintConsole.Print("Введите ссылку на пользователя: ", MenuType.Input);

				long? id = GlobalFunctions.GetID(Console.ReadLine(),MainData.Profiles.GetUser(profile).token);
				//long? id = GlobalFunctions.GetID("https://vk.com/d.sharchekev", Profiles.UsersList[profile].token.AccT_VK);

				if (id == null)
					continue;
				
				if (id == 0)
					return;

				GetFriends(id, MainData.Profiles.GetUser(profile).API);

				Console.ReadKey();
			}
		}

		static void GetFriends(long? id, VkApi api)
		{
			Console.Clear();
			PrintConsole.Print("Откуда пользователь", MenuType.Header);
			PrintConsole.Print("\nПолучение информации...", MenuType.InfoHeader);
			Sort(
				api.Friends.Get(new VkNet.Model.RequestParams.FriendsGetParams
				{
					UserId = id,
					Fields = ProfileFields.City
				}));
		}

		static void Sort(VkCollection<VkNet.Model.User> users)
		{
			string[] citys = new string[0];

			var p = users.Select(x => x.City);

			foreach (var item in p)
			{
				Array.Resize(ref citys, citys.Length + 1);

				if (item == null)
					citys[citys.Length - 1] = "Не указан";
				else 
					citys[citys.Length - 1] = item.Title;
			}

			Dictionary<string, int> Output = new Dictionary<string, int>();

			CompareCity(citys);
		}

		static void CompareCity(string[] citys)
		{
			Dictionary<string, int> Output = new Dictionary<string, int>();

			for (int i = 0; i < citys.Length; i++)
			{
				if (citys[i] == string.Empty)
					continue;

				for (int q = 0; q < citys.Length; q++)
				{
					if (q != i)
					{
						if (string.Compare(citys[i], citys[q]) == 0)
						{
							if (Output.ContainsKey(citys[i]))
								Output[citys[i]] += 1;
							else
								Output.Add(citys[i], 1);

							citys[q] = string.Empty;
						}
					}
				}

				citys[i] = string.Empty;
			}

			Output = Output.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
			View(Output);
		}

		static void View(Dictionary<string, int> data)
		{
			int count = 0;
			int total = 0;

			try
			{
				Console.Clear();
				PrintConsole.Print("Результат работы", MenuType.Header);
				
				foreach (var city in data)
				{
					if (count < 11)
					{
						if (string.Compare(city.Key, "Не указан") < 0)
						{
							count++;
							PrintConsole.Print($"  [{city.Value}] {city.Key}", MenuType.Custom);
						}
					}

					total += city.Value;
				}

				PrintConsole.Print($"  [{data["Не указан"]}] Не указан", MenuType.Custom);
			}
			catch (KeyNotFoundException ex)
			{
				PrintConsole.Print("     [0] Не указан");
			}
			finally
			{
				if (count < 11)
				{
					
				}
			}
		}

	}
}