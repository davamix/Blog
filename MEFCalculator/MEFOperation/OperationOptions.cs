using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace MEFOperation
{
	[MetadataAttribute]
	public class OperationOptions : Attribute
	{
		public int NumVersion { get; set; }
		public string Name { get; set; }
	}
}
