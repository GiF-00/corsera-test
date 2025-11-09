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
	public class ExperimentsDb
	{
		private readonly ILogger logger;

		private readonly ExperimentErrorsDb errorsDb;

		public ExperimentsDb(ILogger logger)
        {
			this.logger = logger;

			SeedDatabase();

			errorsDb = new ExperimentErrorsDb(this.logger);
		}

		// ----------------------------------------------------------------------------------------------------------
		// - 
		// - Interfaces - Password
		// - 
		// ----------------------------------------------------------------------------------------------------------

		public string ReadPassword()
		{
			string password = "";

			ExecuteQueryWithRowDataProcessor(
				$@"SELECT PWD FROM PWDs WHERE UN = '{Constants.DefaultUsername}';",
				reader =>
				{
					if (password == "")
					{
						password = reader.GetString(0);
						return true;
					}

					return false;
				});

			return password;
		}

		public void UpdatePassword(string newPassword)
		{
			ExecuteQuery($@"UPDATE PWDs SET PWD = '{newPassword}' WHERE UN = '{Constants.DefaultUsername}';");
		}

		// ----------------------------------------------------------------------------------------------------------
		// - 
		// - Interfaces - PCBs
		// - 
		// ----------------------------------------------------------------------------------------------------------

		public void CreatePCB(PCB pcb)
		{
			ExecuteQuery(
				$@"INSERT INTO PCBs (Title) VALUES ($Title);",
				new Dictionary<string, object>()
				{
					{ "$Title", pcb.Title }
				});
		}

		public PCB ReadPCB(int id)
		{
			PCB pcb = null!;

			ExecuteQueryWithRowDataProcessor(
				$@"SELECT Id,Title FROM PCBs;",
				(reader) =>
				{
					int index = 0;

					pcb = new PCB()
					{
						Id = reader.GetInt32(index++),
						Title = reader.GetString(index++),
					};

					return true;
				});

			if (pcb != null)
			{
				pcb.Circuits = ReadCircuitsForPCB(pcb.Id);
			}

			return pcb!;
		}

		public List<PCB> ReadAllPCBs()
		{
			List<PCB> allPCBs = new List<PCB>();

			ExecuteQueryWithRowDataProcessor(
				$@"SELECT Id,Title FROM PCBs;",
				(reader) =>
				{
					int index = 0;

					allPCBs.Add(new PCB()
					{
						Id = reader.GetInt32(index++),
						Title = reader.GetString(index++),
					});

					return false;
				});

			foreach (PCB pcb in allPCBs)
			{
				pcb.Circuits = ReadCircuitsForPCB(pcb.Id);
			}

			return allPCBs;
		}

		public void UpdatePCB(PCB pcb)
		{
			ExecuteQuery(
				$@"UPDATE PCBs SET Title = $Title WHERE Id = $Id;",
				new Dictionary<string, object>
				{
					{ "$Title", pcb.Title },
					{ "$Id", pcb.Id }
				});
		}

		public void DeletePCB(int id)
		{
			DeleteCircuitsForPCB(id);

			ExecuteQuery(
				$@"DELETE FROM PCBs WHERE Id = $Id;",
				new Dictionary<string, object>() { { "$Id", id } });
		}

		// ----------------------------------------------------------------------------------------------------------
		// - 
		// - Interfaces - Circuits
		// - 
		// ----------------------------------------------------------------------------------------------------------

		public void CreateCircuit(Circuit circuit)
		{
			ExecuteQuery(
				$@"INSERT INTO Circuits (pcb_id,Title) VALUES ($pcb_id,$Title);",
				new Dictionary<string, object>()
				{
					{ "$pcb_id", circuit.PcbId },
					{ "$Title", circuit.Title }
				});
		}

		public Circuit ReadCircuit(int id)
		{
			Circuit circuit = null!;

			ExecuteQueryWithRowDataProcessor(
				$@"SELECT Id,pcb_id,Title FROM Circuits WHERE Id = $Id;",
				new Dictionary<string, object>()
				{
					{ "$Id", id }
				},
				(reader) =>
				{
					int index = 0;

					circuit = new Circuit()
					{
						Id = reader.GetInt32(index++),
						PcbId = reader.GetInt32(index++),
						Title = reader.GetString(index++),
					};

					return true;
				});

			if (circuit != null)
			{
				circuit.Experiments = ReadExperimentSummariesForCircuit(circuit.Id);
			}

			return circuit!;
		}

		public List<Circuit> ReadCircuitsForPCB(int pcbId)
		{
			List<Circuit> circuits = new List<Circuit>();

			ExecuteQueryWithRowDataProcessor(
				$@"SELECT Id,pcb_id,Title FROM Circuits WHERE pcb_id = $pcb_id;",
				new Dictionary<string, object>()
				{
					{ "$pcb_id", pcbId }
				},
				(reader) =>
				{
					int index = 0;

					circuits.Add(new Circuit()
					{
						Id = reader.GetInt32(index++),
						PcbId = reader.GetInt32(index++),
						Title = reader.GetString(index++),
					});

					return false;
				});

			foreach (Circuit cct in circuits)
			{
				cct.Experiments = ReadExperimentSummariesForCircuit(cct.Id);
			}

			return circuits;
		}

		public void UpdateCircuit(Circuit circuit)
		{
			ExecuteQuery(
				$@"UPDATE Circuits SET pcb_id = $pcb_id, Title = $Title WHERE Id = $Id;",
				new Dictionary<string, object>
				{
					{ "$pcb_id", circuit.PcbId },
					{ "$Title", circuit.Title },
					{ "$Id", circuit.Id }
				});
		}

		public void DeleteCircuit(int id)
		{
			DeleteExperimentsForCircuit(id);

			ExecuteQuery(
				$@"DELETE FROM Circuits WHERE Id = $Id;",
				new Dictionary<string, object>() { { "$Id", id } });
		}

		public void DeleteCircuitsForPCB(int pcbId)
		{
			foreach (Circuit circuit in ReadCircuitsForPCB(pcbId))
			{
				DeleteCircuit(circuit.Id);
			}
		}

		// ----------------------------------------------------------------------------------------------------------
		// - 
		// - Interfaces - Experiments
		// - 
		// ----------------------------------------------------------------------------------------------------------

		public void CreateExperiment(Experiment experiment)
		{
			UpdateImagePath(ref experiment);

			Dictionary<string, object> parameters = new Dictionary<string, object>()
			{
				{ "$cct_id", experiment.CircuitId },
				{ "$Title", experiment.Title },
				{ "$ImagePath", experiment.ImagePath },
				{ "$Procedure", experiment.Procedure }
			};

			for (int i = 0; i < experiment.InputEnabled.Length; i++)
			{
				parameters[$"$InputEnabledA{i + 1}"] = experiment.InputEnabled[i];
				parameters[$"$InputMinA{i + 1}"] = experiment.InputMin[i];
				parameters[$"$InputMaxA{i + 1}"] = experiment.InputMax[i];
			}

			int idAssigned = ExecuteQueryAndGetId($@"
INSERT INTO Experiments (
 cct_id, Title, ImagePath, Procedure,
 InputEnabledA1,   InputEnabledA2,   InputEnabledA3,   InputEnabledA4,
 InputEnabledA5,   InputEnabledA6,   InputEnabledA7,   InputEnabledA8,
 InputEnabledA9,   InputEnabledA10,  InputEnabledA11,  InputEnabledA12,
 InputEnabledA13,  InputEnabledA14,  InputEnabledA15,  InputEnabledA16,
 InputMinA1,   InputMinA2,   InputMinA3,   InputMinA4,
 InputMinA5,   InputMinA6,   InputMinA7,   InputMinA8,
 InputMinA9,   InputMinA10,  InputMinA11,  InputMinA12,
 InputMinA13,  InputMinA14,  InputMinA15,  InputMinA16,
 InputMaxA1,   InputMaxA2,   InputMaxA3,   InputMaxA4,
 InputMaxA5,   InputMaxA6,   InputMaxA7,   InputMaxA8,
 InputMaxA9,   InputMaxA10,  InputMaxA11,  InputMaxA12,
 InputMaxA13,  InputMaxA14,  InputMaxA15,  InputMaxA16) 
VALUES (
 $cct_id, $Title, $ImagePath, $Procedure,
 $InputEnabledA1,   $InputEnabledA2,   $InputEnabledA3,   $InputEnabledA4,
 $InputEnabledA5,   $InputEnabledA6,   $InputEnabledA7,   $InputEnabledA8,
 $InputEnabledA9,   $InputEnabledA10,  $InputEnabledA11,  $InputEnabledA12,
 $InputEnabledA13,  $InputEnabledA14,  $InputEnabledA15,  $InputEnabledA16,
 $InputMinA1,   $InputMinA2,   $InputMinA3,   $InputMinA4,
 $InputMinA5,   $InputMinA6,   $InputMinA7,   $InputMinA8,
 $InputMinA9,   $InputMinA10,  $InputMinA11,  $InputMinA12,
 $InputMinA13,  $InputMinA14,  $InputMinA15,  $InputMinA16,
 $InputMaxA1,   $InputMaxA2,   $InputMaxA3,   $InputMaxA4,
 $InputMaxA5,   $InputMaxA6,   $InputMaxA7,   $InputMaxA8,
 $InputMaxA9,   $InputMaxA10,  $InputMaxA11,  $InputMaxA12,
 $InputMaxA13,  $InputMaxA14,  $InputMaxA15,  $InputMaxA16);
  select last_insert_rowid();", parameters);

			if (idAssigned != Constants.UnassignedId)
			{
				errorsDb.CreateOrUpdateErrorMessages(idAssigned, experiment.ErrorMessages);
			}
			else
			{
				throw new Exception("Could not get the id of experiment.");
			}
		}

		public Experiment ReadExperiment(int id)
		{
			Experiment experiment = null!;

			ExecuteQueryWithRowDataProcessor(
				$@"
SELECT Id, cct_id, Title, ImagePath, Procedure, 
  InputEnabledA1  ,   InputEnabledA2  ,   InputEnabledA3  ,   InputEnabledA4  ,
  InputEnabledA5  ,   InputEnabledA6  ,   InputEnabledA7  ,   InputEnabledA8  ,
  InputEnabledA9  ,   InputEnabledA10 ,   InputEnabledA11 ,   InputEnabledA12 ,
  InputEnabledA13 ,   InputEnabledA14 ,   InputEnabledA15 ,   InputEnabledA16 ,
  InputMinA1  ,   InputMinA2  ,   InputMinA3  ,   InputMinA4  ,
  InputMinA5  ,   InputMinA6  ,   InputMinA7  ,   InputMinA8  ,
  InputMinA9  ,   InputMinA10 ,   InputMinA11 ,   InputMinA12 ,
  InputMinA13 ,   InputMinA14 ,   InputMinA15 ,   InputMinA16 ,
  InputMaxA1  ,   InputMaxA2  ,   InputMaxA3  ,   InputMaxA4  ,
  InputMaxA5  ,   InputMaxA6  ,   InputMaxA7  ,   InputMaxA8  ,
  InputMaxA9  ,   InputMaxA10 ,   InputMaxA11 ,   InputMaxA12 ,
  InputMaxA13 ,   InputMaxA14 ,   InputMaxA15 ,   InputMaxA16 FROM Experiments WHERE Id = $ID;",
				new Dictionary<string, object>() { { "$ID", id } },
				(reader) =>
				{
					experiment = new Experiment();

					int index = 0;

					experiment.Id = reader.GetInt32(index++);
					experiment.CircuitId = reader.GetInt32(index++);

					experiment.Title = reader.GetString(index++);
					experiment.ImagePath = reader.GetString(index++);
					experiment.Procedure = reader.GetString(index++);

					for (int i = 0; i < experiment.InputEnabled.Length; i++)
					{
						experiment.InputEnabled[i] = reader.GetBoolean(index++);
					}

					for (int i = 0; i < experiment.InputMin.Length; i++)
					{
						experiment.InputMin[i] = reader.GetDouble(index++);
					}

					for (int i = 0; i < experiment.InputMax.Length; i++)
					{
						experiment.InputMax[i] = reader.GetDouble(index++);
					}

					return true; // Okay now, we have had enough data
				});

			if (experiment != null)
			{
				experiment.ErrorMessages = errorsDb.ReadErrorMessages(id);
			}

			return experiment!;
		}

		public List<Experiment> ReadExperimentSummariesForCircuit(int circuitId)
		{
			List<Experiment> experiments = new List<Experiment>();

			ExecuteQueryWithRowDataProcessor(
				$@"SELECT Id, cct_id, Title FROM Experiments WHERE cct_id = $circuitId;",
				new Dictionary<string, object>() { { "$circuitId", circuitId } },
				(reader) =>
				{
					int index = 0;

					experiments.Add(new Experiment()
					{
						Id = reader.GetInt32(index++),
						CircuitId = reader.GetInt32(index++),
						Title = reader.GetString(index++)
					});

					return false; // keep going - we need more data
				});

			return experiments;
		}

		public void UpdateExperiment(Experiment experiment)
		{
			UpdateImagePath(ref experiment);

			Dictionary<string, object> parameters = new Dictionary<string, object>()
			{
				{ "$ID", experiment.Id },
				{ "$cct_id", experiment.CircuitId },
				{ "$Title", experiment.Title },
				{ "$ImagePath", experiment.ImagePath },
				{ "$Procedure", experiment.Procedure }
			};

			for (int i = 0; i < experiment.InputEnabled.Length; i++)
			{
				parameters[$"$InputEnabledA{i + 1}"] = experiment.InputEnabled[i];
				parameters[$"$InputMinA{i + 1}"] = experiment.InputMin[i];
				parameters[$"$InputMaxA{i + 1}"] = experiment.InputMax[i];
			}

			ExecuteQuery(
				$@"UPDATE Experiments SET 
 cct_id = $cct_id, Title = $Title, ImagePath = $ImagePath, Procedure = $Procedure,
 InputEnabledA1  = $InputEnabledA1,   InputEnabledA2  = $InputEnabledA2,   InputEnabledA3  = $InputEnabledA3,   InputEnabledA4  = $InputEnabledA4,
 InputEnabledA5  = $InputEnabledA5,   InputEnabledA6  = $InputEnabledA6,   InputEnabledA7  = $InputEnabledA7,   InputEnabledA8  = $InputEnabledA8,
 InputEnabledA9  = $InputEnabledA9,   InputEnabledA10 = $InputEnabledA10,  InputEnabledA11 = $InputEnabledA11,  InputEnabledA12 = $InputEnabledA12,
 InputEnabledA13 = $InputEnabledA13,  InputEnabledA14 = $InputEnabledA14,  InputEnabledA15 = $InputEnabledA15,  InputEnabledA16 = $InputEnabledA16,
 InputMinA1  = $InputMinA1,   InputMinA2  = $InputMinA2,   InputMinA3  = $InputMinA3,   InputMinA4  = $InputMinA4,
 InputMinA5  = $InputMinA5,   InputMinA6  = $InputMinA6,   InputMinA7  = $InputMinA7,   InputMinA8  = $InputMinA8,
 InputMinA9  = $InputMinA9,   InputMinA10 = $InputMinA10,  InputMinA11 = $InputMinA11,  InputMinA12 = $InputMinA12,
 InputMinA13 = $InputMinA13,  InputMinA14 = $InputMinA14,  InputMinA15 = $InputMinA15,  InputMinA16 = $InputMinA16,
 InputMaxA1  = $InputMaxA1,   InputMaxA2  = $InputMaxA2,   InputMaxA3  = $InputMaxA3,   InputMaxA4  = $InputMaxA4,
 InputMaxA5  = $InputMaxA5,   InputMaxA6  = $InputMaxA6,   InputMaxA7  = $InputMaxA7,   InputMaxA8  = $InputMaxA8,
 InputMaxA9  = $InputMaxA9,   InputMaxA10 = $InputMaxA10,  InputMaxA11 = $InputMaxA11,  InputMaxA12 = $InputMaxA12,
 InputMaxA13 = $InputMaxA13,  InputMaxA14 = $InputMaxA14,  InputMaxA15 = $InputMaxA15,  InputMaxA16 = $InputMaxA16 
 WHERE Id = $ID;", parameters);

			errorsDb.CreateOrUpdateErrorMessages(experiment.Id, experiment.ErrorMessages);
		}

		private void UpdateImagePath(ref Experiment experiment)
		{
			try
			{
				if (experiment.ImagePath.Contains(Constants.ExperimentImagesFolder) == false)
				{
					string updatedPath = experiment.ImagePath.UpdateFilenameWhenMovedToFolder(Constants.ExperimentImagesFolder);

					updatedPath.CreateFoldersForRelativeFilename();

					File.Copy(experiment.ImagePath, updatedPath, true);
					experiment.ImagePath = updatedPath;
				}
			}
			catch (Exception ex)
			{
				logger?.Error(
					$"Error occurred while copying image file for experiment {experiment.Title}",
					ex.Message);
			}
		}

		public void DeleteExperiment(int id)
		{
			ExecuteQuery(
				$@"DELETE FROM Experiments WHERE Id = $ID;",
				new Dictionary<string, object>() { { "$ID", id } });

			errorsDb.DeleteErrorMessages(id);
		}

		public void DeleteExperimentsForCircuit(int circuitId)
		{
			ExecuteQuery(
				$@"DELETE FROM Experiments WHERE cct_id = $cct_id;",
				new Dictionary<string, object>() { { "$cct_id", circuitId } });

			//- 
			//- We have an overflow here, as error messages are not being cleared
			//- 
		}

		// ----------------------------------------------------------------------------------------------------------
		// - 
		// - Internal Helpers
		// - 
		// ----------------------------------------------------------------------------------------------------------

		private void SeedDatabase()
		{
			//- Passwords
			ExecuteQuery(@"
CREATE TABLE IF NOT EXISTS PWDs (
  Id INTEGER PRIMARY KEY ASC AUTOINCREMENT,
  UN  TEXT UNIQUE NOT NULL,
  PWD TEXT NOT NULL);");

			//- PCBs
			ExecuteQuery(@"
CREATE TABLE IF NOT EXISTS PCBs (
  Id INTEGER PRIMARY KEY ASC AUTOINCREMENT,
  Title TEXT NOT NULL);");

			//- Circuits
			ExecuteQuery(@"
CREATE TABLE IF NOT EXISTS Circuits (
  Id INTEGER PRIMARY KEY ASC AUTOINCREMENT,
  pcb_id INTEGER REFERENCES PCBs(Id),
  Title TEXT NOT NULL);");

			//- Experiments
			ExecuteQuery(@"
CREATE TABLE IF NOT EXISTS Experiments (
  Id INTEGER PRIMARY KEY ASC AUTOINCREMENT,
  cct_id INTEGER REFERENCES Circuits(Id),
  Title TEXT NOT NULL,
  ImagePath TEXT NOT NULL,
  Procedure TEXT,
  InputEnabledA1  INTEGER,  InputEnabledA2  INTEGER,  InputEnabledA3  INTEGER,  InputEnabledA4  INTEGER,
  InputEnabledA5  INTEGER,  InputEnabledA6  INTEGER,  InputEnabledA7  INTEGER,  InputEnabledA8  INTEGER,
  InputEnabledA9  INTEGER,  InputEnabledA10 INTEGER,  InputEnabledA11 INTEGER,  InputEnabledA12 INTEGER,
  InputEnabledA13 INTEGER,  InputEnabledA14 INTEGER,  InputEnabledA15 INTEGER,  InputEnabledA16 INTEGER,
  InputMinA1  REAL,  InputMinA2  REAL,  InputMinA3  REAL,  InputMinA4  REAL,
  InputMinA5  REAL,  InputMinA6  REAL,  InputMinA7  REAL,  InputMinA8  REAL,
  InputMinA9  REAL,  InputMinA10 REAL,  InputMinA11 REAL,  InputMinA12 REAL,
  InputMinA13 REAL,  InputMinA14 REAL,  InputMinA15 REAL,  InputMinA16 REAL,
  InputMaxA1  REAL,  InputMaxA2  REAL,  InputMaxA3  REAL,  InputMaxA4  REAL,
  InputMaxA5  REAL,  InputMaxA6  REAL,  InputMaxA7  REAL,  InputMaxA8  REAL,
  InputMaxA9  REAL,  InputMaxA10 REAL,  InputMaxA11 REAL,  InputMaxA12 REAL,
  InputMaxA13 REAL,  InputMaxA14 REAL,  InputMaxA15 REAL,  InputMaxA16 REAL);");

			//- Insert password
			ExecuteQueryIfNoDataInTable("PWDs",
				$@"INSERT INTO PWDs (UN, PWD) VALUES ('{Constants.DefaultUsername}', '{Constants.DefaultPassword.Hash()}');");
		}

		/// <summary>
		/// Executes a given query without getting any results.
		/// </summary>
		/// <param name="query">The query.</param>
		private void ExecuteQuery(string query)
		{
			try
			{
				using (var connection = new SqliteConnection(Constants.DbConnectionString))
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
		private void ExecuteQuery(string query, Dictionary<string,object> parameters)
		{
			try
			{
				using (var connection = new SqliteConnection(Constants.DbConnectionString))
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
		/// Executes a given query and gets assigned id.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="parameters">Parameters for @name - value.</param>
		private int ExecuteQueryAndGetId(string query, Dictionary<string, object> parameters)
		{
			try
			{
				using (var connection = new SqliteConnection(Constants.DbConnectionString))
				{
					connection.Open();
					SqliteCommand command = connection.CreateCommand();
					command.CommandText = query;

					foreach (var parameter in parameters)
					{
						command.Parameters.AddWithValue(parameter.Key, parameter.Value);
					}

					object? r = command.ExecuteScalar();

					return r == null ? Constants.UnassignedId : Convert.ToInt32(r);
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error occurred in ExecuteQuery", ex.Message);
			}

			return Constants.UnassignedId;
		}

		/// <summary>
		/// Executes query and passes data reader to given function.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="rowDataProcessor">Processing function - returns true when no more rows are required.</param>
		private void ExecuteQueryWithRowDataProcessor(string query, Func<SqliteDataReader,bool> rowDataProcessor)
		{
			try
			{
				using (var connection = new SqliteConnection(Constants.DbConnectionString))
				{
					connection.Open();
					SqliteCommand command = connection.CreateCommand();
					command.CommandText = query;

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
				using (var connection = new SqliteConnection(Constants.DbConnectionString))
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

		/// <summary>
		/// Executes the given query when there is no data available in given table.
		/// </summary>
		/// <param name="table">The table that needs to be checked for data.</param>
		/// <param name="query">The query.</param>
		private void ExecuteQueryIfNoDataInTable(string table, string query)
		{
			try
			{
				using (var connection = new SqliteConnection(Constants.DbConnectionString))
				{
					connection.Open();

					SqliteCommand command = connection.CreateCommand();
					
					command.CommandText = $"SELECT * from {table};";
					
					SqliteDataReader tableCheckReader = command.ExecuteReader();

					if (tableCheckReader.HasRows == false)
					{
						tableCheckReader.Close();

						command.CommandText = query;

						command.ExecuteNonQuery();
					}
					else
					{
						tableCheckReader.Close();
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error occurred in ExecuteQueryIfNoDataInTable", ex.Message);
			}
		}
		/// <summary>
		/// Executes the given query when there is no data available in given table.
		/// </summary>
		/// <param name="table">The table that needs to be checked for data.</param>
		/// <param name="query">The query.</param>
		/// <param name="parameters">Parameters for @name - value.</param>
		private void ExecuteQueryIfNoDataInTable(string table, string query, Dictionary<string, object> parameters)
		{
			try
			{
				using (var connection = new SqliteConnection(Constants.DbConnectionString))
				{
					connection.Open();

					SqliteCommand tableCheckCommand = connection.CreateCommand();

					tableCheckCommand.CommandText = $"SELECT * from {table};";

					SqliteDataReader tableCheckReader = tableCheckCommand.ExecuteReader();

					if (tableCheckReader.HasRows == false)
					{
						SqliteCommand command = connection.CreateCommand();

						command.CommandText = query;

						foreach (var parameter in parameters)
						{
							command.Parameters.AddWithValue(parameter.Key, parameter.Value);
						}

						tableCheckCommand.ExecuteNonQuery();
					}

					tableCheckReader.Close();
				}
			}
			catch (Exception ex)
			{
				logger.Error("Error occurred in ExecuteQueryIfNoDataInTable", ex.Message);
			}
		}
	}
}
