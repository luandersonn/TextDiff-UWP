using System;
using System.Threading.Tasks;
using TextDiff_UWP.Helpers;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TextDiff_UWP
{
	public sealed partial class MainPage : Page
	{
		DiffHelper DiffHelper { get; } = new DiffHelper();

		ScrollViewer leftScroll;
		ScrollViewer rightScroll;
		const int ScrollLoopbackTimeout = 500;
		object _lastScrollingElement;
		int _lastScrollChange = Environment.TickCount;

		public MainPage()
		{
			InitializeComponent();
			var titleBar = ApplicationView.GetForCurrentView().TitleBar;
			ActualThemeChanged += (s, e) => titleBar.ButtonForegroundColor = ActualTheme == ElementTheme.Dark ? (Color?)Colors.White : (Color?)Colors.Black;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			DiffHelper.LeftPanel = LeftPanelDiff;
			DiffHelper.RightPanel = RightPanelDiff;
			DiffHelper.InlinePanel = InlinePanelDiff;
		}

		#region event handlers
				
		private void TitleBarLoaded(object sender, RoutedEventArgs e)
		{
			// Set a border as title bar
			CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
			var titleBar = ApplicationView.GetForCurrentView().TitleBar;
			titleBar.ButtonBackgroundColor = Colors.Transparent;
			titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
			Window.Current.SetTitleBar((UIElement)sender);
		}
		/// <summary>
		/// Sync scrolls - Thanks to René Sackers on StackOverflow https://stackoverflow.com/a/39425257
		/// </summary>
		private void SynchronizedScrollerOnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			if (_lastScrollingElement != sender && Environment.TickCount - _lastScrollChange < ScrollLoopbackTimeout)
				return;
			if (leftScroll == null || rightScroll == null)
				return;

			_lastScrollingElement = sender;
			_lastScrollChange = Environment.TickCount;

			ScrollViewer sourceScrollViewer;
			ScrollViewer targetScrollViewer;
			if (sender == leftScroll)
			{
				sourceScrollViewer = leftScroll;
				targetScrollViewer = rightScroll;
			}
			else
			{
				sourceScrollViewer = rightScroll;
				targetScrollViewer = leftScroll;
			}
			targetScrollViewer.ChangeView(null, sourceScrollViewer.VerticalOffset, null);
		}

		private async void OpenFileButtonClicked(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			button.IsEnabled = false;
			var file = await OpenFileAsync();
			if (file != null)
			{
				try
				{
					// Get the DiffPanel linked with this button
					var tag = (DiffPanel)button.Tag;
					await tag.SetFileAsync(file);
					await DiffHelper.UpdateDiffAsync();
				}
				catch (Exception ex)
				{
					await new ContentDialog
					{
						Title = "Error",
						Content = ex,
						CloseButtonText = "Close"
					}.ShowAsync();
				}
			}
			button.IsEnabled = true;
		}

		private void PanelDiffLoaded(object sender, RoutedEventArgs e)
		{
			var panel = (DiffPanel)sender;
			var tag = (string)panel.Tag;
			switch (tag)
			{

				case "leftPanel":
					leftScroll = panel.ScrollViewer;
					leftScroll.ViewChanged += SynchronizedScrollerOnViewChanged;
					DiffHelper.LeftPanel = panel;
					break;
				case "rightPanel":
					rightScroll = panel.ScrollViewer;
					rightScroll.ViewChanged += SynchronizedScrollerOnViewChanged;
					DiffHelper.RightPanel = panel;
					break;
				case "inlinePanel":
					DiffHelper.InlinePanel = panel;
					break;
			}
		}
		#endregion

		private async Task<StorageFile> OpenFileAsync()
		{
			var picker = new Windows.Storage.Pickers.FileOpenPicker
			{
				CommitButtonText = "Open",
				ViewMode = Windows.Storage.Pickers.PickerViewMode.List
			};
			picker.FileTypeFilter.Add("*");
			return await picker.PickSingleFileAsync();
		}

		private async void FileChanged(object sender, object e) => await DiffHelper.UpdateDiffAsync();

		private void PanelChangeButtonClick(object sender, RoutedEventArgs e) => pivot.SelectedIndex = pivot.SelectedIndex == 0 ? 1 : 0;

		private void PivotSelectionChanged(object sender, SelectionChangedEventArgs e) => viewChangeButton.Content = pivot.SelectedIndex == 0 ? "\uE75B" : "\uEA61";
	}
}
