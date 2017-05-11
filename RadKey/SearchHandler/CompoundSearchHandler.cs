using RadKey.Compounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RadKey.SearchHandler
{
    public class CompoundSearchHandler
    {
        // Can I dependency inject these?
        private SearchCache _compoundSearchCache = new SearchCache();
        private SearchHistory _compoundSearchHistory = new SearchHistory();
        private CompoundSearch _compoundSearch;

        public CompoundSearchHandler(CompoundSearch compoundSearch)
        {
            _compoundSearch = compoundSearch;
        }

        private void MoveExactMatchToFront(List<Compound> matchResults, string searchString)
        {
            int exactMatchCount = 0;
            for (int x = (matchResults.Count)-1; x > 0; x--)
            {
                if(x < exactMatchCount)
                {
                    break;
                }
                else if(matchResults[x].ToString() == searchString)
                {
                    matchResults.Insert(0, matchResults[x]);
                    matchResults.RemoveAt(x+1);
                    exactMatchCount++;
                }
            }
        }

        public void Search(TextBox searchCriteria, ListBox resultOutput, TextBox outputMessage, TextBox outputReading)
        {
            // Update Japanese punctuation and malformed wildcards to appropriate values.
            TextConverter.ConvertPunctuation(searchCriteria);

            // Do kana or radical conversion.
            // If only romaji were entered, this should just do a straight kana conversion anyway, so
            // no need to do the special checks to see if only romaji were present.
            TextConverter.ConvertToRadicalOrKana(searchCriteria);

            // Find the matches.
            if (!_compoundSearchCache.IsCached(searchCriteria.Text))
            {
                // Cache the search string so that the system doesn't search again if the input criteria hasn't changed.
                _compoundSearchCache.CacheSearchCriteria(searchCriteria.Text);

                // Perform the actual search.
                List<Compound> matchResults = _compoundSearch.Search(searchCriteria.Text);
                  
                if (matchResults.Count() > 0)
                {
                    // Don't update this until we get results -- this should really store the last GOOD string.
                    matchResults.Sort();
                    MoveExactMatchToFront(matchResults, searchCriteria.Text);
                    resultOutput.Items.Clear();                    
                    resultOutput.Items.AddRange(matchResults.ToArray());
                    resultOutput.Focus();
                    resultOutput.SelectedIndex = 0;
                    SelectionInfoHandler.ShowCompoundSelectionInfo((Compound)resultOutput.SelectedItem, outputMessage, outputReading);

                    // Add the search string to history.
                    // Doing this here to stop adding the same search string multiple times/remove failed searches.
                    _compoundSearchHistory.AddHistory(searchCriteria.Text);

                }
                else
                {
                    outputMessage.Text = "No matching compounds.";
                }


            }
            else
            {
                // Don't change focus if the box is blank.
                if (resultOutput.Items.Count > 0)
                {
                    resultOutput.Focus();
                    outputMessage.Text = "";

                }
            }
        }

        public void ForceNewSearch()
        {
            _compoundSearchCache.Clear();
        }

        public void GetNextFromHistory(TextBox searchInput)
        {
            searchInput.Text = _compoundSearchHistory.GetNextFromHistory();
        }

        public void GetLastFromHistory(TextBox searchInput)
        {
            searchInput.Text = _compoundSearchHistory.GetLastFromHistory(searchInput.Text);
        }
    }
}
