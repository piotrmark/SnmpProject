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

        private static readonly Regex ObjectTypeRegex =
            new Regex("\\S*\\s*OBJECT-TYPE.*?::= {.*?}", RegexOptions.Singleline);

        private static readonly Regex SyntaxRegex = new Regex("SYNTAX.*");
        private static readonly Regex AccessRegex = new Regex("ACCESS.*");
        private static readonly Regex StatusRegex = new Regex("STATUS.*");
        private static readonly Regex DescriptionRegex = new Regex("\".*\"", RegexOptions.Singleline);
        private static readonly Regex ClassDataRegex = new Regex("::= {.*");

        private static readonly Regex ModuleIdentityRegex =
            new Regex("\\S*\\s*MODULE-IDENTITY.*?::= {.*?}", RegexOptions.Singleline);

        #endregion

        public static ObjectTree Parse(string mibFile) //TODO: return parsed tree
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

            //Parse object types
            var objectTypesData =
                ObjectTypeRegex.Matches(mibFile, importsSection.Value.Length); //Do not search in imports section
            var objectTypes = (from Match objType in objectTypesData select CreateObjectType(objType.Value)).ToList();

            //Parse object identity
            var moduleIdentity = CreateOid(ModuleIdentityRegex.Match(mibFile, importsSection.Value.Length).Value);

            return new ObjectTree(moduleIdentity, oids, objectTypes);
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
            return elements.Where(e => e != "OBJECT-TYPE").ToList();
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

        private static ObjectType CreateObjectType(string objTypeData)
        {
            var classData = ObjectIdentifierDataRegex.Match(ClassDataRegex.Match(objTypeData).Value).Value.Split(' ');
            return new ObjectType
            {
                Name = objTypeData.Split(' ').First(),
                Syntax = SyntaxRegex.Match(objTypeData).Value.Replace("SYNTAX", string.Empty).Trim(),
                Access = AccessRegex.Match(objTypeData).Value.Replace("ACCESS", string.Empty).Trim(),
                Status = StatusRegex.Match(objTypeData).Value.Replace("STATUS", string.Empty).Trim(),
                Description = DescriptionRegex.Match(objTypeData).Value.Trim('"'),
                Class = classData[1],
                Number = int.Parse(classData[2])
            };
        }
    }
}
