using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace MEFOperation
{
	[Export(typeof(Operation))]

	public abstract class Operation
	{
		public abstract double Calculate(double num1, double num2);
	}
}
