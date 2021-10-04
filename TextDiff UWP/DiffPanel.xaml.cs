using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace TextDiff_UWP
{
	public sealed partial class DiffPanel : UserControl
	{

		public event EventHandler<object> FileChanged;

		public DiffPanel() => InitializeComponent();

		#region Dependency Properties		
		public StorageFile File
		{
			get => (StorageFile)GetValue(FileProperty);
			private set => SetValue(FileProperty, value);
		}
		public static readonly DependencyProperty FileProperty =
			DependencyProperty.Register("File", typeof(StorageFile), typeof(DiffPanel), new PropertyMetadata(null));

		public IEnumerable<DiffPiece> ItemsSource
		{
			get { return (IEnumerable<DiffPiece>)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IEnumerable<DiffPiece>), typeof(DiffPanel), new PropertyMetadata(null));



		public bool IsDragAndDropPaneLoaded
		{
			get { return (bool)GetValue(IsDragAndDropPaneLoadedProperty); }
			set { SetValue(IsDragAndDropPaneLoadedProperty, value); }
		}

		public static readonly DependencyProperty IsDragAndDropPaneLoadedProperty =
			DependencyProperty.Register("IsDragAndDropPaneLoaded", typeof(bool), typeof(DiffPanel), new PropertyMetadata(true));




		#endregion

		public ScrollViewer ScrollViewer { get; private set; }
		public async Task SetFileAsync(StorageFile file)
		{
			try
			{
				var value = await Helpers.DiffHelper.ReadTextAsync(file);

				ItemsSource = value.Split(Environment.NewLine, options: StringSplitOptions.None)
									.Select((line, index) => new DiffPiece(line, ChangeType.Unchanged, index + 1));

				File = file;
				FileChanged?.Invoke(this, null);
				if (file != null)
					IsDragAndDropPaneLoaded = false;
			}
			catch (Exception e)
			{
				await new ContentDialog
				{
					Title = "Error",
					Content = e,
					CloseButtonText = "Close"
				}.ShowAsync();
			}
		}

		#region event handlers		
		private void ListViewLoaded(object sender, RoutedEventArgs e)
		{
			var border = (Border)VisualTreeHelper.GetChild(listview, 0);
			ScrollViewer = (ScrollViewer)border.Child;
			ScrollViewer.CanContentRenderOutsideBounds = true;
		}

		private void OnDragLeave(object sender, DragEventArgs e)
		{
			if (File != null)
				IsDragAndDropPaneLoaded = false;
		}

		private void OnDragOver(object sender, DragEventArgs e)
		{
			IsDragAndDropPaneLoaded = true;
			e.AcceptedOperation = DataPackageOperation.Link;
		}

		private async void OnDrop(object sender, DragEventArgs e)
		{
			IsDragAndDropPaneLoaded = false;

			if (e.DataView.Contains(StandardDataFormats.StorageItems))
			{
				var items = await e.DataView.GetStorageItemsAsync();
				if (items.Count > 0)
					await SetFileAsync((StorageFile)items[0]);

			}
		}
		#endregion
	}
}
