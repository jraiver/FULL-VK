using System;
using VkNet;
using VkNet.Model.RequestParams;
using static fullvk.TextConsole;

namespace fullvk.Methods
{
	class Relation
	{ 
		public static void Start()
		{
			int profileNum = MainData.ChoiseProfile();
			if (profileNum == -1)
				return;

			VkApi api = MainData.Profiles.GetUser(profileNum).GetApi();

			string HeaderName = "Поиск общих друзей между пользователями";
			PrintConsole.Header(HeaderName);
			long? FirstId ;
			long? LastId ;

			while (true)
			{
				PrintConsole.Header(HeaderName);
				PrintConsole.Print("Введите ссылку на 1 пользователя: ", MenuType.Input);
				string url = Console.ReadLine();

				FirstId = GlobalFunctions.GetID(url, api.Token);
				if (FirstId != null)
				{
					BackLine.Clear();
					PrintConsole.Print($"{GlobalFunctions.WhoIs(api, FirstId)}\n");
					break;
				}
			}

			while (true)
			{

				PrintConsole.Print("Введите ссылку на 2 пользователя: ", MenuType.Input);
				string url = Console.ReadLine();

				LastId = GlobalFunctions.GetID(url, api.Token);
				if (LastId != null)
				{
					break;
				}
				BackLine.Clear();
			}

			Search(FirstId, LastId, api);


		}

		static void Search(long? fID, long? lID, VkApi api)
		{
			string HeaderName = $"Поиск общих друзей между {GlobalFunctions.WhoIs(api, fID)} и  {GlobalFunctions.WhoIs(api, lID)}";
			PrintConsole.Header(HeaderName);
			PrintConsole.Print("Получение списка пользователей...", MenuType.InfoHeader);

			var list = api.Friends.GetMutual(new FriendsGetMutualParams()
			{
				SourceUid = fID,
				TargetUid = lID
			});

			PrintConsole.Print($"Найдено {list[0].CommonCount} общих друзей.\n", MenuType.InfoHeader);

			for (int i = 0; i < list[0].CommonFriends.Count; i++)
			{
				PrintConsole.Print($" [{i + 1}] {GlobalFunctions.WhoIs(api, (long?)list[0].CommonFriends[i])} [https://vk.com/{list[0].CommonFriends[i]}]");
			}

			Console.WriteLine();
			BackLine.Continue();
		}

	}
}

