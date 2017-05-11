using RadKey.Kanji;
using RadKey.SearchHandler;

namespace RadKey
{
    partial class RadKey
    {
        private void toggleSortByFreq()
        {
            kanjiSearchManager.ForceNewSearch();
            compoundSearchHandler.ForceNewSearch();

            if (KanjiData.ToggleSortingMethod() == KanjiData.SortingMethod.FrequencyOnly)
            {
                messageBox.Text = "Now sorting by frequency only.";
                frequencySortCB.Checked = true;
            }
            else
            {
                messageBox.Text = "Now sorting by stroke count and frequency.";
                frequencySortCB.Checked = false;
            }
        }

        private void toggleNoLowFreq()
        {
            kanjiSearchManager.ForceNewSearch();

            // TODO: See if I can't move this out to the SearchManagers.

            if (kanjiSearchManager.TogglLowFrequencyKanji() == KanjiSearchHandler.KanjiFiltering.HideLowFrequencyKanji)
            {
                messageBox.Text = "Now filtering out low frequency kanji.";
                includeLowFreqCB.Checked = false;
            }
            else
            {
                messageBox.Text = "Now including low frequency kanji.";
                includeLowFreqCB.Checked = true;
            }
        }
    }
}
