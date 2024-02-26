using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace ED.Atlas.Svc.TC.Ice.FE.Mapping
{
    public class FieldMapper
    {
        public string ParseField(int tag, string message)
        {
            var regex = new Regex($"(?={tag}=)(.+?)(?=\\u0001)");
            var partiallyParseMessage = regex.Match(message).Value;

            var lastindex = partiallyParseMessage.IndexOf(tag.ToString()) + tag.ToString().Length + 1;
            var length = partiallyParseMessage.Length - lastindex;
            var result = partiallyParseMessage.Substring(lastindex, length);

            return result;
        }


        public string ParseValueForPartyRoleTag(int id, string originalText)
        {
            var firstRegex = new Regex($"452={id}(?=\\u0001)");
            var keyMatched = firstRegex.IsMatch(originalText);
            if (!keyMatched)
            {
                // Log: Could not find the key.
                return string.Empty;
            }

            var secondRegex = new Regex($"452={id}\\u0001448=(.+?)(?=\\u0001)");
            var valueMatchRegex = secondRegex.Match(originalText);

            if (!valueMatchRegex.Success)
            {
                return string.Empty;
            }

            int startIndex = valueMatchRegex.ToString().IndexOf("448=") + 4;
            var valueMatch = valueMatchRegex.ToString().Substring(startIndex, valueMatchRegex.ToString().Length - startIndex);
            return valueMatch;
        }
        public string RemoveSpecialCharacters(string message)
        {
            var regex = new Regex("[*'\".!,-_&#^@|]");

            return regex.Replace(message, string.Empty);
        }

        public string ParseTrader(string originalText, List<string> dayAheadTraderNames)
        {
            if (originalText.Contains("edtray-fx7|"))
            {
                originalText = originalText.Replace("edtray-fx7|", string.Empty);
            }
            string parseTrayportTrader = string.Empty;

            parseTrayportTrader = ParseTrayportTrader(originalText);
            Debug.WriteLine("");

            Debug.WriteLine("ParseTrader");
            
            var regex = new Regex($"448=(.+?)(?=\\u0001)");
            MatchCollection matches = regex.Matches(originalText);

            foreach (Match match in matches)
            {
                Debug.WriteLine($"Match ={match}");
                var matchToString = match.ToString().Replace("448=", string.Empty);
             
                matchToString = RemoveSpecialCharacters(matchToString);
                matchToString = matchToString.Replace("edtrayfx", string.Empty);
                string trader = dayAheadTraderNames.FirstOrDefault(x => x == (matchToString));
                if (!string.IsNullOrEmpty(trader))
                {
                    return trader;
                }
                
            }

            // 448=edtray-fx7|jolp

            //if (originalText.Contains("edtray-fx7"))
            //{
            //    return "edtray-fx7";
            //}

            return string.Empty;
        }

        private string ParseTrayportTrader(string originalText)
        {
            try
            {
                var regex = new Regex($"448=(.+?)(?=\\u0001)");
                MatchCollection matches = regex.Matches(originalText);

                bool doesContainTrayport = false;

                if (matches.Count > 0)
                {
                    foreach (var match in matches)
                    {
                        if (match.ToString().ToLower().Contains("trayport"))
                        {
                            doesContainTrayport = true;
                            break;
                        }
                    }
                }

                if (doesContainTrayport)
                {
                    var foo = matches[matches.Count - 1].ToString();
                    var splitted = foo.Split('|');
                    if (splitted.Length == 2)
                    {
                        return splitted[1];
                    }
                }
            }
            catch (Exception ex)
            {
                var foo = ex;
                return string.Empty;
            }

            return string.Empty;
        }
    }
}