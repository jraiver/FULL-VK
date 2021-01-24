using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace fullvk.Methods.All

{
	internal class Classes
	{
		public static bool STOP = false;

		#region Classes

		public class Data
		{
			public LastChoiseClass[] LastChoise { get; set; }
			public class LastChoiseClass
			{
				public long? url { get; set; }
				public string name{ get; set; }
			}
		}

		public class Response
		{
			public object response { get; set; }
			public object error { get; set; }
			public string error_description { get; set; }
			public string error_type { get; set; }

			public class errorResponse
			{
				public string error_msg { get; set; } = "";
				public string captcha_sid { get; set; } = "";
				public string captcha_img { get; set; } = "";
				public int error_code { get; set; }
				public string validation_type { get; set; } = "";
			}

			public class errorCaptcha
			{
				public string captcha_sid { get; set; } = "";
				public string captcha_img { get; set; } = "";

			}

			public class validation
			{
				public string validation_type { get; set; }
				public string phone_mask { get; set; }
			}


		}

		public class ResponseItems : Response
		{
			public int count { get; set; }
			public object[] items { get; set; }
		}

		public class ItemToClear
		{
			public int id { get; set; }
			public int owner_id { get; set; }
			public string title { get; set; }
			public string text { get; set; }
		}

		public class ResponseUser
		{
			public int id { get; set; }
			public object city { get; set; }
			public string first_name { get; set; }
			public string last_name { get; set; }
			public string photo_50 { get; set; }
			public bool is_closed { get; set; }
		}

		public class UserInfo
		{
			public object[] response { get; set; }
			public int id { get; set; }
			public string first_name { get; set; }
			public string last_name { get; set; }
			public bool is_closed { get; set; }
			public bool can_access_closed { get; set; }
		}

		#endregion

		public static class KeyboardSend
		{
			[DllImport("user32.dll")]
			private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

			private const int KEYEVENTF_EXTENDEDKEY = 1;
			private const int KEYEVENTF_KEYUP = 2;

			public static void KeyDown(Keys vKey)
			{
				keybd_event((byte)vKey, 0, KEYEVENTF_EXTENDEDKEY, 0);
			}

			public static void KeyUp(Keys vKey)
			{
				keybd_event((byte)vKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
			}
		}

		public class GetIdFromAlias : Response
		{
			public static long? GetFromResponce(string result)
			{
				try
				{
					Response respon = JsonConvert.DeserializeObject<Response>(result);

					if ((respon.response as JObject).Count != 0)
					{
						GetIdFromAlias data =
							JsonConvert.DeserializeObject<GetIdFromAlias>(respon.response.ToString());

						if (string.Compare(data.type, "group") == 0)
							return long.Parse(data.object_id.ToString()) * -1;
						else
							return long.Parse(data.object_id.ToString());
					}
					else
						return null;
				}
				catch (Exception ex)
				{
					return null;
				}

			}

			public string type { get; set; }
			public int object_id { get; set; }
		}

	}
}