using ED.Atlas.Svc.TC.Ice.FE.Logging;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using log4net;
using System;
using System.Diagnostics;
using System.Threading;

namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public class FixMessageFactory
    {
        private readonly FixParser _fixParser;

        private static readonly log4net.ILog _heartbeatLog = LogManager.GetLogger(LoggerNames.HeartbeatLoggerName);
        private static readonly log4net.ILog _logonLog = LogManager.GetLogger(LoggerNames.LogonLoggerName);
        private static readonly log4net.ILog _securityLog = LogManager.GetLogger(LoggerNames.SecurityDefLoggerName);
        public FixMessageFactory(FixParser fixParser)
        {
            _fixParser = fixParser;
        }

        public IMessage Create(FixMessage fixMessage)
        {
            var type = _fixParser.DetermineType(fixMessage.Message);
            if (type=="0")
            {
                _heartbeatLog.Debug("Received Heatbeat, " + fixMessage.Message);
                return _fixParser.HeartbeatMessage(fixMessage.Message);
            }
            if (type == "1")
            {
                _heartbeatLog.Debug("Received TestRequestMessage, " + fixMessage.Message);
                return _fixParser.TestReqMessage(fixMessage.Message);
            }
            if (type == "A")
            {
                _logonLog.Debug("Succesfull logon, " + fixMessage.Message);
                return _fixParser.LogonResponse(fixMessage.Message);
            }
            if (type == "5") 
            {
                _logonLog.Debug("Login failed, waiting 15 seconds for retry," + fixMessage.Message);
                Console.WriteLine("Login failed, waiting 15 seconds to try again");
                return _fixParser.Logout(fixMessage.Message);
            }
            if (type == "AE")
            {
                return _fixParser.TradeCapture(fixMessage.Message);
            }
            if (type == "d")
            {
                _securityLog.Debug("Received security definitions respons, " + fixMessage.Message);
                return _fixParser.SecurityDefinition(fixMessage.Message);
            }
            if (type == "UDS")
            {
                _securityLog.Debug("Received defined security strategy, " + fixMessage.Message);
                return _fixParser.SecurityDefinition(fixMessage.Message);
            }
            if (type == "UCR")
            {
                return _fixParser.UserCompanyReceiver(fixMessage.Message);
            }
            else
            {
                Debug.WriteLine(fixMessage);
            }
            return null;
        }


    }
}