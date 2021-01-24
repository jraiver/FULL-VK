using fullvk.Methods.Music;
using VkNet;
using VkNet.Enums.SafetyEnums;

namespace fullvk.Methods.All
{
	class AnyData
	{
		public class Data
		{
			public Get.GetType type { get; set; } = Get.GetType.Profile;
			public MediaType mType { get; set; }
			public string SubName { get; set; }
			public Get.Track[] audios { get; set; }
			public VkApi api { get; set; }
			public long? id { get; set; }
			public ChoiseMedia.Media[] mediaList { get; set; }
			public object AnyData { get; set; }


			public static ChoiseMedia.Media[] FromTask(object dataFromTask)
			{
				ChoiseMedia.Media[] media = new ChoiseMedia.Media[0];

				return dataFromTask as ChoiseMedia.Media[];
			}
		}

	}
}
