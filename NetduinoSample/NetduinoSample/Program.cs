using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace NetduinoSample
{
	public class Program
	{
		static OutputPort _outDig0;
		static OutputPort _outDig1;
		static OutputPort _outDig2;

		static bool _isOn;
		static int _count;
		
		public static void Main()
		{
			_outDig0 = new OutputPort(Pins.GPIO_PIN_D0, false);
			_outDig1 = new OutputPort(Pins.GPIO_PIN_D1, false);
			_outDig2 = new OutputPort(Pins.GPIO_PIN_D2, false);
			
			_isOn = false;
			_count = 0;

			InterruptPort mySwicht = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
			mySwicht.OnInterrupt += new NativeEventHandler(mySwicht_OnInterrupt);

			while (true)
			{
				if( (_isOn) && (_count < 8))
				{
					SetLights(ToBinary(_count));
					Thread.Sleep(1000);
					_count++;
				}
				else
				{
					_outDig0.Write(false);
					_outDig1.Write(false);
					_outDig2.Write(false);
					_count = 0;
				}
			}
		}

		static void SetLights(string BinaryStringValue)
		{
			bool[] lights = GetBinaryArrayLights(BinaryStringValue);

			_outDig0.Write(lights[0]);
			_outDig1.Write(lights[1]);
			_outDig2.Write(lights[2]);
		}

		/// <summary>
		/// Obtiene un array con los estados de las luces a partir del valor
		/// </summary>
		/// <param name="BinaryValue">Valor en el que se quieren las luces</param>
		/// <returns></returns>
		static bool[] GetBinaryArrayLights(string BinaryValue)
		{
			bool[] lightValues = new bool[3];

			for (int x = 0; x < BinaryValue.Length; x++)
			{
				lightValues[x] = BinaryValue[BinaryValue.Length - x - 1] == '1';
			}

			return lightValues;
		}
		
		/// <summary>
		/// Pasa un valor entero a su correspondiente en binario
		/// </summary>
		/// <param name="value">Valor a convertir</param>
		/// <returns></returns>
		static string ToBinary(int value)
		{
			///http://www.geekpedia.com/tutorial137_Converting-from-decimal-to-binary-and-back.html
			string retVal;
			int mod = 0;
			string valueString = string.Empty;
			char[] valueArray = null;

			while (value > 0)
			{
				mod = value % 2;
				valueString += mod;
				value = value / 2;
			}

			if (valueString.Length > 0)
				valueArray = ReverseArray(valueString.ToCharArray());

			if (valueArray == null)
				return "0";

			retVal = new String(valueArray);

			return retVal;
		}

		/// <summary>
		/// Implementación manual de Array.Reverse() de .NET 
		/// ya que  .NET MF no lo trae.
		/// </summary>
		/// <param name="ArrayToReverse">Array de caracteres a invertir</param>
		/// <returns>Array de caracteres invertidos</returns>
		private static char[] ReverseArray(char[] ArrayToReverse)
		{
			char[] retVal = new char[ArrayToReverse.Length];

			for (int x = 0 ; x < ArrayToReverse.Length; x++)
			{
				retVal[x] = ArrayToReverse[ArrayToReverse.Length - x - 1];
			}

			return retVal;
		}

		static void mySwicht_OnInterrupt(uint data1, uint data2, DateTime time)
		{
			if (data2 == 1)
				_isOn = !_isOn;
		}



	}
}
