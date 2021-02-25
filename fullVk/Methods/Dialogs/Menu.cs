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

			VkApi api = MainData.Profiles.GetUser(profileNum).GetApi();

			while (true)
			{
				var menuList = new List<string>()
				{
					"Изображения"/*, "Голосовые сообщения"*/
				};
				int pos = gMenu.Menu(menuList.ToList(), HeaderName);

				switch (pos)
				{
					case 1:
						Get.GetImages(api, HeaderName);
						break;
					case 2:
					//	Get.GetVoiceMessages(api, HeaderName);
						break;
					case -1:
						return;
				}
			}


		}

	}
}
