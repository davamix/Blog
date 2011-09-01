using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFTreeviewSample
{
	/// <summary>
	/// Lógica de interacción para MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		NodeItem _rootItem;

		public MainWindow()
		{
			InitializeComponent();

			_rootItem =CreateItems();

			trvLista.DataContext = _rootItem;
		}

		/// <summary>
		/// Se crea el árbol de nodos
		/// </summary>
		/// <returns></returns>
		private NodeItem CreateItems()
		{
			//Elemento principal
			NodeItem ItemMain = new NodeItem { Text = "Main" };

			//Se crean dos sub nodos.
			NodeItem ItemA = new NodeItem { Text = "Item A" };
			NodeItem ItemB = new NodeItem { Text = "Item B" };

			//Se añaden al nodo principal
			ItemMain.Items.Add(ItemA);
			ItemMain.Items.Add(ItemB);

			//Grupo de sub nodos para ItemA
			NodeItem ItemA1 = new NodeItem { Text = "Item A 1" };
			NodeItem ItemA2 = new NodeItem { Text = "Item A 2" };
			NodeItem ItemA3 = new NodeItem { Text = "Item A 3" };

			//Se añaden al Item A
			ItemA.Items.Add(ItemA1);
			ItemA.Items.Add(ItemA2);
			ItemA.Items.Add(ItemA3);

			//Grupo de sub nodos para ItemB
			NodeItem ItemB1 = new NodeItem { Text = "Item B 1" };
			NodeItem ItemB2 = new NodeItem { Text = "Item B 2" };

			//Se añaden al Item B
			ItemB.Items.Add(ItemB1);
			ItemB.Items.Add(ItemB2);

			//Se añade un tercer nivel, al ItemA1
			NodeItem ItemAA = new NodeItem { Text = "Item de tercer nivel" };
			ItemA1.Items.Add(ItemAA);

			return ItemMain;
		}

		/// <summary>
		/// Devuelve el item desde el que se lanza el evento Checked
		/// </summary>
		/// <param name="obj">Elemento que lanza el evento (Checkbox)</param>
		/// <returns>Item del treeview que contiene el checkbox</returns>
		private TreeViewItem GetNodetItem(DependencyObject obj)
		{
			if (obj != null)
			{
				DependencyObject parent = VisualTreeHelper.GetParent((DependencyObject)obj);

				TreeViewItem project = parent as TreeViewItem;

				return (project != null) ? project : GetNodetItem(parent);
			}

			return null;
		}

		/// <summary>
		/// Muestra el estado de nodo y sus hijos, si tiene.
		/// </summary>
		/// <param name="item"></param>
		private void ShowItems(NodeItem item)
		{
			AddMensaje(String.Format("{0}: {1}", item.Text, item.IsChecked));

			foreach (NodeItem node in item.Items)
			{
				if (item.Items.Count > 0)
					ShowItems(node);
			}
		}

		/// <summary>
		/// Muestra un mensaje en la lista de mensajes
		/// </summary>
		/// <param name="mensaje"></param>
		private void AddMensaje(string mensaje)
		{
			lstMensajes.Items.Add(mensaje);
		}

		#region "Eventos"
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ShowItems(_rootItem);
		}

		private void CheckBox_OnCheck(object sender, RoutedEventArgs e)
		{
			TreeViewItem item = GetNodetItem((DependencyObject)sender);

			NodeItem node = item.Header as NodeItem;

			AddMensaje(String.Format("{0} is checked? {1}", node.Text, node.IsChecked));

		}

		#endregion
		

		



	}
}

