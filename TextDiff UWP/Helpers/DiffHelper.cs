using DiffPlex;
using DiffPlex.DiffBuilder;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace TextDiff_UWP.Helpers
{
	public class DiffHelper
	{
		public DiffPanel LeftPanel { get; set; }
		public DiffPanel RightPanel { get; set; }
		public DiffPanel InlinePanel { get; set; }

		public async Task UpdateDiffAsync()
		{
			if (LeftPanel.File == null || RightPanel.File == null)
				return;
			var textFile1 = await FileIO.ReadTextAsync(LeftPanel.File);
			var textFile2 = await FileIO.ReadTextAsync(RightPanel.File);

			var differ = new Differ();
			var sideBySideDiff = new SideBySideDiffBuilder(differ); // Side by side
			var inlineDiff = new InlineDiffBuilder(differ);         // Inline
			var sideBySideModel = await Task.Run(() => sideBySideDiff.BuildDiffModel(textFile1, textFile2));
			var inlineModel = await Task.Run(() => inlineDiff.BuildDiffModel(textFile1, textFile2));
			
			LeftPanel.ItemsSource = sideBySideModel.OldText.Lines;
			RightPanel.ItemsSource = sideBySideModel.NewText.Lines;
			InlinePanel.ItemsSource = inlineModel.Lines;
		}
	}
}
