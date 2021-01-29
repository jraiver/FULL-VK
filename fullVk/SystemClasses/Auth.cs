using CptchCaptchaSolving;
using Microsoft.Extensions.DependencyInjection;
using System;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils.AntiCaptcha;
using static fullvk.TextConsole;

namespace fullvk.SystemClasses
{
	class Auth
	{
		const ulong _appID = 6787981;
		/// <summary>
		/// Инициализация
		/// </summary>
		public static bool Login()
		{
			string login;
			string password = "";
			string Header = "Авторизация";

			PrintConsole.Header(Header);
			
			while (true)
			{
				PrintConsole.Header(Header);
				PrintConsole.Print($"Введите логин: ", MenuType.Input);
				login = Console.ReadLine();
				if (login != null && login.Length > 0)
					break;
			}

			while (true)
			{
				PrintConsole.Header(Header);
				PrintConsole.Print($"Введите пароль: ", MenuType.Input);
				string str = string.Empty;
				ConsoleKeyInfo key;
				do
				{
					key = Console.ReadKey(true);

					if (key.Key == ConsoleKey.Enter) break;
					if (key.Key == ConsoleKey.Backspace)
					{
						if (password.Length != 0)
						{
							password = password.Remove(password.Length - 1);
							Console.Write("\b \b");
						}
					}
					else
					{
						password += key.KeyChar;
						Console.Write("*");
					}
				} while (true);

				if (password != null && password.Length > 0)
					break;
			}
		
			Console.WriteLine();
			Authorization(login, password);
			return true;
		}

		/// <summary>
		/// Авторизация
		/// </summary>
		/// <param name="login"></param>
		/// <param name="password"></param>
		static void Authorization(string login, string password)
		{
			ulong _appID = 6787981;
			string Header = "Авторизация";

			var service = new ServiceCollection();
			service.AddAudioBypass();
			service.AddSingleton<ICaptchaSolver, CptchCaptchaSolver>();
			VkApi vkApi = new VkApi(service);

			Retry:
			try
			{
				vkApi.Authorize(new ApiAuthParams
				{
					Login = login,
					Password = password,
					ApplicationId = _appID,
					Settings = Settings.All
				});

			}

			catch (Exception ex)
			{ 
				if (ex.Message.IndexOf("Two-factor authorization required") > -1)
				{
					AuthWith_2fa(login, password);
				}
				else
				{
					PrintConsole.Header(Header);
					PrintConsole.Print(ex.Message, MenuType.Warning);
					goto Retry;
				}
				return;
			}

			AuthSeccesfull(vkApi);

			PrintConsole.Header(Header);
			
			PrintConsole.Print("Авторизация прошла успешно", MenuType.InfoHeader);
			BackLine.Continue();

		}

		/// <summary>
		/// Авторизация с двухфакторной аутентификацией
		/// </summary>
		/// <param name="login"></param>
		/// <param name="password"></param>
		static void AuthWith_2fa(string login, string password)
		{
			string Header = "Авторизация";

			var service = new ServiceCollection();
			service.AddAudioBypass();
			service.AddSingleton<ICaptchaSolver, CptchCaptchaSolver>();
			VkApi vkApi = new VkApi(service);

			string key2fa;

			Retry:
			try
			{
				PrintConsole.Header(Header);
				PrintConsole.Print($"Введите код двухфакторной аутентификации: ", MenuType.Input);
				key2fa = Console.ReadLine();

				vkApi.Authorize(new ApiAuthParams
				{
					Login = login,
					Password = password,
					ApplicationId = _appID,
					Settings = Settings.All,
					TwoFactorAuthorization = () => { return key2fa; }
				});

				if (vkApi.Token != null)
				{
					AuthSeccesfull(vkApi);
				}

			}

			catch (Exception ex)
			{
				PrintConsole.Header(Header);
				PrintConsole.Print(ex.Message, MenuType.Warning);
				goto Retry;
			}
		}


		/// <summary>
		/// Проверка токена на валидность
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static VkApi AuthByToken(string token)
		{
			var services = new ServiceCollection();

			services.AddAudioBypass();
			services.AddSingleton<ICaptchaSolver, CptchCaptchaSolver>();

			VkApi vkApi = new VkApi(services);

			if (token != null)
			{
				vkApi.Authorize(new ApiAuthParams
				{
					AccessToken = token
				});

				vkApi.UserId = GlobalFunctions.GetIdNet(vkApi);
				if (vkApi.UserId == null)
					return null;
			}

			return vkApi;

		}

		/// <summary>
		/// Добавление нового пользователя
		/// </summary>
		static bool AuthSeccesfull(VkApi api)
		{
			var user = api.Account.GetProfileInfo();

			MainData.Profiles.AddUser(new User(api, user.FirstName, user.LastName, api.Token, api.UserId.ToString()));

			MainData.Profiles.RewriteUsers(MainData.Profiles.GetAllUser());
			return true;
		}

	}
}
