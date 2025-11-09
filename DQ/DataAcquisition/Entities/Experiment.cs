using DataAcquisition.Settings;

namespace DataAcquisition.Entities
{
	/// <summary>
	/// Class <c>Experiment</c> represents an experiment's details.
	/// </summary>
	public class Experiment
	{
		public int Id { get; set; }
		public int CircuitId { get; set; }
		public string Title { get; set; }
		public string ImagePath { get; set; }
		public string Procedure { get; set; }

		public bool[] InputEnabled = new bool[16] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
		public double[] InputMin = new double[16] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
		public double[] InputMax = new double[16] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

		public string[] ErrorMessages = new string[16] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
	}
}
