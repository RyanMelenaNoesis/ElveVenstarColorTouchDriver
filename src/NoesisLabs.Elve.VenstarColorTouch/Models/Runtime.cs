using System;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public abstract class Runtime
	{
		public int Auxilary1Minutes { get; set; }
		public int Auxilary2Minutes { get; set; }
		public int Cool1Minutes { get; set; }
		public int Cool2Mintues { get; set; }
		public int Heat1Minutes { get; set; }
		public int Heat2Minutes { get; set; }
		public Int64 Timestamp { get; set; }
	}
}