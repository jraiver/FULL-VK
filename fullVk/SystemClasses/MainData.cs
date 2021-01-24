using fullvk.Menu;
using fullvk.SystemClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet;

namespace fullvk
{
	class MainData
	{
		static Dictionary<long?, string> LChoise { get; set; } = new Dictionary<long?, string>();

		/// <summary>
		/// Последние ссылки на музыку
		/// </summary>
		public class LastChoise
		{
			/// <summary>
			/// Получить ссылку
			/// </summary>
			/// <returns></returns>
			public static KeyValuePair<long?, string> Get(int id) => LChoise.ElementAt(id);

			/// <summary>
			/// Получить все ссылки 
			/// </summary>
			/// <returns></returns>
			public static Dictionary<long?, string> GetAll() => LChoise;

			/// <summary>
			/// Количество
			/// </summary>
			/// <returns></returns>
			public static int Count() => LChoise.Count;

			/// <summary>
			/// Добавить
			/// </summary>
			/// <returns></returns>
			public static void Add(KeyValuePair<long?, string> url, bool load = false)
			{

				if (!LChoise.ContainsKey(url.Key))
					LChoise.Add(url.Key, url.Value);
				
				else
				{
					LChoise.Remove(url.Key);
					LChoise.Add(url.Key, url.Value);

					if (LChoise.Count > 1)
					{
						Dictionary<long?, string> urls = new Dictionary<long?, string>()
						{
							{ LChoise.ElementAt(LChoise.Count - 1).Key, LChoise.ElementAt(LChoise.Count - 1).Value }
						};

						for (int i = 0; i < LChoise.Count - 1; i++)
							urls.Add(LChoise.ElementAt(i).Key, LChoise.ElementAt(i).Value);

						LChoise = urls;
					}
				}

				if (!load)
					LoadSave.WriteData();
			}

			/// <summary>
			/// Удалить все ссылки
			/// </summary>
			public static void Clear()
			{
				LChoise = new Dictionary<long?, string>();
				LoadSave.WriteData();
			}
		}

		public static class Profiles
		{
			static User[] UsersList { get; set; } = new User[0];

			/// <summary>
			/// Получить пользователя
			/// </summary>
			public static User GetUser(int i)
			{
				if (i == -1)
					return null;
				else if (i < 0 && i > UsersList.Length - 1)
					return null;
				return UsersList[i];
			}

			/// <summary>
			/// Получить всех пользователей
			/// </summary>
			public static User[] GetAllUser() => UsersList;

			/// <summary>
			/// Перезаписать всех пользователей
			/// </summary>
			public static void RewriteUsers(User[] usrs, bool load = false)
			{
				UsersList = usrs;

				if (!load)
					LoadSave.WriteUsers();
			}

			/// <summary>
			/// Количество авторизированных пользоавтелей
			/// </summary>
			/// <returns></returns>
			public static int Count() => UsersList.Length;

			/// <summary>
			/// Добавить пользователя
			/// </summary>
			public static bool AddUser(User user)
			{
				User[] list = UsersList;

				if (list != null)
					Array.Resize(ref list, UsersList.Length + 1);
				else
					list = new User[1];

				list[list.Length - 1] = user;
				UsersList = list;
				return true;

			}

		}

		/// <summary>
		/// Выбор профиля для дальнейшей работы
		/// </summary>
		public static int ChoiseProfile()
		{
			Start:
			if ( Profiles.Count() == 0)
			{
				Auth.Login();
				return 0;
			}
			else
			{
				var menuList = new List<string>() ;

				for (int i = 0; i < Profiles.Count(); i++)
				{
					menuList.Add(
						$"{Profiles.GetUser(i).first_name} {Profiles.GetUser(i).last_name} [{Profiles.GetUser(i).id}]");
				}

				menuList.Add("Добавить профиль");
			
				int pos = gMenu.Menu(menuList, "Для дальнейшей работы выберите профиль");

				if (pos > 0)
					pos--;

				if (pos == Profiles.Count())
				{
					if (Auth.Login())
						goto Start;
					else
						return -1;
				}
				else
					return pos;

			}
		}
	}

	/// <summary>
	/// Пользователь
	/// </summary>
	public class User : ICloneable
	{
		/// <summary>
		/// Клонировать
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return MemberwiseClone();
		}

		int GetId()
		{
			User[] list = MainData.Profiles.GetAllUser();

			for (int i = 0; i < MainData.Profiles.Count(); i++)
			{
				if (string.Compare(id, MainData.Profiles.GetUser(i).id) == 0)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Удалить пользователя
		/// </summary>

		public void Delete()
		{
			User[] list = MainData.Profiles.GetAllUser();
			int ProfileId = GetId();

			for (int i = ProfileId; i < list.Length; i++)
			{
				if (i + 1 == list.Length)
					break;

				list[i] = list[i + 1];
			}

			Array.Resize(ref list, list.Length - 1);

			MainData.Profiles.RewriteUsers(list);
		}

		/// <summary>
		/// Зашифровать пользователя
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <returns></returns>
		public User GetProtectedUser(string key)
		{
			User user = Clone() as User;
			user.id = "0";

			User UserToCrypt = new User()
			{
				first_name = Crypt.EncryptStringAES(user.first_name, key),
				last_name = Crypt.EncryptStringAES(user.last_name, key),
				id = Crypt.EncryptStringAES(user.id, key),
				token = Crypt.EncryptStringAES(user.token, key)
			};

			return UserToCrypt;

		}

		/// <summary>
		/// Рашифровать данные пользователя
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <returns></returns>
		public User GetDecryptUser(string key)
		{
			User user = Clone() as User;

			User EncryptedUser = new User()
			{
				first_name = Crypt.DecryptStringAES(user.first_name, key),
				last_name = Crypt.DecryptStringAES(user.last_name, key),
				id = Crypt.DecryptStringAES(user.id, key),
				token = Crypt.DecryptStringAES(user.token, key)
			};
			return EncryptedUser;
		}

		#region Class

		public string id { get; set; }
		public string first_name { get; set; }
		public string last_name { get; set; }

		public string token { get; set; }

		public VkApi API { get; set; }

		#endregion
	}
}