using fullvk.Methods.All;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;
using static fullvk.MainData;

namespace fullvk.SystemClasses
{
	class LoadSave
	{
		static string cryptPass = "5NhtX7d5WxU" + getP() + "YPbulIXVUfLF6p";
		readonly static string path = $"{Environment.CurrentDirectory}\\Users.txt";

		/// <summary>
		/// Сохранить пользователей
		/// </summary>
		/// <returns></returns>
		public static bool WriteUsers()
		{
			try
			{
				User[] toWrite = new User[Profiles.Count()];

				for (int i = 0; i < Profiles.Count(); i++)
				{
					toWrite[i] = Profiles.GetUser(i).GetProtectedUser(cryptPass);
				}

				var FavoriteData = JsonConvert.SerializeObject(toWrite, Formatting.None, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});

				File.WriteAllText(path, FavoriteData, Encoding.UTF8);

				return true;
			}
			catch (Exception ex)
			{
				TextConsole.PrintConsole.Print(ex.Message, TextConsole.MenuType.Error);
				return false;
			}

		}

		class tUser
		{
			private string _fName { get; set; }
			private string _lName { get; set; }
			private string _id { get; set; }
			private string _token { get; set; }
		}
		
		/// <summary>
		/// Загрузить пользователей
		/// </summary>
		/// <returns></returns>
		public static bool ReadUsers(bool load = true)
		{
			if (!File.Exists(path))
			{
				File.Create(path);
				Auth.Login();
				return false;
			}

			var fl = File.ReadAllText(path, Encoding.UTF8);
			var Users = JsonConvert.DeserializeObject<User[]>(File.ReadAllText(path, Encoding.UTF8));
			
			if (Users == null)
			{
				Auth.Login();
				return false;
			}

			if (Users.Length > 0)
			{
				for (int i = 0; i < Users.Length; i++)
				{
					Users[i] = Users[i].GetDecryptUser(cryptPass);

					Users[i].SetAPI(Auth.AuthByToken(Users[i].GetToken()));
					Users[i].SetId(Users[i].GetApi().UserId.ToString());

					if (Users[i].GetApi() == null)
					{
						Console.WriteLine("error read");
					}
				}

				Profiles.RewriteUsers(Users, load);
			}

			return true;
		}

		/// <summary>
		/// Чтение
		/// </summary>
		/// <returns></returns>
		public static bool ReadData(bool load = true)
		{
			string path = $"{Environment.CurrentDirectory}\\Data.txt";

			if (File.Exists(path))
			{
				var data = JsonConvert.DeserializeObject<Classes.Data>(File.ReadAllText(path, Encoding.UTF8));
				if (data != null)
				{
					for (int i = 0; i < data.LastChoise.Length; i++)
						LastChoise.Add(new KeyValuePair<long?, string>(data.LastChoise[i].url,
							data.LastChoise[i].name), true);
				}
			}
			
			ReadUsers(load);
			return true;
		}

		/// <summary>
		/// Запись информации приложения
		/// </summary>
		/// <returns></returns>
		public static bool WriteData(bool users = false)
		{
			string path = $"{Environment.CurrentDirectory}\\Data.txt";

			Classes.Data toWrite = new Classes.Data();
			toWrite.LastChoise = new Classes.Data.LastChoiseClass[LastChoise.Count()];
			var urls = LastChoise.GetAll();

			for (int i = 0; i < urls.Count;i++)
				toWrite.LastChoise[i] = new Classes.Data.LastChoiseClass()
				{
					url = urls.ElementAt(i).Key,
					name = urls.ElementAt(i).Value
				};

			for (int i = 0; i < LastChoise.Count(); i++)
			{
				toWrite.LastChoise[i] = new Classes.Data.LastChoiseClass()
				{
					url = LastChoise.Get(i).Key,
					name = LastChoise.Get(i).Value
				};
			}

			var FavoriteData = JsonConvert.SerializeObject(toWrite, Formatting.None, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			});

			File.WriteAllText(path, FavoriteData, Encoding.UTF8);
			if (users)
				WriteUsers();

			return true;
		}

		private static string getP()
		{
			string cpuInfo = string.Empty;
			ManagementClass mc = new ManagementClass("win32_processor");
			ManagementObjectCollection moc = mc.GetInstances();

			foreach (ManagementObject mo in moc)
			{
				if (cpuInfo == "")
				{
					cpuInfo = mo.Properties["processorID"].Value.ToString();
					break;
				}
			}

			return cpuInfo;
		}
	}
}
