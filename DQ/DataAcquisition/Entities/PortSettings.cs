using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcquisition.Entities
{
    /// <summary>
    /// Class <c>PortSettings</c> represents overall port settings.
    /// Including the COM port and the A1-A16 registers.
    /// </summary>
    public class PortSettings
	{
        public string ComPort { get; set; } = "";
        public int DeviceAddress { get; set; } = 1;

        public BaudRate BaudRate { get; set; } = BaudRate._115200;
        public DataBits DataBits { get; set; } = DataBits._8;
        public Parity Parity { get; set; } = Parity.None;
        public StopBits StopBits { get; set; } = StopBits.One;

        public string RegistersStartAddress { get; set; } = "0x0020";

        public double[] MultiplicationFactors = new double[16] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
        public double[] Offsets = new double[16]               { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
		public string[] DisplayUnits = new string[16] { "V", "V", "V", "V", "V", "V", "V", "V", "V", "V", "V", "V", "V", "V", "V", "V" };
	}
}
