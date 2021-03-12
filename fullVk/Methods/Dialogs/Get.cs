using fullvk.Methods.All;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace fullvk.Methods.Dialogs
{
	class Get
	{

		public static ReadOnlyCollection<HistoryAttachment> GetDialogsAttachments(VkApi api, string HeaderName, MediaType type)
		{
			long ConvID = GetConversationID.Get(api, HeaderName);

			TextConsole.PrintConsole.Header(HeaderName);
			TextConsole.PrintConsole.Print("Получаем список диалогов...",
				TextConsole.MenuType.InfoHeader);

			string nextFrom = "";

			var Attachments = api.Messages.GetHistoryAttachments(
				new MessagesGetHistoryAttachmentsParams()
				{
					PeerId = ConvID, MediaType = type, Count = 200
				},
				out nextFrom
			);

			return Attachments;

		}

		public static void GetImages(VkApi api, string HeaderName)
		{
			var Attachments = GetDialogsAttachments(api, HeaderName, MediaType.Photo);

			ChoiseMedia.Media[] images = new ChoiseMedia.Media[0];

			for (int i = 0; i < Attachments.Count; i++)
			{
				var image = (Attachments[i].Attachment.Instance as VkNet.Model.Attachments.Photo);
				if (image.OwnerId != api.UserId)
				{
					Array.Resize(ref images, images.Length + 1);
					var value = (Attachments[i].Attachment.Instance as VkNet.Model.Attachments.Photo).Sizes.Last().Url
						.ToString();
					var name = new Regex(@"[\S]+(.jpg)").Match(value).Value;
					name = name.Substring(name.LastIndexOf("/") + 1, name.Length - name.LastIndexOf("/") - 1);

					images[images.Length - 1] = new ChoiseMedia.Media()
					{
						url = value,
						name = name.Substring(1, name.Length - 1),
						duration = "IMG"
					};

				}
			}

			Download.DownloadStart(images, MediaType.Photo);
		}


		//public static void GetVoiceMessages(VkApi api, string HeaderName)
		//{
		//	var Attachments = GetDialogsAttachments(api, HeaderName, MediaType.Audio);

		//	ChoiseMedia.Media[] vMessages = new ChoiseMedia.Media[0];

		//	for (int i = 0; i < Attachments.Count; i++)
		//	{
		//		var message = (Attachments[i].Attachment.Instance as VkNet.Model.Attachments.AudioMessage);

		//		if (message.OwnerId != api.UserId)
		//		{
		//			Array.Resize(ref vMessages, vMessages.Length + 1);

		//			var value = (Attachments[i].Attachment.Instance as VkNet.Model.Attachments.AudioMessage).LinkMp3.ToString();


		//			var name = new Regex(@"[\S]+(.jpg)").Match(value).Value;

		//			name = name.Substring(name.LastIndexOf("/") + 1, name.Length - name.LastIndexOf("/") - 1);

		//			vMessages[vMessages.Length - 1] = new ChoiseMedia.Media()
		//			{
		//				url = value,
		//				name = name.Substring(1, name.Length - 1),
		//				duration = "VOICE"
		//			};

		//		}
		//	}

		//	Download.DownloadStart(vMessages, MediaType.Audio);
		//}
	}
}
