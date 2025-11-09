using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcquisition.Entities
{
	/// <summary>
	/// Class <c>ApplicationSettings</c> represents overall application general settings.
	/// </summary>
	public class ApplicationSettings
    {
        public bool MainWindowIsFullScreen { get; set; }
        public double MainWindowX { get; set; }
		public double MainWindowY { get; set; }
		public double MainWindowWidth { get; set; }
		public double MainWindowHeight { get; set; }
    }
}
