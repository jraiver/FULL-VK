using System;
using System.Threading;

namespace CptchCaptchaSolving
{
	public class CptchCaptchaSolver : VkNet.Utils.AntiCaptcha.ICaptchaSolver
	{
		public CptchCaptchaSolver() { }

		public string Solve(string url)
		{
			return fullvk.CaptchaProcessing.EnterCaptcha(url);
		}
		
		public void CaptchaIsFalse()
		{
			Console.WriteLine("Капча введена неверно");
			Thread.Sleep(500);
		}
	}
}