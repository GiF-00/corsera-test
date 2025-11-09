using System.Collections.Generic;

namespace DataAcquisition.Entities
{
	/// <summary>
	/// Class <c>PCB</c> represents PCB.
	/// </summary>
	public class PCB
	{
		public int Id { get; set; }
		public string Title { get; set; } = "";
		public List<Circuit> Circuits { get; set; } = new List<Circuit>();
	}
}
