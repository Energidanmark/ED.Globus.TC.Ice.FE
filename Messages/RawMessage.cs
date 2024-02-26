using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ED.Atlas.Svc.TC.Ice.FE.Trades;

namespace ED.Atlas.Svc.TC.Ice.FE.Messages
{
    public class RawMessage
    {
        public string Text { get; }

        public RawMessage(string text)
        {
            Text = text;
        }

        public BlockingCollection<FixMessage> ParseIntoMessages()
        {
            var message = Text.TrimEnd(new char[] { (char)0 });
            var splitted = Regex.Split(message, @"8=FIX.4.4");
            if (!splitted.Any() || (splitted[0] == string.Empty && splitted.Length == 1))
            {
                return null;
            }
            var splitList = splitted.ToList();
            splitList.RemoveAt(0);

            var messages = new BlockingCollection<FixMessage>();
            foreach (var rawMessageSplitted in splitList)
            {
                var builder = new StringBuilder(rawMessageSplitted);
                builder.Insert(0, @"8=FIX.4.4");
                messages.Add(new FixMessage(builder.ToString()));
            }

            return messages;
        }
    }
}