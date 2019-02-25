// Copyright © 2019 Shawn Baker using the MIT License.
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RPiCameraViewer
{
	/// <summary>
	/// Log files page.
	/// </summary>
	public sealed partial class LogFilesPage : Page
	{
		/// <summary>
		/// Constructor - Initializes the page.
		/// </summary>
		public LogFilesPage()
		{
			InitializeComponent();
			Loaded += HandleLoaded;
		}

		/// <summary>
		/// Initializes the controls.
		/// </summary>
		private async void HandleLoaded(object sender, RoutedEventArgs e)
		{
			Log.Info("+LogFilesPage.HandleLoaded");
			await DisplayLogFileAsync(Log.CurrentFile);
			Log.Info("-LogFilesPage.HandleLoaded");
		}

		/// <summary>
		/// Display the contents of File1.
		/// </summary>
		private async void HandleFile1ButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("LogFilesPage.HandleFile1ButtonClick");
			await DisplayLogFileAsync(Log.File1);
		}

		/// <summary>
		/// Display the contents of File2.
		/// </summary>
		private async void HandleFile2ButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("LogFilesPage.HandleFile2ButtonClick");
			await DisplayLogFileAsync(Log.File2);
		}

		/// <summary>
		/// Prompt the user to delete the contents of the log files.
		/// </summary>
		private void HandleClearButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("LogFilesPage.HandleClearButtonClick");
			Utils.YesNoAsync(Res.Str.OkToClearLogs, (command) =>
			{
				Log.Cleared += HandleLogCleared;
				Log.Clear();
			});
		}

		/// <summary>
		/// Email the zipped log files.
		/// </summary>
		private async void HandleEmailButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("LogFilesPage.HandleEmailButtonClick");
			EmailMessage message = new EmailMessage();
			message.To.Add(new EmailRecipient("support@frozen.ca"));
			message.Subject = Res.Str.AppName + " " + Res.Str.LogFiles;
			StorageFile zipFile = await ApplicationData.Current.LocalFolder.GetFileAsync("LogFiles.zip");
			if (zipFile != null)
			{
				await zipFile.DeleteAsync();
			}
			string fileName = Path.Combine(ApplicationData.Current.LocalFolder.Path, "LogFiles.zip");
			message.Body = string.Format(Res.Str.AttachmentWarning, fileName);
			ZipFile.CreateFromDirectory(Log.Folder.Path, fileName, CompressionLevel.Optimal, true);
			zipFile = await ApplicationData.Current.LocalFolder.GetFileAsync("LogFiles.zip");
			if (zipFile != null)
			{
				var stream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(zipFile);
				var attachment = new EmailAttachment(zipFile.Name, stream);
				message.Attachments.Add(attachment);
				await EmailManager.ShowComposeNewEmailAsync(message);
			}
			else
			{
				Utils.ErrorAsync(Res.Error.CouldntCreateZipFile);
			}
		}

		/// <summary>
		/// Return to the previous page.
		/// </summary>
		private void HandleBackButtonClick(object sender, RoutedEventArgs e)
		{
			Log.Info("LogFilesPage.HandleBackButtonClick");
			Frame.GoBack();
		}

		/// <summary>
		/// Displays the log file after being cleared.
		/// </summary>
		private async void HandleLogCleared(object sender, EventArgs e)
		{
			Log.Info("LogFilesPage.HandleClearButtonClick: cleared");
			Log.Cleared -= HandleLogCleared;
			await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
			{
				await DisplayLogFileAsync(Log.File1);
			});
		}

		/// <summary>
		/// Displays the contents of one of the log files.
		/// </summary>
		/// <param name="file">Log file to be displayed.</param>
		/// <returns>The task.</returns>
		private async Task DisplayLogFileAsync(StorageFile file)
		{
			bool isFile1 = file == Log.File1;
			Log.Info("+LogFilesPage.DisplayLogFileAsync: {0}", isFile1);
			string text = await FileIO.ReadTextAsync(file);
			Utils.CreateInlines(linesTextBlock, text, null);
			scrollViewer.UpdateLayout();
			scrollViewer.ChangeView(0, double.MaxValue, 1, true);
			file1Button.IsChecked = isFile1;
			file2Button.IsChecked = !isFile1;
			Log.Info("-LogFilesPage.DisplayLogFileAsync");
		}
	}
}
