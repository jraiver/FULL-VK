using System;
using System.ComponentModel;

namespace fullvk
{

	public class vkApiClass
	{

		public string access_token { get; set; }
		public string error_description { get; set; }
		public int expires_in { get; set; }
		public int user_id { get; set; }
		public string need_captcha { get; set; }

	}

	static class vk
	{
		public static vkApiClass API = null;
		public static int id { get; set; }
		public static string first_name { get; set; }
		public static string last_name { get; set; }
	}

	

}
