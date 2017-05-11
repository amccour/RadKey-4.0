using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadKey
{
    class SearchCache
    {
        private string _cachedSearchCriteria = null;
        public void CacheSearchCriteria(string toCache)
        {
            _cachedSearchCriteria = toCache;
        }

        public void Clear()
        {
            _cachedSearchCriteria = null;
        }

        public bool IsCached(string toCompare)
        {
            if(_cachedSearchCriteria == null)
            {
                return false;
            }
            else if(_cachedSearchCriteria == toCompare)
            {
                return true;
            }
            return false;
        }
    }
}
