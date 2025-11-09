using System.Collections.Generic;

namespace DataAcquisition.Entities
{
	/// <summary>
	/// Class <c>Circuit</c> represents a circuit board within a PCB.
	/// </summary>
	public class Circuit
	{
		public int Id { get; set; }
		public int PcbId { get; set; }
		public string Title { get; set; } = "";
		public List<Experiment> Experiments { get; set; } = new List<Experiment>();
	}
}
