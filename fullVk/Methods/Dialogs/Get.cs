using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl;
using Flurl.Util;
using fullvk.Methods.All;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace fullvk.Methods.Dialogs
{
	class Get
	{
		public static void GetDialogsList(VkApi api, string HeaderName)
		{
			long ConvID = GetConversationID.Get(api, HeaderName);
			//if (ConvID < 0)
			//	return null;

			TextConsole.PrintConsole.Header(HeaderName);
			TextConsole.PrintConsole.Print("Получаем список диалогов...",
				TextConsole.MenuType.InfoHeader);

			string nextFrom = "";

			var Attachments = api.Messages.GetHistoryAttachments(
				new MessagesGetHistoryAttachmentsParams()
				{
					PeerId = ConvID,
					MediaType = MediaType.Photo
				},
				out nextFrom
			);

			ChoiseMedia.Media[] images = new ChoiseMedia.Media[0];
		//	

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
						name = name.Substring(1, name.Length - 1)
					};

				}
			}

			Download.DownloadStart(images, MediaType.Photo);


		}
	}
}
