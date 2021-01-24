using fullvk.Methods;
using System.Collections.Generic;
using VkNet.Model.RequestParams;

namespace fullvk.Menu
{
	class MainMenu
	{
		public static void ViewMenu()
		{
			while (true)
			{
				var menuList = new List<string>() { "Действия с профилем", "Узнать город", "Музыка", "Видеозаписи", "Диалоги", "Посты от пользователя"};
				int pos = gMenu.Menu(menuList, $"FULL VK");

				switch (pos)
				{
					case 1:
						MenuProfiles.ViewMenu();
						break;

					case 2:
						WhereIsFrom.Start();
						break;
					case 3:
						Methods.Music.MusicMenu.Menu();
						break;
					case 4:
						Methods.Video.Menu.View();
						break;
					case 5:
						Methods.Dialogs.Menu.View();
						break;
					case 6:
						FindPostsFunc.Search();
						break;

						
					case -1:
						return;
				}
			}
		}
	}
}
