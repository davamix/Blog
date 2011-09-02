using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace WPFTreeviewSample
{
	using System.ComponentModel;
	using System.Collections.ObjectModel;

	public class NodeItem : INotifyPropertyChanged
	{
		#region Miembros de INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private string _text;
		public string Text
		{
			get { return _text; }
			set
			{
				_text = value;
				OnPropertyChanged("Text");
			}
		}

		private bool _isChecked;
		public bool IsChecked
		{
			get { return _isChecked; }
			set
			{
				_isChecked = value;
				OnPropertyChanged("IsChecked");
			}
		}

		private ObservableCollection<NodeItem> _items;
		public ObservableCollection<NodeItem> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged("Items");
			}
		}

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public NodeItem()
		{
			Items = new ObservableCollection<NodeItem>();
		}
	}
}
