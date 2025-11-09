using DataAcquisition.Entities;
using DataAcquisition.Extensions;
using DataAcquisition.Logger;
using DataAcquisition.Settings;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataAcquisition.Services
{
	public class ExperimentErrorsDb
	{
		private readonly ILogger logger;

		public ExperimentErrorsDb(ILogger logger)
        {
			this.logger = logger;
			SeedDatabase();
		}

		public void CreateOrUpdateErrorMessages(int id, string[] messages)
		{
			if (messages == null || messages.Length < 16)
				throw new Exception($"Invalid number of error messages provided against ID {id}");

			bool isExisting = false;

			//- Check query
			ExecuteQueryWithRowDataProcessor(
				$@"SELECT Id FROM ErrorMessages WHERE Id = $ID;",
				new Dictionary<string, object>() { { "$ID", id } },
				(reader) =>
				{
					isExisting = true;

					return true; // Okay now, we have had enough data
				});

			Dictionary<string, object> parameters = new Dictionary<string, object>()
			{
				{ "$ID", id }
			};

			for (int i = 0; i < messages.Length; i++)
			{
				parameters[$"$ErrorMessageA{i + 1}"] = messages[i];
			}

			if (isExisting == false)
			{
				ExecuteQuery($@"
INSERT INTO ErrorMessages (
 Id, 
 ErrorMessageA1  , ErrorMessageA2  , ErrorMessageA3  , ErrorMessageA4  ,
 ErrorMessageA5  , ErrorMessageA6  , ErrorMessageA7  , ErrorMessageA8  ,
 ErrorMessageA9  , ErrorMessageA10 , ErrorMessageA11 , ErrorMessageA12 ,
 ErrorMessageA13 , ErrorMessageA14 , ErrorMessageA15 , ErrorMessageA16) 
VALUES (
 $ID, 
 $ErrorMessageA1  , $ErrorMessageA2  , $ErrorMessageA3  , $ErrorMessageA4  ,
 $ErrorMessageA5  , $ErrorMessageA6  , $ErrorMessageA7  , $ErrorMessageA8  ,
 $ErrorMessageA9  , $ErrorMessageA10 , $ErrorMessageA11 , $ErrorMessageA12 ,
 $ErrorMessageA13 , $ErrorMessageA14 , $ErrorMessageA15 , $ErrorMessageA16);", parameters);
			}
			else
			{
				ExecuteQuery(
				$@"UPDATE ErrorMessages SET 
 ErrorMessageA1  = $ErrorMessageA1  , ErrorMessageA2  = $ErrorMessageA2  , ErrorMessageA3  = $ErrorMessageA3  , ErrorMessageA4  = $ErrorMessageA4  ,
 ErrorMessageA5  = $ErrorMessageA5  , ErrorMessageA6  = $ErrorMessageA6  , ErrorMessageA7  = $ErrorMessageA7  , ErrorMessageA8  = $ErrorMessageA8  ,
 ErrorMessageA9  = $ErrorMessageA9  , ErrorMessageA10 = $ErrorMessageA10 , ErrorMessageA11 = $ErrorMessageA11 , ErrorMessageA12 = $ErrorMessageA12 ,
 ErrorMessageA13 = $ErrorMessageA13 , ErrorMessageA14 = $ErrorMessageA14 , ErrorMessageA15 = $ErrorMessageA15 , ErrorMessageA16 = $ErrorMessageA16
 WHERE Id = $ID;", parameters);
			}
		}

		public string[] ReadErrorMessages(int id)
		{
			string[] errorMessages = new string[16] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };

			ExecuteQueryWithRowDataProcessor(
				$@"SELECT 
  ErrorMessageA1  , ErrorMessageA2  , ErrorMessageA3  , ErrorMessageA4  ,
  ErrorMessageA5  , ErrorMessageA6  , ErrorMessageA7  , ErrorMessageA8  ,
  ErrorMessageA9  , ErrorMessageA10 , ErrorMessageA11 , ErrorMessageA12 ,
  ErrorMessageA13 , ErrorMessageA14 , ErrorMessageA15 , ErrorMessageA16  FROM ErrorMessages WHERE Id = $ID;",
				new Dictionary<string, object>() { { "$ID", id } },
				(reader) =>
				{
					for (int i = 0; i < errorMessages.Length; i++)
					{
						errorMessages[i] = reader.GetString(i);
					}

					return true; // Okay now, we have had enough data
				});

			return errorMessages;
		}

		public void DeleteErrorMessages(int id)
		{
			ExecuteQuery(
				$@"DELETE FROM ErrorMessages WHERE Id = $ID;",
				new Dictionary<string, object>() { { "$ID", id } });
		}

		// ----------------------------------------------------------------------------------------------------------
		// - 
		// - Internal Helpers
		// - 
		// ----------------------------------------------------------------------------------------------------------

		private void SeedDatabase()
		{
			ExecuteQuery(@"
CREATE TABLE IF NOT EXISTS ErrorMessages (
  Id INTEGER PRIMARY KEY,
  ErrorMessageA1  TEXT,  ErrorMessageA2  TEXT,  ErrorMessageA3  TEXT,  ErrorMessageA4  TEXT,
  ErrorMessageA5  TEXT,  ErrorMessageA6  TEXT,  ErrorMessageA7  TEXT,  ErrorMessageA8  TEXT,
  ErrorMessageA9  TEXT,  ErrorMessageA10 TEXT,  ErrorMessageA11 TEXT,  ErrorMessageA12 TEXT,
  ErrorMessageA13 TEXT,  ErrorMessageA14 TEXT,  ErrorMessageA15 TEXT,  ErrorMessageA16 TEXT);");
		}

		/// <summary>
		/// Executes a given query without getting any results.
		/// </summary>
		/// <param name="query">The query.</param>
		private void ExecuteQuery(string query)
		{
			try
			{
				using (var connection = new SqliteConnection(Constants.DbErrorsConnectionString))
				{
					connection.Open();
					SqliteCommand command = connection.CreateCommand();
					command.CommandText = query;

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error occurred in ExecuteQuery", ex.Message);
			}
		}

		/// <summary>
		/// Executes a given query without getting any results.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="parameters">Parameters for @name - value.</param>
		private void ExecuteQuery(string query, Dictionary<string, object> parameters)
		{
			try
			{
				using (var connection = new SqliteConnection(Constants.DbErrorsConnectionString))
				{
					connection.Open();
					SqliteCommand command = connection.CreateCommand();
					command.CommandText = query;

					foreach (var parameter in parameters)
					{
						command.Parameters.AddWithValue(parameter.Key, parameter.Value);
					}

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error occurred in ExecuteQuery", ex.Message);
			}
		}

		/// <summary>
		/// Executes query and passes data reader to given function.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="parameters">Parameters for @name - value.</param>
		/// <param name="rowDataProcessor">Processing function - returns true when no more rows are required.</param>
		private void ExecuteQueryWithRowDataProcessor(string query, Dictionary<string, object> parameters, Func<SqliteDataReader, bool> rowDataProcessor)
		{
			try
			{
				using (var connection = new SqliteConnection(Constants.DbErrorsConnectionString))
				{
					connection.Open();
					SqliteCommand command = connection.CreateCommand();
					command.CommandText = query;

					foreach (var parameter in parameters)
					{
						command.Parameters.AddWithValue(parameter.Key, parameter.Value);
					}

					using (SqliteDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							if (rowDataProcessor(reader))
							{
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error occurred in ExecuteQueryWithRowDataProcessor", ex.Message);
			}
		}
	}
}
