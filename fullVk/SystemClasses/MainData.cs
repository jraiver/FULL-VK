using fullvk.Menu;
using fullvk.SystemClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VkNet;
using VkNet.Model;

namespace fullvk
{
	internal class MainData
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
				if (i < 0 && i > UsersList.Length - 1)
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
						$"{Profiles.GetUser(i).GetName()} [{Profiles.GetUser(i).GetId()}]");
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

		public User(VkApi api, string fName, string lName, string token, string id)
		{
			_id = id;
			_fName = fName;
			_lName = lName;
			_token = token;
			_api = api;
		}
		
		int GetUserId()
		{
			User[] list = MainData.Profiles.GetAllUser();

			for (int i = 0; i < MainData.Profiles.Count(); i++)
			{
				if (string.Compare(_id, MainData.Profiles.GetUser(i)._id) == 0)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Удалить пользователя
		/// </summary>

		public void SetId(string id) => _id = id;

		public void SetAPI(VkApi api) => _api = api;
		
		public void Delete()
		{
			User[] list = MainData.Profiles.GetAllUser();
			int ProfileId = GetUserId();

			for (int i = ProfileId; i < list.Length; i++)
			{
				if (i + 1 == list.Length)
					break;

				list[i] = list[i + 1];
			}

			Array.Resize(ref list, list.Length - 1);

			MainData.Profiles.RewriteUsers(list);
			if (MainData.Profiles.Count() == 0)
				Auth.Login();
		}

		/// <summary>
		/// Зашифровать пользователя
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <returns></returns>
		public User GetProtectedUser(string key)
		{
			User user = Clone() as User;
			user._id = "0";
			user._api = null;
			return new User(null,fName: Crypt.EncryptStringAES(user._fName, key), 
				lName: Crypt.EncryptStringAES(user._lName, key), 
				Crypt.EncryptStringAES(user._token, key), 
				Crypt.EncryptStringAES(user._id, key)
			);;

		}

		/// <summary>
		/// Рашифровать данные пользователя
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <returns></returns>
		public User GetDecryptUser(string key)
		{
			User user = Clone() as User;

			return new User(null,Crypt.DecryptStringAES(user._fName, key), 
				Crypt.DecryptStringAES(user._lName, key),
				Crypt.DecryptStringAES(user._token, key),
				Crypt.DecryptStringAES(user._id, key)
				);
		}

		/// <summary>
		/// Имя пользователя
		/// </summary>
		/// <returns>Имя и фамилию</returns>
		public string GetName() => $"{_fName} {_lName}";

		/// <summary>
		/// Токен
		/// </summary>
		/// <returns>Токен </returns>
		public string GetToken() => _token;

		/// <summary>
		/// ID пользователя 
		/// </summary>
		/// <returns>ID</returns>
		public long? GetId()
		{
			long? id = long.Parse(_id);
			if (_api.UserId == null)
				return id;
			else return _api.UserId;
		}

		/// <summary>
		/// VkApi пользователя
		/// </summary>
		/// <returns>VkApi</returns>
		public VkApi GetApi() => _api;
		
		#region Var

		[JsonProperty]
		string _id { get; set; }
		
		[JsonProperty]
		string _fName { get; set; }
		
		[JsonProperty]
		string _lName { get; set; }
		
		[JsonProperty]
		string _token { get; set; }
		
		VkApi _api { get; set; }

		#endregion
	}
}