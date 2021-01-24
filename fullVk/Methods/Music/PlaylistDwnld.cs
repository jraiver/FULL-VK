using System.Collections.Generic;
using System.Diagnostics;
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
				case Music.Get.GetType.Profile:
					if (data.id < 0)
					{
						var group = data.api.Groups.GetById(null, (0 - data.id).ToString(), null).FirstOrDefault();
						name = $"[PL] {group.Name}";
					}
					else
					{
						var user = data.api.Users.Get(new List<long>() {long.Parse(data.id.ToString())}).FirstOrDefault();
						name = $"[PL] {user.FirstName} {user.LastName}";
					}
					break;
				case Music.Get.GetType.Recommendation:
					name = $"[PL] {data.SubName} ";
					break;
			}


			string PlayList = "#EXTM3U\n";

			for (int i = 0; i < mediaListMedia.Length; i++)
			{
				PlayList +=
					$"\n#EXTINF:216,{mediaListMedia[i].name}\n{mediaListMedia[i].url}";
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
