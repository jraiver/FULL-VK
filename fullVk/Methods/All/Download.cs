using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VkNet.Enums.SafetyEnums;
using static fullvk.TextConsole;

namespace fullvk.Methods.All
{

	class Download
	{
		static Stopwatch SpeedMeter = new Stopwatch();
		static CancellationTokenSource cts = null;

		/// <summary>
		/// Отмена
		/// </summary>
		/// <param name="task">Задача</param>
		/// <param name="cts">Токен</param>
		/// 
		static void Cancel(Task task, CancellationTokenSource cts)
		{
			if (task == null)
				return;

			Console.ReadKey();
			cts.Cancel();
			task.Wait();
		}

		public static void DownloadStart(ChoiseMedia.Media[] list, MediaType TypeMedia)
		{
			try
			{
				if (list == null)
					return;
				cts = new CancellationTokenSource();

				var task = DownloadTask(list, TypeMedia, cts.Token);
				//task.Wait();

				Cancel(task, cts);

			}
			catch (Exception ex)
			{
				PrintConsole.Print(ex.Message, MenuType.Error);
			}
		}

		/// <summary>
		/// Загрущик
		/// </summary>
		/// <param name="list"></param>
		/// <param name="TypeMedia"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		static async Task DownloadTask(ChoiseMedia.Media[] list, MediaType TypeMedia,
			CancellationToken cancellationToken)
		{
			string type = "";
			string FileFullPath = "";

			if (TypeMedia == MediaType.Audio)
				type = "mp3";
			else if (TypeMedia == MediaType.Video)
				type = "mp4";

			try
			{
				string HeaderName = "Скачивание медиа";
				Console.ResetColor();
				Console.Clear();
				PrintConsole.Print(HeaderName, MenuType.Header);
				PrintConsole.Print("Для отмены нажмите любую кнопку", MenuType.Custom, ConsoleColor.DarkRed);
				PrintConsole.Print($"Скачано 0 из {list.Length}\n\n", MenuType.InfoHeader);

				for (int i = 0; i < list.Length; i++)
					PrintConsole.Print($"[{i}] {FileName(list[i])}", MenuType.Track);

				int count = 7;
				int top = 6;
				int topIndent = 4;

				Console.CursorVisible = false;

				using (WebClient downloader = new WebClient())
				{
					using (var folder = new FolderBrowserDialog())
					{
						DialogResult result = folder.ShowDialog();

						if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folder.SelectedPath))
						{
							downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
							string track = "";

							for (int i = 0; i < list.Length; i++)
							{
								Console.SetCursorPosition(0, top);
								BackLine.Clear();
								PrintConsole.Print($"Скачивается: {FileName(list[i])}\n", MenuType.Custom,
									ConsoleColor.DarkGray);

								Console.SetCursorPosition(0, i + count);
								Console.ForegroundColor = ConsoleColor.DarkMagenta;
								Console.WriteLine($"[{i + 1}] {FileName(list[i])}");

								Console.ResetColor();
								SpeedMeter = new Stopwatch();
								SpeedMeter.Start();

								downloader.DownloadProgressChanged +=
									delegate(object sender, DownloadProgressChangedEventArgs e)
									{
										Downloader_DownloadProgressChanged(sender, e, FileNameTS(list[i]),
											$"[{i}/{list.Length}]", cancellationToken);
									};

								FileFullPath = $"{Path.Combine(folder.SelectedPath, FileNameTS(list[i]))}";

								if (File.Exists(FileFullPath + $".{type}"))
								{
									for (int q = 1;; q++)
									{
										if (!File.Exists($"{FileFullPath} ({q}).{type}"))
										{
											FileFullPath = $"{FileFullPath} ({q}).{type}";
											break;
										}
									}
								}

								else
									FileFullPath += $".{type}";

								try
								{
									if (list[i].url == null)
										continue;

									await downloader.DownloadFileTaskAsync(new Uri(list[i].url), FileFullPath);
									//cancellationToken.ThrowIfCancellationRequested();
								}
								catch (WebException ex)
								{
									Console.Title = Application.ProductName;
									if (ex.Status == WebExceptionStatus.RequestCanceled)
									{
										PrintConsole.Header(HeaderName);
										PrintConsole.Print("Скачивание отменено.", MenuType.InfoHeader);
										BackLine.Continue();
										goto EndOfDownload;
									}
								}
								catch (Exception ex)
								{
									
								}

								SpeedMeter.Stop();
								Console.SetCursorPosition(0, i + count + 1);
								BackLine.Clear();
								Console.SetCursorPosition(0, i + count);
								Console.ForegroundColor = ConsoleColor.Green;
								Console.WriteLine($"[{i + 1}] {FileName(list[i])}");
								Console.ResetColor();

								Console.SetCursorPosition(0, topIndent);

								PrintConsole.Print($"Скачано {i + 1} из {list.Length}\n", MenuType.InfoHeader);

								Console.SetCursorPosition(0, i + count);
							}

							PrintConsole.Print("\nСкачивание файлов завершено.", MenuType.InfoHeader);
							Process.Start(folder.SelectedPath);
						}
						else if (result == DialogResult.Cancel)
						{
							cts.Token.ThrowIfCancellationRequested();
							PrintConsole.Print(HeaderName, MenuType.Header);
							PrintConsole.Print("Скачивание отменено.", MenuType.InfoHeader);
							PrintConsole.Print("Для продолжения нажмите любую клавишу....", MenuType.Custom, ConsoleColor.DarkGray);
						}
					}
				}

				Console.Title = Application.ProductName;
				EndOfDownload:
				Console.Title = Application.ProductName;
				Console.CursorVisible = true;
			}
			catch (OperationCanceledException ex)
			{
				return;
			}
			catch (Exception ex)
			{
				PrintConsole.Print(ex.Message, MenuType.Error);
			}
			
		}


		static void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			//throw new NotImplementedException();
		}

		static void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, string track,
			string _count, CancellationToken cancellationToken)
		{
			var speed = (Convert.ToDouble(e.BytesReceived) / 1024 / SpeedMeter.Elapsed.TotalSeconds).ToString("0,00") +
			            " KB/s";
			Console.Title =
				$"{_count} [{string.Format("{0:0.#}", (Convert.ToDouble(e.BytesReceived) / 1024 / 1024))} / {string.Format("{0:0.#}", (Convert.ToDouble(e.TotalBytesToReceive) / 1024 / 1024))} MB] [{speed}] {track}";
	
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
			catch (OperationCanceledException ex)
			{
				(sender as WebClient).CancelAsync();
			}
		}

		static string FileName(ChoiseMedia.Media file)
		{
			return $"{file.name} [{file.duration}]";
		}

		static string FileNameTS(ChoiseMedia.Media file)
		{
			return $"{file.name}";
		}
	}
}
