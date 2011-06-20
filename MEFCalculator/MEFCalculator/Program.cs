using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using MEFOperation;


namespace MEFCalculator
{
	class Program
	{
		//Contenedor de plugins
		private static CompositionContainer _container;
		private static List<string> _operationList;

		[ImportMany(AllowRecomposition = true)]
		private static IEnumerable<Lazy<Operation, IOperationOptions>> AllPlugins { get; set; }

		public static List<Lazy<Operation, IOperationOptions>> PluginControl
		{
			get
			{
				var _plugins = (from plugin in AllPlugins
								let metadata = plugin.Metadata
								select plugin).ToList();

				return _plugins;
			}
		}

		static void Main(string[] args)
		{
			//Ruta donde buscará los plugins
			string pluginsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
			var catalog = new DirectoryCatalog(pluginsPath);

			_container = new CompositionContainer(catalog);
			_operationList = new List<string>();

			try
			{
				_container.ComposeParts();
					//_operationList = (from plugin in PluginControl
					//                  let metadata = plugin.Metadata
					//                  select metadata.Name).ToList();

				var lPlugins = (from plugin in PluginControl
								let metadata = plugin.Metadata
								select metadata.Name).ToList();
				

				CrearMenu();

				Console.Read();
			}
			catch (CompositionException ex)
			{
				Console.WriteLine("ERROR: {0}", ex.Message);
			}

		}

		private static void CrearMenu()
		{
			Console.WriteLine("### CALCULADORA MEF ###");

			for (int x = 0; x <= _operationList.Count - 1; x++)
				Console.WriteLine("{0}. {1}", x, _operationList[x]);

			Console.Write("\nSeleccoione una operacion: ");
		}
	}
}
