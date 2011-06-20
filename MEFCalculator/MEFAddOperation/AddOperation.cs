using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using MEFOperation;

namespace MEFAddOperation
{
	[Export(typeof(MEFOperation.Operation))]
	[OperationOptions(NumVersion = 1, Name = "Sumar")]
	public class AddOperation : Operation
	{
		public override double Calculate(double num1, double num2)
		{
			return num1 + num2;
		}
	}
}
