using DiffPlex.DiffBuilder.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TextDiff_UWP.Helpers
{
	public class FileLineTemplateSelector : DataTemplateSelector
	{
		public DataTemplate Unchanged { get; set; }
		public DataTemplate Inserted { get; set; }
		public DataTemplate Deleted { get; set; }
		public DataTemplate Modified { get; set; }
		public DataTemplate DefaultDataTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item is DiffPiece piece)
			{
				return piece.Type switch
				{
					ChangeType.Inserted => Inserted,
					ChangeType.Deleted => Deleted,
					ChangeType.Modified => Modified,
					_ => Unchanged
				};
			}
			else
				return DefaultDataTemplate;
		}
	}
}
