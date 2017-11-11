using System;
using System.Collections.Generic;
using System.IO;
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

        //private static readonly Regex ModuleIdentityRegex =
        //    new Regex("\\S*\\s*MODULE-IDENTITY.*?::= {.*?}", RegexOptions.Singleline);

        private const string MibsPath = "..//..//mibs//";

        #endregion

        public static ObjectTree Parse(string mibFile)
        {
            //Parse imports
            var importsSection = ImportsSectionRegex.Match(mibFile);
            var importsGroups = ImportsGroupsRegex.Matches(importsSection.Value);
            var importsDictionary = importsGroups.Cast<Match>()
                .ToDictionary(import => import.Value.Split(' ').Last().Replace(";", String.Empty).Trim(),
                    import => GetImportedElements(import.Value));

            //Parse OIDs
            var oidLines = ObjectIdentifierLineRegex.Matches(mibFile);
            var oids = (from Match oidLine in oidLines select CreateOid(oidLine.Value)).ToList();

            foreach (var file in importsDictionary.Keys)
                oids.AddRange(GetOidsFromFile(file));

            //Parse object types
            var objectTypesData =
                ObjectTypeRegex.Matches(mibFile, importsSection.Value.Length); //Do not search in imports section
            var objectTypes = (from Match objType in objectTypesData select CreateObjectType(objType.Value)).ToList();

            //Parse data types
            var dataTypes = new List<DataType>();
            foreach (var import in importsDictionary)
            {
                try
                {
                    var path = MibsPath + import.Key;
                    var file = File.ReadAllText(path);
                    foreach (var type in import.Value)
                    {
                        ParseDataType(file, type, dataTypes);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            var typesToParse = FilterDataTypes(objectTypes.Select(o => o.Syntax));
            foreach (var type in typesToParse)
            {
                if (dataTypes.All(t => t.Name != type))
                {
                    ParseDataType(mibFile, type, dataTypes);
                }
            }
            return new ObjectTree(oids, objectTypes, dataTypes);
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
                Name = oidLine.Trim().Split(' ').First(),
                Class = splitted[1],
                Number = int.Parse(splitted[splitted.Length - 2])
            };
        }

        private static ObjectType CreateObjectType(string objTypeData)
        {
            var classData = ObjectIdentifierDataRegex.Match(ClassDataRegex.Match(objTypeData).Value).Value.Split(' ');
            var objectType = new ObjectType
            {
                Name = objTypeData.Split(' ').First(),
                Syntax = SyntaxRegex.Match(objTypeData).Value.Replace("SYNTAX", string.Empty).Trim(),
                Access = AccessRegex.Match(objTypeData).Value.Replace("ACCESS", string.Empty).Trim(),
                Status = StatusRegex.Match(objTypeData).Value.Replace("STATUS", string.Empty).Trim(),
                Description = DescriptionRegex.Match(objTypeData).Value.Trim('"'),
                Class = classData[1],
                Number = int.Parse(classData[2])
            };
            var restictions = GetRestrictionsFromSyntax(objectType.Syntax);
            if (restictions == null) return objectType;
            objectType.Min = restictions.Item1;
            objectType.Max = restictions.Item2;
            return objectType;
        }

        private static IEnumerable<ObjectIdentifier> GetOidsFromFile(string fileName)
        {
            try
            {
                var path = MibsPath + fileName;
                var mibFile = File.ReadAllText(path);
                var oidLines = ObjectIdentifierLineRegex.Matches(mibFile);
                return from Match oidLine in oidLines select CreateOid(oidLine.Value);
            }
            catch (Exception)
            {
                return new List<ObjectIdentifier>();
            }
        }

        private static void ParseDataType(string file, string typeName, List<DataType> dataTypes)
        {
            var regex = new Regex(typeName + "\\s*::=.*?\n\r\n", RegexOptions.Singleline);
            var match = regex.Match(file);
            if (match.Value != string.Empty)
                dataTypes.Add(CreateDataType(match.Value));
        }

        private static DataType CreateDataType(string typeInfo)
        {
            var baseTypeMatch = new Regex("\\n.*?[({]").Match(typeInfo);
            var restrictionsMatch = new Regex("[{(].*[})]", RegexOptions.Singleline).Match(typeInfo);
            var codingMatch = new Regex("\\[.*?\\]").Match(typeInfo);
            return new DataType
            {
                Name = typeInfo.Trim().Split(' ').First(),
                BaseType = baseTypeMatch.Value.Replace("{", string.Empty).Replace("(", string.Empty)
                    .Replace("IMPLICIT", string.Empty).Replace("EXPLICIT", string.Empty).Trim(),
                Restrictions = restrictionsMatch.Value.Trim('{', '(', ')', '}').Trim(),
                CodingType = typeInfo.Contains("EXPLICIT")
                    ? "EXPLICIT"
                    : typeInfo.Contains("IMPLICIT")
                        ? "IMPLICIT"
                        : string.Empty,
                CodingValue = !string.IsNullOrEmpty(codingMatch.Value)
                    ? int.Parse(codingMatch.Value.Trim('[', ']').Split(' ').Last())
                    : (int?) null
            };
        }

        private static IEnumerable<string> FilterDataTypes(IEnumerable<string> types)
        {
            var result = (from type in types
                where !type.ToLower().Contains("integer") && !type.ToLower().Contains("string") &&
                      !type.ToLower().Contains("object identifier") && !type.ToLower().Contains("null") &&
                      !string.IsNullOrEmpty(type)
                select type.Replace("SEQUENCE OF", string.Empty).Trim()).ToList();
            return result.Distinct();
        }

        public static Tuple<long, long> GetRestrictionsFromSyntax(string syntax)
        {
            var rangeMatch = new Regex("[0-9]*\\.\\.[0-9]*").Match(syntax);
            if (!string.IsNullOrEmpty(rangeMatch.Value))
            {
                var minMax = rangeMatch.Value.Split('.');
                return new Tuple<long, long>(long.Parse(minMax.First()), long.Parse(minMax.Last()));
            }
            return null;
        }
    }
}
