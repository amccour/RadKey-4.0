using RadKey.Compounds;
using RadKey.Kanji;
using RadKey.Radicals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RadKey.SearchHandler
{
    public class KanjiSearchHandler
    {        
        private SearchCache _kanjiSearchCache = new SearchCache();

        // Control whether the kanji search results include low frequency kanji or not.
        private bool _noLowFrequencyKanji = true;

        private KanjiSearch _kanjiSearch;
        private IKanjidic _kanjiDictionary;

        public class KanjiFiltering
        {
            public const bool HideLowFrequencyKanji = true;
            public const bool ShowLowFrequencyKanji = false;
        }

        public KanjiSearchHandler(KanjiSearch kanjiSearch, IKanjidic kanjiDictionary)
        {
            _kanjiSearch = kanjiSearch;
            _kanjiDictionary = kanjiDictionary;
        }

        public bool TogglLowFrequencyKanji()
        {
            _noLowFrequencyKanji = _noLowFrequencyKanji ^ true;
            return _noLowFrequencyKanji;
        }

        // Convert the list of kanji characters into sortable/filterable kanji data.
        // TODO: This shouldn't be part of kanji search. Kanji Search just gets the kanji strings.
        private List<KanjiData> BuildKanjiDataList(List<char> kanjiCharList)
        {
            List<KanjiData> kanjiDataList = new List<KanjiData>();
            KanjiData tKanjiData;
            bool addFlag = true;

            foreach(char kanjiChar in kanjiCharList)
            {
                // Get a KanjiData object from the dictionary, based on the input text.
                tKanjiData = _kanjiDictionary.Lookup(kanjiChar.ToString());

                // NOTE: Ideally this should check that tKanjiData isn't null. However, for now,
                // I want the application to throw an exception if that happens. You should NOT be
                // getting nulls at this point.

                // Otherwise add everything.
                if (addFlag == true)
                {
                    kanjiDataList.Add(tKanjiData);
                }

                // Reset addFlag to true, as this is its base case.
                addFlag = true;

            }

            return kanjiDataList;
        }

        private List<KanjiData> FilterByStrokes(List<KanjiData> toFilter, int strokes)
        {
            if(toFilter == null)
            {
                return new List<KanjiData>();
            }

            List<KanjiData> filteredResultKanji = new List<KanjiData>();

            // Result Filtering Section
            // First, filter out low-frequency kanji if necessary.
            if (_noLowFrequencyKanji == true)
            {
                foreach (KanjiData kanji in toFilter)
                {
                    if ((kanji.Frequency() != 9999) || (kanji.Grade() < 9))
                    {
                        // Also needs to check if a stroke count filter was provided. Could do this as a second pass over the list but that could potentially get slow.
                        if (strokes > 0)
                        {
                            if (kanji.Strokes() >= strokes)
                            {
                                filteredResultKanji.Add(kanji);
                            }
                        }
                        else
                        {
                            filteredResultKanji.Add(kanji);
                        }
                    }
                }
            }

            // If not filtering out low-frequency kanji, see if a specific stroke filter was provided.
            else
            {
                if (strokes > 0)
                {
                    foreach (KanjiData kanji in toFilter)
                    {
                        if (kanji.Strokes() >= strokes)
                        {
                            filteredResultKanji.Add(kanji);
                        }
                    }                    
                }
                else
                {
                    return toFilter;
                }
            }

            return filteredResultKanji;
        }

        public void Search(TextBox searchInput, TextBox strokeInput, ListBox resultOutput, TextBox outputMessage, TextBox outputReading, TextBox outputMeaning)
        {
            // Searching for an empty string should move the cursor back to the result output list if it's not empty.
            if (searchInput.Text == "")
            {
                if(resultOutput.Items.Count > 0)
                {
                    resultOutput.Focus();
                    SelectionInfoHandler.ShowKanjiSelectionInfo(resultOutput.Text, outputMessage, outputReading, outputMeaning);
                    return;
                }
            }

            int strokes = -1;
            if (strokeInput.Text != "")
            {
                strokes = System.Int32.Parse(strokeInput.Text);
            }

            if(!_kanjiSearchCache.IsCached(searchInput.Text + ";" + strokeInput.Text))
            {                
                List<KanjiData> filteredResultKanji 
                    = FilterByStrokes(BuildKanjiDataList(_kanjiSearch.Search(searchInput.Text)), strokes);

                if (filteredResultKanji.Count > 0)
                {
                    _kanjiSearchCache.CacheSearchCriteria(searchInput.Text + ";" + strokeInput.Text);
                    filteredResultKanji.Sort();
                    resultOutput.Items.Clear();
                    resultOutput.Items.AddRange(filteredResultKanji.ToArray());
                    resultOutput.Focus();
                    resultOutput.SelectedIndex = 0;
                    SelectionInfoHandler.ShowKanjiSelectionInfo(resultOutput.Text, outputMessage, outputReading, outputMeaning);
                }
                else
                {
                    outputMessage.Text = "No matching kanji.";
                    // Don't need to change the last focus or last search set here. The results aren't getting cleared or changed.
                    searchInput.Focus();
                }

            }
            else
            {
                resultOutput.Focus();
                SelectionInfoHandler.ShowKanjiSelectionInfo(resultOutput.Text, outputMessage, outputReading, outputMeaning);
            }
            
        }

        public void ForceNewSearch()
        {
            _kanjiSearchCache.Clear();
        }
    }
}
