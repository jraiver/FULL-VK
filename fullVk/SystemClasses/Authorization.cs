using CptchCaptchaSolving;
using fullvk.Methods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using fullvk.Methods.All;
using VkNet;
using VkNet.AudioBypassService.Exceptions;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.NLog.Extensions.Logging;
using VkNet.NLog.Extensions.Logging.Extensions;
using VkNet.Utils.AntiCaptcha;
using static fullvk.Methods.All.Classes;
using static fullvk.TextConsole;
using static fullvk.MainData;

namespace fullvk
{
	class Authorization1
	{
		private protected static class offApps
		{
			public class Android
			{
				public static string client_id => "2274003";
				public static string client_secret => "hHbZxrka2uZ6jB1inYsH";
			}

			public class IPhone
			{
				public static string client_id => "3140623";
				public static string client_secret => "VeWdmVclDCtn6ihuP1nt";
			}

			public class IPad
			{
				public static string client_id => "3682744";
				public static string client_secret => "mY6CDUswIVdJLCD3j15n";
			}

			public class Windows
			{
				public static string client_id => "3697615";
				public static string client_secret => "AlVXZFMUqyrnABp8ncuU";
			}

			public class WindowsPhone
			{
				public static string client_id => "3502557";
				public static string client_secret => "PEObAuQi6KloPM4T30DV";
			}

			public class Standalone
			{
				public static string client_id => "5582694";
				public static string client_secret => "27BYHlhhDg9incWBn30T";
			}
		}

		static string client_id = offApps.IPhone.client_id;
		static string client_secret = offApps.IPhone.client_secret;

		static string VkNET_Token = null;
		static string _2_fa = "";

		static WebClient api = new WebClient() {Encoding = Encoding.UTF8};

		static vkApiClass data;

		enum Scope
		{
			notify = 1,
			friends = 2,
			photos = 4,
			audio = 8,
			video = 16,
			docs = 32,
			notes = 64,
			pages = 128,
			status = 256,
			offers = 512,
			questions = 1024,
			wall = 2048,
			groups = 4096,
			messages = 8192,
			notifications = 32768,
			stats = 131072,
			offline = 524288,

			all = notify + friends + photos + audio + video + docs + notes + pages + status + offers + questions +
			      wall + groups + messages + notifications + stats + offline
		}

		public static void StandaloneAuth()
		{
			string ur = @"https:\\chlen.com";

			Form qauth = new Form() {Width = 600, Height = 400};

			//qw.Navigate($"https://oauth.vk.com/authorize?client_id=5582694&redirect_ur={ur}&display=mobile&response_type=token&{Scope.all}");

			qauth.ShowDialog();
		}

		public static VkApi vkNetAuthByToken(string token)
		{
			ulong _appID = 6787981;
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
			}

			return vkApi;

		}

		private static ILogger<T> InitLogger<T>()
		{
			var consoleTarget = new ColoredConsoleTarget
			{
				UseDefaultRowHighlightingRules = true,
				Layout = "${level} ${longdate} ${message}"
			};
			var fileTarget = new FileTarget
			{
				FileName = "log.txt",
				DeleteOldFileOnStartup = true,
				Layout = "${level} ${longdate} ${message}"
			};
			var config = new LoggingConfiguration();
			config.AddTarget("console", consoleTarget);
			config.AddTarget("file", fileTarget);
			var rule = new LoggingRule("*", NLog.LogLevel.Trace, consoleTarget);
			var rule1 = new LoggingRule("*", NLog.LogLevel.Trace, fileTarget);
			config.LoggingRules.Add(rule);
			config.LoggingRules.Add(rule1);
			LogManager.Configuration = config;

			var loggerFactory = new LoggerFactory();
			loggerFactory.AddNLog(new NLogProviderOptions
				{CaptureMessageTemplates = true, CaptureMessageProperties = true});
			return loggerFactory.CreateLogger<T>();
		}

		public static bool Authentification(bool start)
		{
			api.Headers[HttpRequestHeader.AcceptLanguage] = "ru-RU";

			string[] lData = new string[2];

			Console.Clear();
			PrintConsole.Print($"{Tab()}Авторизация{Tab()}", MenuType.Header);

			inputLogin:
			PrintConsole.Print($"{Tab()}Введите логин: ", MenuType.Input);

			lData[0] = Console.ReadLine();

			if (lData[0].Length < 1)
			{
				BackLine.Clear();
				goto inputLogin;
			}

			inputPass:
			PrintConsole.Print($"\n{Tab()}Введите пароль:", MenuType.Input);
			string str = string.Empty;
			ConsoleKeyInfo key;
			do
			{
				key = Console.ReadKey(true);

				if (key.Key == ConsoleKey.Enter) break;
				if (key.Key == ConsoleKey.Backspace)
				{
					if (lData[1].Length != 0)
					{
						lData[1] = lData[1].Remove(lData[1].Length - 1);
						Console.Write("\b \b");
					}
				}
				else
				{
					lData[1] += key.KeyChar;
					Console.Write("*");
				}
			} while (true);

			if (lData[1] == null || lData[1].Length < 1)
			{
				BackLine.Clear();
				goto inputPass;
			}

			VkNET_Token = VkNetAuth(lData[0], lData[1], _2_fa);

			Console.WriteLine();
			

			return startLogin(lData[0], lData[1]);

		}


		static bool startLogin(string login, string password)
		{

			string loginURL =
				$"https://oauth.vk.com/token?grant_type=password&client_id={client_id}&client_secret={client_secret}&username={login}" +
				$"&password={password}&v=5.92&2fa_supported=1";

			User read = new User();

			GetData(new string[3]
			{
				loginURL, login, password
			});

			if (data != null)
			{
				getDataFromUser(data.access_token, login, password);
				SystemClasses.LoadSave.WriteUsers();

				return true;
			}
			else
				return false;

		}

		static string VkNetAuth(string _login, string _password, string _2fa = "")
		{

			ulong _appID = 6787981;
			var service = new ServiceCollection();
			service.AddAudioBypass();
			service.AddSingleton<ICaptchaSolver, CptchCaptchaSolver>();

			VkApi vkApi = new VkApi(service);

			try
			{
				vkApi.Authorize(new ApiAuthParams
				{
					Login = _login,
					Password = _password,
					ApplicationId = _appID,
					Settings = Settings.All
				//	TwoFactorAuthorization = () => { return _2fa; }
				});

			}

			catch (Exception ex)
			{
				if (ex is VkAuthException)
				{
					VkAuthException vkAuthException = ex as VkAuthException;

					switch (vkAuthException.AuthError.ErrorType)
					{
						case "wrong_otp":
							GetData(new string[3]
							{
								null,
								_login,
								_password
							}, true);
							break;
					}

				}

			}

			return vkApi.Token;
		}

		static void GetData(string[] auth, bool toNET = false)
		{
			try
			{
				if (!toNET)
				{
					vkApiClass accessResponse = JsonConvert.DeserializeObject<vkApiClass>(api.DownloadString(auth[0]));
					data = accessResponse;
				}
				else
				{
					Console.Clear();
					PrintConsole.Print("Двухфакторная аутентификация", MenuType.Header);

					PrintConsole.Print("Введите код из приложения генерации кодов/sms: ", MenuType.Input);
					_2_fa = Console.ReadLine();
				}

				VkNET_Token = VkNetAuth(auth[1], auth[2], _2_fa);
			}
			catch (Exception ex)
			{
				if (ex is WebException)
				{
					string content;

					WebException exept = ex as WebException;
					var temp = exept.Response as HttpWebResponse;

					using (var r = new StreamReader(exept.Response.GetResponseStream()))
						content = r.ReadToEnd();

					switch (temp.StatusCode)
					{
						case HttpStatusCode.Unauthorized:

							Response response = JsonConvert.DeserializeObject<Classes.Response>(content);

							object[] errorStatus = CheckErrorAPI.checkAPI(content, true);
							if (errorStatus == null)
								return;
							switch (errorStatus[0])
							{
								case 0:
									switch (errorStatus[1].ToString())
									{
										case "need_captcha":

											break;
										case "invalid_client":
											goto case "invalid_request";
										case "invalid_request":
											switch (response.error_type)
											{
												case "otp_format_is_incorrect":
													goto case "wrong_otp";
												case "wrong_otp":
													PrintConsole.Print(response.error_description, MenuType.Error);
													PrintConsole.Print("Для продолжения нажмите любую клавишу...");
													auth[0] = clearLogintUrl(auth[0]);
													GetData(auth);
													break;
												default:
													PrintConsole.Print(response.error_description, MenuType.Error);
													break;
											}

											break;

										case "need_validation":
											Response.validation data =
												JsonConvert.DeserializeObject<Response.validation>(content);

											Console.Clear();
											PrintConsole.Print("Двухфакторная аутентификация", MenuType.Header);

											switch (data.validation_type)
											{
												case "2fa_app":
													if (auth[0].IndexOf("&force_sms=1") > -1)
														goto case "2fa_sms";

													if (data.phone_mask != null)
														PrintConsole.Print(
															$"[1] - Получить SMS с кодом на номер {data.phone_mask}",
															MenuType.Menu);

													PrintConsole.Print("Введите код из приложения генерации кодов: ",
														MenuType.Input);

													string code2fa_app = Console.ReadLine();
													_2_fa = code2fa_app;

													if (String.Compare(code2fa_app, "1") == 0)
													{
														auth[0] = clearLogintUrl(auth[0]) + "&force_sms=1";
														GetData(auth);
													}
													else if (String.Compare(code2fa_app, "0") == 0)
														return;
													else
													{
														auth[0] = clearLogintUrl(auth[0]) + $"&code={code2fa_app}";
														GetData(auth);
													}


													break;
												case "2fa_sms":

													PrintConsole.Print($"[1] - Ввести код из генератора паролей",
														MenuType.Menu);

													PrintConsole.Print("Введите код из смс: ", MenuType.Input);

													string code2fa_sms = Console.ReadLine();
													_2_fa = code2fa_sms;
													if (String.Compare(code2fa_sms, "1") == 0)
													{
														auth[0] = clearLogintUrl(auth[0]) + "&force_sms=0";
														GetData(auth);
													}

													else if (String.Compare(code2fa_sms, "0") == 0)
														return;
													else
													{
														auth[0] += $"&code={code2fa_sms}";
														GetData(auth);
													}

													break;
											}

											break;
									}

									break;
								case 14:
									GetData(auth);
									break;

							}

							break;
					}


				}
			}


		}

		static string clearLogintUrl(string url)
		{
			string lastPar = "&2fa_supported=1";
			return url.Substring(0, url.IndexOf(lastPar) + lastPar.Length);
		}

		static bool getDataFromUser(string token, string _l = null, string _p = null, string net_Token = null,
			bool start = false, VkApi vkapi = null)
		{
			if (VkNET_Token != null)
				net_Token = VkNET_Token;

			string result = api.DownloadString($"https://api.vk.com/method/users.get?&v=5.92&access_token={token}");
			Response response = JsonConvert.DeserializeObject<Response>(result);

			if (response.error != null)
			{
				Response.errorResponse err =
					JsonConvert.DeserializeObject<Response.errorResponse>(response.error.ToString());


				//обработать ошибку Console.ReadKey();
				return false;
			}
			else
			{
				UserInfo usr =
					JsonConvert.DeserializeObject<UserInfo>(response.response.ToString()
						.Substring(1, response.response.ToString().Length - 2));

				return true;
			}

		}

		static bool setClient()
		{
			Start:
			Console.Clear();
			PrintConsole.Print($"{Tab()}Авторизация{Tab()}", MenuType.Header);
			PrintConsole.Print(
				$"[1] - Android\n[2] - iPhone\n[3] - iPad\n[4] - Windows\n[5] - Windows Phone\n[6] - Standalone",
				MenuType.Menu);

			PrintConsole.Print("Выберите клиент для авторизации: ", MenuType.Input);

			switch (Console.ReadKey().KeyChar)
			{
				case '1':
					client_id = offApps.Android.client_id;
					client_secret = offApps.Android.client_secret;
					break;
				case '2':
					client_id = offApps.IPhone.client_id;
					client_secret = offApps.IPhone.client_secret;
					break;
				case '3':
					client_id = offApps.IPad.client_id;
					client_secret = offApps.IPad.client_secret;
					break;
				case '4':
					client_id = offApps.Windows.client_id;
					client_secret = offApps.Windows.client_secret;
					break;
				case '5':
					client_id = offApps.WindowsPhone.client_id;
					client_secret = offApps.WindowsPhone.client_secret;
					break;
				case '6':
					client_id = offApps.Standalone.client_id;
					client_secret = offApps.Standalone.client_secret;
					break;
				case '0':
					return false;
				default:
					goto Start;

			}

			return true;
		}

		public static bool ClearReadUsers()
		{
			try
			{
				SystemClasses.LoadSave.ReadUsers();
				SystemClasses.LoadSave.ReadData();
				return true;
			}
			catch (Exception ex)
			{
				Task<bool> auth = new Task<bool>(() => Authentification(true));
				auth.Start();
				auth.Wait();
				return auth.Result;
			}

		}
	}
}
