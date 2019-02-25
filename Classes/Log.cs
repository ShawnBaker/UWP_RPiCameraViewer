// Copyright © 2019 Shawn Baker using the MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace RPiCameraViewer
{
	public static class Log
	{
		// public constants
		public const string FOLDER_NAME = "Logs";
		public const ulong MIN_FILE_SIZE = 1024 * 1024;

		// public events
		public static event EventHandler Cleared = null;

		// public properties
		public static StorageFile File1 { get; private set; } = null;
		public static StorageFile File2 { get; private set; } = null;
		public static StorageFile CurrentFile { get; private set; } = null;
		public static StorageFolder Folder { get; private set; } = null;

		// static variables
		private static LogLevel level = LogLevel.Info;
		private static ulong maxFileSize = MIN_FILE_SIZE;
		private static List<string> messages = new List<string>();
		private static AutoResetEvent signal = new AutoResetEvent(false);
		private static bool clear = false;

		/// <summary>
		/// Constructor - Starts the log writing thread.
		/// </summary>
		static Log()
		{
			Task.Run(WriteLogFileAsync);
		}

		/// <summary>
		/// Writes lines to the log file.
		/// </summary>
		/// <returns>The task.</returns>
		private static async Task WriteLogFileAsync()
		{
			// get the Logs folder
			Folder = ApplicationData.Current.LocalFolder;
			Folder = await Folder.CreateFolderAsync(FOLDER_NAME, CreationCollisionOption.OpenIfExists);

			// get the two log files
			File2 = await Folder.CreateFileAsync("2.log", CreationCollisionOption.OpenIfExists);
			File1 = await Folder.CreateFileAsync("1.log", CreationCollisionOption.OpenIfExists);

			// get the current log file
			BasicProperties prop1 = await File1.GetBasicPropertiesAsync();
			BasicProperties prop2 = await File2.GetBasicPropertiesAsync();
			CurrentFile = (prop1.DateModified >= prop2.DateModified) ? File1 : File2;
			var stream = await CurrentFile.OpenStreamForWriteAsync();
			stream.Seek(0, SeekOrigin.End);

			// process log file events
			List<string> myMessages = new List<string>();
			while (true)
			{
				// wait for an event
				signal.WaitOne();

				// clear the log files
				if (clear)
				{
					CurrentFile = File1 = await Folder.CreateFileAsync("1.log", CreationCollisionOption.ReplaceExisting);
					File2 = await Folder.CreateFileAsync("2.log", CreationCollisionOption.ReplaceExisting);
					clear = false;
					Cleared.Invoke(null, EventArgs.Empty);
				}

				// get the messages to be written
				lock (messages)
				{
					myMessages.AddRange(messages);
					messages.Clear();
				}

				// write the messages to the current log file
				foreach (string message in myMessages)
				{
					Debug.WriteLine(message);
					try
					{
						byte[] bytes = Encoding.UTF8.GetBytes(message);
						await stream.WriteAsync(bytes);
						stream.WriteByte(13);
						await stream.FlushAsync();
					}
					catch (Exception ex)
					{
						Debug.WriteLine("AppendTextAsync EXCEPTION: " + ex.Message);
					}
				}
				myMessages.Clear();

				// switch log files if necessary
				BasicProperties prop = await CurrentFile.GetBasicPropertiesAsync();
				if (prop.Size >= maxFileSize)
				{
					if (CurrentFile == File1)
					{
						CurrentFile = File2 = await Folder.CreateFileAsync("2.log", CreationCollisionOption.ReplaceExisting);
					}
					else
					{
						CurrentFile = File1 = await Folder.CreateFileAsync("1.log", CreationCollisionOption.ReplaceExisting);
					}
					stream = await CurrentFile.OpenStreamForWriteAsync();
					stream.Seek(0, SeekOrigin.End);
				}
			}
		}

		/// <summary>
		/// Gets/sets the current logging level.
		/// </summary>
		public static LogLevel Level
		{
			get { return level; }
			set
			{
				if (value != level)
				{
					level = value;
				}
			}
		}

		/// <summary>
		/// Gets/sets the maximum file size.
		/// </summary>
		public static ulong MaxFileSize
		{
			get { return maxFileSize; }
			set
			{
				ulong newMaxFileSize = Math.Max(value, MIN_FILE_SIZE);
				if (newMaxFileSize != maxFileSize)
				{
					maxFileSize = newMaxFileSize;
				}
			}
		}

		/// <summary>
		/// Deletes the contents of the log files.
		/// </summary>
		public static void Clear()
		{
			lock (messages)
			{
				messages.Clear();
			}
			clear = true;
			signal.Set();
		}

		/// <summary>
		/// Writes a message to the current log file.
		/// </summary>
		/// <param name="level">Logging level for the message to be written.</param>
		/// <param name="format">Format string for the message.</param>
		/// <param name="args">Arguments for the format string.</param>
		public static void Write(LogLevel level, string format, params object[] args)
		{
			string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
			string lvl = level.ToString();
			string message = date + "|" + lvl + "|" + string.Format(format, args);
			lock (messages)
			{
				messages.Add(message);
			}
			signal.Set();
		}

		/// <summary>
		/// Writes a verbose message to the current log file.
		/// </summary>
		/// <param name="format">Format string for the message.</param>
		/// <param name="args">Arguments for the format string.</param>
		public static void Verbose(string format, params object[] args)
		{
			Write(LogLevel.Verbose, format, args);
		}

		/// <summary>
		/// Writes a information message to the current log file.
		/// </summary>
		/// <param name="format">Format string for the message.</param>
		/// <param name="args">Arguments for the format string.</param>
		public static void Info(string format, params object[] args)
		{
			Write(LogLevel.Info, format, args);
		}

		/// <summary>
		/// Writes a warning message to the current log file.
		/// </summary>
		/// <param name="format">Format string for the message.</param>
		/// <param name="args">Arguments for the format string.</param>
		public static void Warning(string format, params object[] args)
		{
			Write(LogLevel.Warning, format, args);
		}

		/// <summary>
		/// Writes an error message to the current log file.
		/// </summary>
		/// <param name="format">Format string for the message.</param>
		/// <param name="args">Arguments for the format string.</param>
		public static void Error(string format, params object[] args)
		{
			Write(LogLevel.Error, format, args);
		}

		/// <summary>
		/// Writes a critical message to the current log file.
		/// </summary>
		/// <param name="format">Format string for the message.</param>
		/// <param name="args">Arguments for the format string.</param>
		public static void Critical(string format, params object[] args)
		{
			Write(LogLevel.Critical, format, args);
		}
	}

	public enum LogLevel
	{
		Critical,
		Error,
		Warning,
		Info,
		Verbose
	}
}
