using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions
{
    public class SecurityDefinitionParser
    {
        public Dictionary<string, SecurityDefinition> SecurityDefinitions { get; set; }

      


        public MatchCollection GetMatches(string message, Regex regex)
        {
            return regex.Matches(message);
        }

        public string ParseUdsKey(string fixMessage)
        {
            Regex regex = new Regex(@"35=UDS(.*?)\u000110="); // Nu har du hele beskeden.
            if (!regex.IsMatch(fixMessage))
            {
                return null;
            }

            var matches = regex.Matches(fixMessage);

            string keyValue = null;
            foreach (var udsMessage in matches)
            {
                var keyRegex = new Regex($@"\u000148=(.*?)\u0001");

                if (keyRegex.IsMatch(udsMessage.ToString()))
                {
                    var keyMatchOnFirstKey = keyRegex.Match(udsMessage.ToString());
                    // This is our key
                    keyValue = keyMatchOnFirstKey.Groups.Count >= 2 ? keyMatchOnFirstKey.Groups[1].ToString() : null;
                }
                else
                {
                    keyRegex = new Regex($@"9048=(.*?)\u0001");
                    if (!keyRegex.IsMatch(udsMessage.ToString()))
                    {

                    }

                    var m = keyRegex.Match(udsMessage.ToString());
                    if (m.Groups.Count >= 2)
                    {
                        keyValue = m.Groups[1].ToString();
                    }
                }
            }


            return keyValue;
        }


        public string ParseUdsKeyValue(string fixMessage)
        {
            var securityNameRegex = new Regex(@"9062=(.*?)(?=\u0001)"); // Nu har du hele beskeden.
            string securityName = "";

            foreach (var match in securityNameRegex.Matches(fixMessage))
            {
                securityName = match.ToString();
                securityName = securityName.Substring(5, securityName.Length - 5);

                Match securityNameMatch = securityNameRegex.Match(match.ToString());

                if (securityNameMatch.Groups.Count >= 2)
                {
                    securityName = securityNameMatch.Groups[1].ToString();
                    if (securityName.IndexOf('.') > 0)
                    {
                        securityName = securityName.Substring(0, securityName.IndexOf('.'));
                    }
                }
            }

            if (string.IsNullOrEmpty(securityName))
            {
                return null;
                
            }

            return securityName;
        }

        private Dictionary<string, SecurityDefinition> NewParseUdsSecurityDefinition(string fixMessage)
        {
            var key = ParseUdsKey(fixMessage);
            var keyValue = ParseUdsKeyValue(fixMessage);

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(keyValue))
            {
                return null;
            }

            key = CleanKeyOrKeyValue(key);
            keyValue = CleanKeyOrKeyValue(keyValue);

            var dictionary = new Dictionary<string, SecurityDefinition>();
            dictionary.Add(key, new SecurityDefinition(keyValue, "", ""));
            return dictionary;
        }

        public string CleanKeyOrKeyValue(string keyOrValue)
        {
            if (keyOrValue.IndexOf('.') > 0)
            {
                keyOrValue = keyOrValue.Substring(0, keyOrValue.IndexOf('.'));
            }

            if (keyOrValue.IndexOf('-') > 0)
            {
                keyOrValue = keyOrValue.Substring(0, keyOrValue.IndexOf('-'));
            }

            var removeSpecialCharactersRegex = new Regex("[*'\".!,_&#^@]");

            keyOrValue = removeSpecialCharactersRegex.Replace(keyOrValue, string.Empty);

            var removeAllDigits = new Regex("\\d");
            keyOrValue = removeAllDigits.Replace(keyOrValue, string.Empty);

            return keyOrValue.Trim();
        }

        private Dictionary<string, SecurityDefinition> ParseUdsSecurityDefinition(string fixMessage)
        {
            Regex regex = new Regex(@"35=UDS.*?\u000110="); // Nu har du hele beskeden.

            MatchCollection matches = regex.Matches(fixMessage);

            var secDefinitions = new Dictionary<string, SecurityDefinition>();
            foreach (var match in matches)
            {
                var bar = match.ToString();
                string keyValue = "";
                var keyRegex = new Regex($@"\u000148=(.*?)\u0001");
                if (keyRegex.Match(match.ToString()).ToString() == "")
                {
                    keyRegex = new Regex($@"9048=(.*?)\u0001");
                    var key = keyRegex.Match(match.ToString());
                    if (key.ToString() == "")
                    {
                    }
                    else
                    {
                        // good
                        var m = keyRegex.Match(match.ToString());
                        if (m.Groups.Count >= 2)
                        {
                            keyValue = m.Groups[1].ToString();
                        }
                    }
                }
                else
                {
                    // good
                    keyValue = keyRegex.Match(match.ToString()).ToString();

                    var m = keyRegex.Match(match.ToString());
                    if (m.Groups.Count >= 2)
                    {
                        keyValue = m.Groups[1].ToString();
                        if (keyValue.IndexOf('.') > 0)
                        {
                            keyValue = keyValue.Substring(0, keyValue.IndexOf('.'));
                        }
                    }
                }


                if (string.IsNullOrEmpty(keyValue))
                {
                    continue; // - we could not find key.
                }

                // securityName is the value that is looked up in the database.
                var securityNameRegex = new Regex(@"9062=(.*?)(?=\u0001)"); // Nu har du hele beskeden.
                var securityNameMatch = securityNameRegex.Match(match.ToString());
                string securityName = "";
                if (securityNameMatch.Groups.Count >= 2)
                {
                    securityName = securityNameMatch.Groups[1].ToString();
                    if (securityName.IndexOf('.') > 0)
                    {
                        securityName = keyValue.Substring(0, keyValue.IndexOf('.'));
                    }
                }

                if (string.IsNullOrEmpty(securityName))
                {
                    continue;
                }


                var splitted = keyValue.Split('-');
                if (splitted.Count() == 2)
                {
                    keyValue = splitted.First();
                }

                var removeSpecialCharactersRegex = new Regex("[*'\".!,_&#^@]");

                keyValue = removeSpecialCharactersRegex.Replace(keyValue, string.Empty);
                var removeAllDigits = new Regex("\\d");
                keyValue = removeAllDigits.Replace(keyValue, string.Empty);
                securityName = removeSpecialCharactersRegex.Replace(securityName, string.Empty);

                secDefinitions.Add(keyValue, new SecurityDefinition(securityName, "", ""));
            }

            return secDefinitions;
        }

        public string ParseSecurityKey(string rawKey)
        {
            if (rawKey.IndexOf('.') > 0)
            {
                rawKey = rawKey.Substring(0, rawKey.IndexOf('.'));
            }

            var removeSpecialCharactersRegex = new Regex("[*'\".!,_&#^@]");

            rawKey = removeSpecialCharactersRegex.Replace(rawKey, string.Empty);

            var removeAllDigits = new Regex("\\d");
            rawKey = removeAllDigits.Replace(rawKey, string.Empty);

            return rawKey;
        }

        public Dictionary<string, SecurityDefinition> ParseFixMessage(string fixMessage)
        {
            if (fixMessage.Contains("ATW FYF"))
            {

            }
            Dictionary<string, SecurityDefinition> udsSecurityDefinitions = NewParseUdsSecurityDefinition(fixMessage);
            if (udsSecurityDefinitions != null && udsSecurityDefinitions.Any())
            {
                // Nothing found, return null;
                return udsSecurityDefinitions;
            }

            Regex regex = new Regex(@"307=.*?\u0001307="); // Nu har du hele beskeden.

            MatchCollection matches = regex.Matches(fixMessage);


            var secDefinitions = new Dictionary<string, SecurityDefinition>();


            foreach (var match in matches)
            {
                // Tag først 307= som er værdien i databasen. Det skal være din value i dictionary.
                // Tag så som key, tag 309= som svarer til tag 48= i trade. Så 48 slås op som 309, og finder 307 som bruges til mapping.
                // Done, solved, donediego.
                var securityKeyRegex = new Regex($@"309=.*?(?=\u0001)");
                var securityKeyTemp = securityKeyRegex.Match(match.ToString());
                var securityKey = securityKeyTemp.ToString().Substring(4, securityKeyTemp.ToString().Length - 4);
                // Why 4? Because 307=
                var mapValueRegex = new Regex(@"9063=.*?(?=\u0001)"); // Nu har du hele beskeden.
                var mapValueKeyTemp = mapValueRegex.Match(match.ToString());
                var mapvalue = match.ToString().Substring(4, mapValueKeyTemp.ToString().Length - 4);
                if (mapvalue.IndexOf('-') > 0)
                {
                    mapvalue = mapvalue.Substring(0, mapvalue.IndexOf('-'));
                }

                string currencyUnit = string.Empty;
                string quantityUnit = string.Empty;

                var currencyQuantityUnitRegex = new Regex(@"9101=.*?(?=\u0001)");
                var currencyQuantityUnitMatch = currencyQuantityUnitRegex.Match(match.ToString());
                if (currencyQuantityUnitMatch.Success)
                {
                    var currencyQuantityUnitTemp = currencyQuantityUnitMatch.ToString().Substring(5, currencyQuantityUnitMatch.ToString().Length - 5); // 5 because 9101=.length = 5
                    var currencyQuantityUnitSplit = currencyQuantityUnitTemp.Split('/');
                    if (currencyQuantityUnitSplit.Length == 2)
                    {
                        currencyUnit = currencyQuantityUnitSplit.First().Trim();
                        quantityUnit = currencyQuantityUnitSplit.Last().Trim();
                    }
                }

                var removeSpecialCharactersRegex = new Regex("[*'\".!,_&#^@]");

                securityKey = removeSpecialCharactersRegex.Replace(securityKey, string.Empty);
                mapvalue = removeSpecialCharactersRegex.Replace(mapvalue.TrimEnd(), string.Empty);
                var splittedKey = securityKey.Split('-');
                if (splittedKey.Count() == 2)
                {
                    securityKey = splittedKey.First();
                }

                var removeAllDigits = new Regex("\\d");
                securityKey = removeAllDigits.Replace(securityKey, string.Empty);

                if (!secDefinitions.ContainsKey(securityKey))
                {
                    secDefinitions.Add(securityKey, new SecurityDefinition(mapvalue.TrimEnd(), quantityUnit, currencyUnit));
                }
            }

            return secDefinitions;
        }
        public string RemoveUnicodeCharacters(string message)
        {
            var regex = new Regex("U00");

            return regex.Replace(message, string.Empty);
        }

        public string RemoveSpecialCharacters(string message)
        {
            var regex = new Regex("[*'\".!,_&#^@]");

            return regex.Replace(message, string.Empty);
        }

        public string RemoveDigits(string message)
        {
            var regex = new Regex("\\d");

            return regex.Replace(message, string.Empty);
        }
    }
}