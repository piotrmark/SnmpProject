using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SnmpProject
{
    public static class MibParser
    {
        #region Regex

        private static readonly Regex ImportsSectionRegex = new Regex("IMPORTS.*?;", RegexOptions.Singleline);
        private static readonly Regex ImportsGroupsRegex = new Regex(".*?FROM.*?[\n;]", RegexOptions.Singleline);
        private static readonly Regex ImportsElementsRegex = new Regex(".*FROM", RegexOptions.Singleline);

        private static readonly Regex ObjectIdentifierLineRegex =
            new Regex("^.*OBJECT IDENTIFIER ::=.*$", RegexOptions.Multiline);

        private static readonly Regex ObjectIdentifierDataRegex = new Regex("{.*}");

        #endregion

        public static void Parse(string mibFile) //TODO: return parsed tree
        {
            //Parse imports
            var importsSection = ImportsSectionRegex.Match(mibFile);
            var importsGroups = ImportsGroupsRegex.Matches(importsSection.Value);
            var importsDictionary = importsGroups.Cast<Match>()
                .ToDictionary(import => import.Value.Split(' ').Last().Trim(),
                    import => GetImportedElements(import.Value));

            //Parse OIDs
            var oidLines = ObjectIdentifierLineRegex.Matches(mibFile);
            var oids = (from Match oidLine in oidLines select CreateOid(oidLine.Value)).ToList();
        }

        private static List<string> GetImportedElements(string importGroup)
        {
            var elementsLine = ImportsElementsRegex.Match(importGroup);
            var elements = elementsLine.Value.Split(',');
            for (var i = 0; i < elements.Length; i++)
            {
                elements[i] = elements[i].Replace("IMPORTS", string.Empty);
                elements[i] = elements[i].Replace("FROM", string.Empty);
                elements[i] = elements[i].Trim();
            }
            return elements.ToList();
        }

        private static ObjectIdentifier CreateOid(string oidLine)
        {
            var oidData = ObjectIdentifierDataRegex.Match(oidLine);
            var splitted = oidData.Value.Split(' ');
            return new ObjectIdentifier
            {
                Name = oidLine.Split(' ').First(),
                Class = splitted[1],
                Number = int.Parse(splitted[2])
            };
        }
    }
}
