using System.Collections.Generic;
using System.Linq;

namespace RadKey.Radicals
{
    static public class NameToRadicalDictionary
    {
        private static Dictionary<string, string> _radicalCodes;

        private static string _radicalNamesFilename;

        public static string Load(string inputFilename)
        {
            _radicalCodes = new Dictionary<string, string>();

            _radicalNamesFilename = inputFilename;

            string[] radicalNames = System.IO.File.ReadAllLines(_radicalNamesFilename);
            string[] tokens;
            for (int x = 0; x < radicalNames.Count(); x++)
            {
                // See if it's blank line.
                if (radicalNames[x] != "")
                {
                    // See if it's comment line.
                    if (radicalNames[x][0] != '#')
                    {
                        // Tokenize it.
                        tokens = radicalNames[x].Trim().Split(' ');
                        // Ignore empty token set.
                        if (tokens.Count() != 0)
                        {
                            // Not checking to elimintate non-radical/multichar 'radicals'. There may be reasons for allowing these.
                            // Were names given?
                            if (tokens.Count() == 1)
                            {
                                return string.Concat("Un-named radical ", tokens[0]);
                            }
                            else
                            {
                                // Create dictionary entries for each radical name.
                                for (int y = 1; y < tokens.Count(); y++)
                                {
                                    // See if we're trying to add the same key twice.
                                    if (_radicalCodes.ContainsKey(tokens[y]))
                                    {
                                        return string.Concat("Non-unique key: ", tokens[y]);
                                    }
                                    // Otherwise, it's safe to add.
                                    _radicalCodes.Add(tokens[y], tokens[0]);
                                }
                            }
                        }
                    }
                }
            }

            return "Radicals loaded correctly.";
        }

        public static string Reload(string inputFilename)
        {
            _radicalCodes.Clear();
            _radicalNamesFilename = inputFilename;
            return Load(inputFilename);
        }

        public static string Reload()
        {
            _radicalCodes.Clear();
            return Load(_radicalNamesFilename);
        }

        public static string Lookup(string toLookup)
        {
            string resultRadical;
            _radicalCodes.TryGetValue(toLookup, out resultRadical);

            if (resultRadical != null)
            {
                return resultRadical;
            }
            else
            {
                return "";
            }
        }

        public static void ShowRadicalNamesFile()
        {
            System.Diagnostics.Process.Start(_radicalNamesFilename);
        }
    }
}
