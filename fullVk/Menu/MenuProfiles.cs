using fullvk.Methods.Page;
using fullvk.SystemClasses;
using System.Collections.Generic;
using static fullvk.MainData;
namespace fullvk.Menu
{
	class MenuProfiles
	{
		public static void ViewMenu()
		{
			if (Profiles.Count() > 0)
			{

				PrintUsers:

				int numProfile = ChoiseProfile();

				switch (numProfile)
				{
					case -1:
						break;
					default:
						if (numProfile > -1 && numProfile < Profiles.Count())
							UserMenu(Profiles.GetUser(numProfile), numProfile);
						else
							goto PrintUsers;
						break;
				}


			}
			else
			{
				Auth.Login();
			}
		}

		static void UserMenu(User user, int id)
		{
			while (true)
			{
				var menuList = new List<string>() { "Очистить страницу", "Бэкап","Восстановить","Удалить из программы" };
				int pos = gMenu.Menu(menuList, user.first_name + " " + user.last_name);

				switch (pos)
				{
					case 1:
						Clear.Menu(user);
						break;
					case 2:
						Backup.Menu(user);
						break;
					case 3:
						Restore.Menu(user);
						break;
					case 4:
						user.Delete();
						return;
					case -1:
						return;

				}
			}
		}


	}

}
