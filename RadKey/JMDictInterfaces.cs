using RadKey.Compounds;
using RadKey.Kanji;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadKey
{
    public interface IRadkFile
    {
        /// <summary>
        /// Given a radical, return a string of kanji matching that radical.
        /// </summary>
        /// <param name="toLookup"></param>
        /// <returns></returns>
        string Lookup(string toLookup);
    }

    public interface IKradFile
    {
        string Lookup(string toLookup);
    }

    public interface IKanjidic
    {
        KanjiData Lookup(string toLookup);
    }

    public interface IJMDictFile
    {
        List<Compound> Lookup();
    }
}
