using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadKey
{
    public static class RegexPatterns
    {
        public const string isLatinPattern = "\\p{IsBasicLatin}";
        public const string isNotLatinPattern = "\\P{IsBasicLatin}";
        public const string isSpecialPattern = "[\\*\\@\\[\\]]";
        public const string MainKanaConversionPattern = "[\\@\\[\\]\\*aeiounAEIOUN\\P{IsBasicLatin}]";
    }
}
