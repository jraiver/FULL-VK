using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fullvk.Menu;
using VkNet;

namespace fullvk.Methods.Dialogs
{
	class Menu
	{
		public static void View()
		{
			int profileNum = MainData.ChoiseProfile();
			if (profileNum == -1)
				return;



			string HeaderName = "Диалоги";

			VkApi api = MainData.Profiles.GetUser(profileNum).API;

			while (true)
			{
				var menuList = new List<string>()
				{
					"Скачать изображения"
				};
				int pos = gMenu.Menu(menuList.ToList(), HeaderName);

				switch (pos)
				{
					case 1:
						GetImages(api, HeaderName);
						break;
					case -1:
						return;
				}
			}


		}

		static void GetImages(VkApi api, string HeaderName)
		{
			Get.GetDialogsList(api, HeaderName);
		}
	}
}
