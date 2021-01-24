using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace fullvk
{
	class CheckErrorAPI
	{
		class Errors
		{
			public static Error get(object response)
			{
				Error resp =  JsonConvert.DeserializeObject<Error>(response.ToString());

				if (resp.error_code != null)
					return resp;
				else if (resp.error == null)
					return null;
				else if(resp.error.ToString()[0] == '{')
					return JsonConvert.DeserializeObject<Error>(resp.error.ToString());
					return resp;
				
			}

			public class Error
			{
				public object response { get; set; }
				public object error { get; set; }
				public string error_description { get; set; }
				public string error_type { get; set; }
				public int error_code { get; set; }
				public string error_msg { get; set; }
				public string captcha_sid { get; set; }
				public string captcha_img { get; set; }
			}

			private static Dictionary<int, string> errorMessage = new Dictionary<int, string>()
			{
				{6, "Слишком много запросов в секунду."},
				{14, "Требуется ввод кода с картинки (Captcha)."},
				{15, "Доступ запрещён."},
				{18, "Пользователь удалён либо заблокирован."},
				{30, "Профиль является приватным."},
				{113, "Неверный идентификатор пользователя."},
				{210, "Нет доступа к записи."},
				
			};

			public static string getErrorMessage(int code)
			{
				try
				{
					return errorMessage[code];
				}
				catch (Exception ex)
				{
					if (ex.GetType() == typeof(KeyNotFoundException))
					{
						return "Неизвестная ошибка";
					}
					else
					{
						return ex.Message;
					}
				}
			}

		}

		/// <summary>
		/// Проверка ответа API на ошибки.
		/// -1 - ошибок нет.
		/// null - была капча, но не введена.
		/// </summary>
		/// <param name="response">Ответ API.</param>
		/// <param name="PrintMess">Выводить сообщение об ошибке в консоль.</param>
		/// <returns></returns>
		public static object [] checkAPI(object response, bool PrintMess = false)
		{			
			Errors.Error resp = Errors.get(response);

			if (resp == null)
				return new object [] {-1};
			else if (resp.error_code == 6)
			{
				Thread tooManyPerSecond = new Thread(TooMany);
				tooManyPerSecond.Start(5);
				tooManyPerSecond.Join();
				return new object[] { resp.error_code};
			}
			else if (resp.error != null && string.Compare(resp.error.ToString(), "need_captcha") == 0 || resp.error_code != null && resp.error_code == 14)
			{
				string capt = pCaptcha(resp.captcha_img, resp.captcha_sid);
				if (capt != null)
					return new object[] { 14, capt };

				return null;
			}
			else if (resp.error_code != null && resp.error_code == 0)
			{
				return new object[] { resp.error_code, resp.error };
			}
			else if (resp.error_msg == null && resp.error_type == null)
			{
				Errors.Error error = JsonConvert.DeserializeObject<Errors.Error>(resp.error.ToString());

				if (PrintMess && resp.error_code != 14)
					PrintMessage(error.error_code);

				return new object[] {error.error_code, error.error_msg};

			}
			else if (resp.error_msg != null)
			{
				if (PrintMess && resp.error_code != 14)
					PrintMessage(resp.error_code);

				return new object[] { resp.error_code, resp.error_msg };
			}
			else
				return new object[] { resp.error_code, resp.error_msg, resp.error};

		}

		private static string pCaptcha(string capchaImg, string CaptchaSid)
		{
			string capt = CaptchaProcessing.EnterCaptcha(capchaImg);

			if (capt != null)
				return $"&captcha_sid={CaptchaSid}&captcha_key={capt}";
			else
				return null;
		}

		private static void PrintMessage(int codeError, TextConsole.MenuType type = TextConsole.MenuType.Error)
		{
			TextConsole.PrintConsole.Print(Errors.getErrorMessage(codeError), type);
		}

		private static void TooMany(object wait)
		{
			for (int i = 0; i < (int)wait; i++)
			{
				PrintMessage(6, TextConsole.MenuType.Warning);
				TextConsole.PrintConsole.Print($"Повторная попытка через: {6 - i}");
				TextConsole.BackLine.Back(25);
				Thread.Sleep(1000);
			}
			TextConsole.BackLine.Clear();
			TextConsole.BackLine.Clear();
		}
	}
}
