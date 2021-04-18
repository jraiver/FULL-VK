using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using fullvk.Methods.All;

namespace fullvk.Methods.Music
{
	class PlaylistDwnld
	{
		public static void Get(AnyData.Data data)
		{
			ChoiseMedia.Media[] mediaListMedia = data.mediaList as ChoiseMedia.Media[];

			string name = "";


			if (data.SubName != null)
				name = data.SubName;

			if (data.id == null)
				data.id = data.api.UserId;

			switch (data.type)
			{
				case Music.Get.Type.Profile:
					if (data.id < 0)
					{
						var group = data.api.Groups.GetById(null, (0 - data.id).ToString(), null).FirstOrDefault();
						name = $"[PL] {group.Name}";
					}
					else
					{
						var user = data.api.Users.Get(new List<long>() { long.Parse(data.id.ToString()) }).FirstOrDefault();
						name = $"[PL] {user.FirstName} {user.LastName}";
					}
					break;
				case Music.Get.Type.Recommendation:
					name = $"[PL] {data.SubName} ";
					break;
				case Music.Get.Type.Daily:
					name = $"Дневная подборка {DateTime.Today.ToShortDateString()}";
					break;
				case Music.Get.Type.Weekly:
					name = $"Недельная подборка {DateTime.Today.ToShortDateString()}";
					break;
			}


			string PlayList = "#EXTM3U\n";

			for (int i = 0; i < mediaListMedia.Length; i++)
			{
				try
				{
					string seconds = String.Empty;
					var prep = mediaListMedia[i].duration.Split(':');

					if (prep.Length == 2)
						seconds = $"00:{mediaListMedia[i].duration}";
					else seconds = mediaListMedia[i].duration;

					var sec = (int)DateTime.Parse(seconds).TimeOfDay.TotalSeconds;

					PlayList +=
						$"\n#EXTINF:{sec},{mediaListMedia[i].name}\n{mediaListMedia[i].url}";
				}
				catch (Exception ex)
				{

				}
			}



			if (mediaListMedia.Length == 0)
			{
				TextConsole.BackLine.Continue();
				return;
			}

			SaveFileDialog dialog = new SaveFileDialog()
			{
				Title = "Сохранить плейлист",
				FileName = name,
				Filter = "M3U (*.m3u)|*.m3u"
			};

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				using (dialog)
				{
					File.WriteAllText(Path.GetFullPath(dialog.FileName), PlayList);
					Process.Start(Path.GetFullPath(dialog.FileName));
				}
			}
		}
	}
}
