using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ED.Atlas.Svc.TC.Ice.FE.Common;
using ED.Atlas.Svc.TC.Ice.FE.Heartbeats;
using ED.Atlas.Svc.TC.Ice.FE.IceConnectivity;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions;
using ED.Atlas.Svc.TC.Ice.FE.TestReqIdMessages;
using ED.Atlas.Svc.TC.Ice.FE.Usercompany;

namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public class FixParser
    {
        private readonly BlockingCollection<string> _tradeCaptures = new BlockingCollection<string>();

        public FixParser(BlockingCollection<string> tradeCaptures)
        {
            _tradeCaptures = tradeCaptures;
        }

        public string DetermineType(string fixMessage)
        {
            var typeIndex = fixMessage.IndexOf("35=") + 3;

            var fromTypeToMessageEnd = fixMessage.Substring(typeIndex, fixMessage.Length - typeIndex);

            var messageTypeEndIndex = fromTypeToMessageEnd.IndexOf("\u0001");

            var messageType = fromTypeToMessageEnd.Substring(0, messageTypeEndIndex);

            return messageType;
        }

        public event EventHandler<IMessage> OnMessageParsed;
        protected virtual void OnOnMessageParsed(IMessage e)
        {
            OnMessageParsed?.Invoke(this, e);
        }

        public void SaveTradeCapture(string message)
        {
            _tradeCaptures.Add(message);
        }
        public HeartbeatMessage HeartbeatMessage(string fixMessage)
        {

            DateTime sendingTime = DateTime.MinValue;
            var values = Regex.Split(fixMessage, @"\u0001");

            var type = DetermineType(fixMessage);

            var sendingTimeRaw = values.FirstOrDefault(x => x.Contains("52="));

            var sendingTimeSplit = sendingTimeRaw.Split('=');

            sendingTime = DateTimeExtensions.TryParseIceFormat(sendingTimeSplit.Last());


            return new HeartbeatMessage(sendingTime, fixMessage);
        }

        public TestReqIdMessage TestReqMessage(string fixMessage)
        {

            int testReqId = -1;
            DateTime sendingTime = DateTime.MinValue;

            var values = Regex.Split(fixMessage, @"\u0001");

            var type = DetermineType(fixMessage);

            var rawReqId = values.FirstOrDefault(x => x.Contains("112="));

            var rawReqIdSplit = rawReqId.Split('=');


            int.TryParse(rawReqIdSplit.Last(), out testReqId);


            var rawSendingTime = values.FirstOrDefault(x => x.Contains("52="));
            var rawSendingTimeSplit = rawSendingTime.Split('=');
            sendingTime = DateTimeExtensions.TryParseIceFormat(rawSendingTimeSplit.Last());


            return new TestReqIdMessage(testReqId, sendingTime, fixMessage);
        }

        public LogonResponse LogonResponse(string fixMessage)
        {
            DateTime sendingTime = DateTime.MinValue;
            var values = Regex.Split(fixMessage, @"\u000110");

            var type = DetermineType(fixMessage);
            bool isLoggedIn = false;
            var sendingTimeRaw = values.FirstOrDefault(x => x.Contains("52="));

            var sendingTimeSplit = sendingTimeRaw.Split('=');

            var logonResponsevalue = ParseField(35, fixMessage);
            isLoggedIn = logonResponsevalue == "0";

            DateTime.TryParse(sendingTimeSplit.Last(), out sendingTime);


            return new LogonResponse(isLoggedIn, sendingTime, fixMessage);
        }

        public Logout Logout(string fixMessage)
        {
            
            return new Logout(DateTime.Now, fixMessage);
        }
        public SecurityDefinitionReceiver SecurityDefinition(string fixMessage)
        {
            DateTime sendingTime = DateTime.MinValue;
            var values = Regex.Split(fixMessage, @"\u000110");

            var type = DetermineType(fixMessage);

            var sendingTimeRaw = values.FirstOrDefault(x => x.Contains("52="));

            var sendingTimeSplit = sendingTimeRaw.Split('=');

            DateTime.TryParse(sendingTimeSplit.Last(), out sendingTime);


            return new SecurityDefinitionReceiver(fixMessage);
        }


        public UserCompanyReceiver UserCompanyReceiver(string fixMessage)
        {
            return new UserCompanyReceiver(fixMessage);
        }

        public TradeCaptureReportMessage TradeCapture(string message)
        {
            var type = DetermineType(message);

            return new TradeCaptureReportMessage(message);
        }

        
        private string ParseField(int tag, string message)
        {
            var regex = new Regex($"(?={tag}=)(.+?)(?=\\u0001)");
            var partiallyParseMessage = regex.Match(message).Value;

            var lastindex = partiallyParseMessage.IndexOf(tag.ToString()) + tag.ToString().Length + 1;
            var length = partiallyParseMessage.Length - lastindex;
            var result = partiallyParseMessage.Substring(lastindex, length);

            return result;
        }

    }
}

