using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectSample
{
	using Microsoft.Research.Kinect.Nui;

	/// <summary>
	/// Simula la estructura PlanarImage
	/// </summary>
	[Serializable]
	public class PlanarImageSerializable
	{
		public byte[] Bits { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int BytesPerPixel { get; set; }
	}
}
