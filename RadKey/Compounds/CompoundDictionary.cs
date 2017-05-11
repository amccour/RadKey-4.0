using System.Collections.Generic;
using System.Xml;

namespace RadKey.Compounds
{
    public class CompoundDictionary : IJMDictFile
    {
        private List<Compound> _compounds = null;

        public CompoundDictionary(string inputFilename)
        {
            _compounds = new List<Compound>();

            // Store temporary data for the next compound to load.
            List<string> nextKeb = new List<string>();
            List<string> nextReb = new List<string>();
            List<string> nextGloss = new List<string>();

            bool uk = false;
            string miscTag = "";

            XmlTextReader JMDict = new XmlTextReader(inputFilename);
            while (JMDict.Read())
            {

                switch (JMDict.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (JMDict.Name)
                        {
                            case "keb":
                                // Read again to get the value.
                                JMDict.Read();
                                nextKeb.Add(JMDict.Value);
                                break;
                            case "reb":
                                JMDict.Read();
                                nextReb.Add(JMDict.Value);
                                break;
                            case "gloss":
                                JMDict.Read();

                                if (miscTag != "")
                                {
                                    nextGloss.Add(JMDict.Value + " " + miscTag);
                                    miscTag = "";
                                }
                                else
                                {
                                    nextGloss.Add(JMDict.Value);
                                }

                                break;
                            case "misc":
                                JMDict.Read();

                                if (JMDict.Name == "uk")
                                {
                                    uk = true;
                                }

                                miscTag = miscTag + " (" + JMDict.Name + ")";

                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        break;
                    case XmlNodeType.EndElement:
                        // Entry ended. Add compound and clear out the next___ stuff.
                        if (JMDict.Name == "entry")
                        {
                            // Add every compound entry.
                            if (nextKeb.Count > 0)
                            {
                                // Word is usually written in kana. Add redundant kana entries, and tag as such.
                                // Technically UK is on a per-definition basis but I don't really have that level of granularity
                                // and it might be misleading anyway as that could hide definitions.
                                if (uk == true)
                                {
                                    foreach (string rebOnly in nextReb)
                                    {
                                        _compounds.Add(new Compound(rebOnly, nextGloss, nextReb));
                                    }
                                }
                                foreach (string compound in nextKeb)
                                {
                                    _compounds.Add(new Compound(compound, nextGloss, nextReb));
                                }
                            }
                            // Word is kana only. Add as such.
                            else
                            {
                                foreach (string rebOnly in nextReb)
                                {
                                    _compounds.Add(new Compound(rebOnly, nextGloss, nextReb));
                                }
                            }

                            nextKeb = new List<string>();
                            nextReb = new List<string>();
                            nextGloss = new List<string>();
                            uk = false;
                            miscTag = "";

                        }
                        break;
                }
            }
        }

        public List<Compound> Lookup()
        {
            return _compounds;
        }
    }
}
