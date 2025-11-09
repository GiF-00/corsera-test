using DataAcquisition.Extensions;
using System;
using System.IO;

namespace DataAcquisition.Logger.DefaultLogger
{
	/// <summary>
	/// Class <c>FileLogger</c> implements <c>ILogger</c> interface to write log messages to a file.
	/// </summary>
	public class FileLogger : ILogger
	{
		private readonly LogLevel _logLevel;
		private readonly bool _timestamp;
		private StreamWriter? _writer;

		/// <summary>
		/// Initializes the class with given options.
		/// </summary>
		/// <param name="filename">Name of the file to which the log messages should be written.</param>
		/// <param name="logLevel">The minimum level of log messages that should be written to the file.</param>
		/// <param name="timestamp">
		///     True = enable printing of timestamps with log messages,
		///     False = disable timestamps.</param>
		/// <exception cref="Exception"></exception>
		public FileLogger(string filename, LogLevel logLevel = LogLevel.Information, bool timestamp = true)
		{
			try
			{
				filename.CreateFoldersForRelativeFilename();

				_writer = File.AppendText(filename);

				_writer.WriteLine($"Data Acquisition");
				_writer.WriteLine($"================");
				_writer.WriteLine($"Starting @ {DateTime.Now:yyyy/MMM/dd - hh:mm:ss tt}");
				_writer.WriteLine($"...");
				_writer.WriteLine($"");

				_writer.Flush();
				_writer.AutoFlush = true;
			}
			catch (Exception ex)
			{
				_writer = null;
				throw new Exception($"Cannot write logfile \"{filename}\" - {ex.Message}");
			}

			_logLevel = logLevel;
			_timestamp = timestamp;
		}

		public void Dispose()
		{
			if (_writer != null)
			{
				_writer.WriteLine($"");
				_writer.WriteLine($"...");
				_writer.WriteLine($"Stopping @ {DateTime.Now:yyyy/MMM/dd - hh:mm:ss tt}");
				_writer.WriteLine($"-------------------------------------------------------");
				_writer.WriteLine($"");

				_writer.Flush();
				_writer.Dispose();
				_writer = null;
			}
		}

		public void Info(params string[] messages)
		{
			if (_logLevel == LogLevel.Information && messages.Length > 0)
			{
				lock (this)
				{
					if (_timestamp)
					{
						_writer?.WriteLine($"{DateTime.Now:hh:mm:ss.fff} {messages[0]}");

						for (int i = 1; i < messages.Length; i++)
						{
							_writer?.WriteLine($"    {messages[i]}");
						}
					}
					else
					{
						_writer?.WriteLine(messages[0]);

						for (int i = 1; i < messages.Length; i++)
						{
							_writer?.WriteLine($"    {messages[i]}");
						}
					}
				}
			}
		}

		public void Warning(params string[] messages)
		{
			if (_logLevel != LogLevel.Error && messages.Length > 0)
			{
				lock (this)
				{
					if (_timestamp)
					{
						_writer?.WriteLine($"{DateTime.Now:hh:mm:ss.fff} Warning: {messages[0]}");

						for (int i = 1; i < messages.Length; i++)
						{
							_writer?.WriteLine($"    {messages[i]}");
						}
					}
					else
					{
						_writer?.WriteLine($"Warning: {messages[0]}");

						for (int i = 1; i < messages.Length; i++)
						{
							_writer?.WriteLine($"    {messages[i]}");
						}
					}
				}
			}
		}

		public void Error(params string[] messages)
		{
			if (messages.Length > 0)
			{
				lock (this)
				{
					if (_timestamp)
					{
						_writer?.WriteLine($"{DateTime.Now:hh:mm:ss.fff} Error: {messages[0]}");

						for (int i = 1; i < messages.Length; i++)
						{
							_writer?.WriteLine($"    {messages[i]}");
						}
					}
					else
					{
						_writer?.WriteLine($"Error: {messages[0]}");

						for (int i = 1; i < messages.Length; i++)
						{
							_writer?.WriteLine($"    {messages[i]}");
						}
					}
				}
			}
		}
	}
}
