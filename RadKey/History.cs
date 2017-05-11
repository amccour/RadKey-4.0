using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RadKey
{
    public class SearchHistory
        {
        private List<string> _history = new List<string>();
        private const int _maxRecords = 100;

        private int historyIndex = 0;
        // Notes on the above. When historyIndex = ...
        // history.Count - 1 <- Last (most recent) record in history.
        // history.Count <- One past the last record in histoy. Indicates storedCurrentSearch should be used.
        // 0 <- First (least recent) record in history.

        // Holds the current search prior to navigating history. i.e., if you typed something and hit pageup, this is what you would've typed.
        private string storedCurrentSearch;

        // Add a string to the search history. If the search historyis to big, remove the last (actually first) record.
        // Also reset the historyIndex to the first (actually last) record of the history list.
        public void AddHistory(string toAdd)
        {
            _history.Add(toAdd);

            if (_history.Count > _maxRecords)
            {
                // Add adds to the end of the list. As such, the last element in the list will be the most recent in history.
                // The one at 0 will be the least recent.
                _history.RemoveAt(0);
            }

            historyIndex = _history.Count;
        }

        private string GetFromHistory()
        {
            return _history.ElementAt(historyIndex);
        }

        // Move the history index forward by increasing it, then get something from history.
        public string GetNextFromHistory()
        {

            if (historyIndex < _history.Count)
            {
                historyIndex++;
            }

            if (historyIndex == _history.Count)
            {
                // historyIndex == history.Count and the user hit PgDwn. Retrieve whatever was originally typed in.
                return storedCurrentSearch;
            }

            return GetFromHistory();
        }

        // Move the history index back, set the storedCurrentSearch if needed, and retrieve the history record.
        public string GetLastFromHistory(string currentSearch)
        {
            // Looking at the most recent history item already. Log the current search 
            if (historyIndex == _history.Count)
            {
                storedCurrentSearch = currentSearch;
            }

            if(_history.Count == 0)
            {
                return currentSearch;
            }
            else if (historyIndex > 0)
            {
                historyIndex--;
            }

            return GetFromHistory();
        }
    }
}