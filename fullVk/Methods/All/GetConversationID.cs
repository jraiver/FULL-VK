using fullvk.Menu;
using System.Collections.Generic;
using System.Linq;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace fullvk.Methods.All
{
	class GetConversationID
	{
		/// <summary>
		/// Выбрать беседу
		/// </summary>
		/// <param name="api"></param>
		/// <param name="HeaderName"></param>
		/// <returns></returns>
		public static long Get(VkApi api, string HeaderName)
		{
			TextConsole.PrintConsole.Header(HeaderName);
			TextConsole.PrintConsole.Print("Получаем список бесед...",
				TextConsole.MenuType.InfoHeader);

			var Conversation = api.Messages.GetConversations(new GetConversationsParams() { Extended = true });

			var menuList = new List<string>();

			///Получаем имя беседы 
			for (int i = 0; i < Conversation.Items.Count; i++)
			{
				if (Conversation.Items[i].Conversation.Peer.Type == ConversationPeerType.Group)
				{
					for (int q = 0; q < Conversation.Groups.Count; q++)
					{
						if (Conversation.Items[i].Conversation.Peer.Id == Conversation.Profiles[q].Id)
							menuList.Add($"{Conversation.Groups[q].Name}");
					}
				}
				else if (Conversation.Items[i].Conversation.Peer.Type == ConversationPeerType.User)
				{
					for (int q = 0; q < Conversation.Profiles.Count; q++)
					{
						if (Conversation.Items[i].Conversation.Peer.Id == Conversation.Profiles[q].Id)
							menuList.Add($"{Conversation.Profiles[q].FirstName} {Conversation.Profiles[q].LastName}");
					}

				}
				else if (Conversation.Items[i].Conversation.Peer.Type == ConversationPeerType.Chat)
				{
					menuList.Add(Conversation.Items[i].Conversation.ChatSettings.Title);
				}
			}

			int pos = gMenu.Menu(menuList.ToList(), "Выберите беседу");

			if (pos == -1)
				return -1;

			return (long)Conversation.Items[pos - 1].Conversation.Peer.Id;
		}
	}
}
