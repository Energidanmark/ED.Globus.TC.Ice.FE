using System;
using System.Text;
using ED.Atlas.Svc.TC.Ice.FE.IceConnectivity;
using ED.Atlas.Svc.TC.Ice.FE.Logging;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using log4net;

namespace ED.Atlas.Svc.TC.Ice.FE.Trades
{
    public class FixMessageConstructor
    {
        private string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly string _senderCompID;
        private readonly string _senderSubID;
        private readonly string _targetCompID;

        private static readonly log4net.ILog _securityLog = LogManager.GetLogger(LoggerNames.SecurityDefLoggerName);
        private static readonly log4net.ILog _heartbeatLog = LogManager.GetLogger(LoggerNames.HeartbeatLoggerName);

        public FixMessageConstructor(string username, string password, string senderCompID, string senderSubID, string targetCompID)
        {
            _username = username;
            _password = password;
            _senderCompID = senderCompID;
            _senderSubID = senderSubID;
            _targetCompID = targetCompID;
        }
        public OutgoingMessageFix44 CreateCompanyRequest(SessionQualifier sessionQualifier)
        {
            var header = CreateHeader(sessionQualifier, SessionMessageCode(SessionMessageType.UserCompanyRequest));
            return new OutgoingMessageFix44(body: string.Empty, header: header);
        }
        public OutgoingMessageFix44 CreateLogonMessage(SessionQualifier sessionQualifier, int heartbeatSeconds, bool resetSetNum)
        {
            var body = new StringBuilder();
            //Defines a message encryption scheme.Currently, only transportlevel security 
            //is supported.Valid value is "0"(zero) = NONE_OTHER (encryption is not used).
            body.Append(AddValue(98, "0"));
            //Heartbeat interval in seconds.
            //Value is set in the 'config.properties' file (client side) as 'SERVER.POLLING.INTERVAL'.
            //30 seconds is default interval value. If HeartBtInt is set to 0, no heart beat message 
            //is required.
            body.Append(AddValue(108, heartbeatSeconds.ToString()));
            // All sides of FIX session should have
            //sequence numbers reset. Valid value
            //is "Y" = Yes(reset).
            if (resetSetNum)
                body.Append(AddValue(141, "Y"));
            //The numeric User ID. User is linked to SenderCompID (#49) value (the
            //user’s organization).
            body.Append(AddValue(553, _username));
            //USer Password
            body.Append(AddValue(554, _password));

            var header = CreateHeader(sessionQualifier, SessionMessageCode(SessionMessageType.Logon));

            return new OutgoingMessageFix44(body: body.ToString(), header: header);
        }

        public OutgoingMessageFix44 CreateLogoutMessage(SessionQualifier sessionQualifier)
        {
            var header = CreateHeader(sessionQualifier, SessionMessageCode(SessionMessageType.Logout));
            return new OutgoingMessageFix44(string.Empty, header);
        }

        public OutgoingMessageFix44 CreateHeartBeatMessage(SessionQualifier sessionQualifier)
        {
            _heartbeatLog.Debug("Heartbeat is being send " + this);
            var header = CreateHeader(sessionQualifier, SessionMessageCode(SessionMessageType.Heartbeat));
            return new OutgoingMessageFix44(string.Empty, header);

        }

        /// <summary>
        /// TradeRequest is the id used for subscription.
        /// </summary>
        /// <param name="sessionQualifier"></param>
        /// <param name="uniqueTradeRequestId"></param>
        /// <returns></returns>
        public OutgoingMessageFix44 CreateCaptureRequest(SessionQualifier sessionQualifier, DateTime tradeTime)
        {
            // TODO [2018-02-20 Robert Nogal] : For live trades = 263 = 1, 580 = 0 and without tag60 and tag75.
            var body = new StringBuilder();
            var rnd = new Random();
            int uniqueID = rnd.Next(Int32.MaxValue);
            
            // This is historical trade request.
            body.Append(AddValue(263, "1")); // 1 for snapshot + updates
            body.Append(AddValue(568, uniqueID.ToString()));
            body.Append(AddValue(569, "0"));
            body.Append(AddValue(453, "1"));
            body.Append(AddValue(448, "13379"));
            body.Append(AddValue(447, "D"));
            body.Append(AddValue(452, "3"));
            body.Append(AddValue(580, "1")); 
            
            body.Append(AddValue(75, tradeTime.ToString("yyyyMMdd")));
            body.Append(AddValue(60, tradeTime.ToString("yyyyMMdd-HH:mm:ss.FFF")));

            var header = CreateHeader(SessionQualifier.QUOTE, SessionMessageCode(SessionMessageType.TradeCaptureReportRequest));

            return new OutgoingMessageFix44(body.ToString(), header);
        }

        public OutgoingMessageFix44 CreateSecurityDefinitionRequest(SessionQualifier sessionQualifier, string newTagValue, int uniqueRequestId, int securityId)
        {
            var body = new StringBuilder();

            _securityLog.Debug("Creating security definitoin request, " + this);
            body.Append(AddValue(48, securityId.ToString()));
            // Production
            //body.Append(AddValue(321, "3")); 
            body.Append(AddValue(321, newTagValue));
            body.Append(AddValue(320, uniqueRequestId.ToString()));

            var header = CreateHeader(SessionQualifier.QUOTE, SessionMessageCode(SessionMessageType.SecurityDefinitionRequest));

            return new OutgoingMessageFix44(body.ToString(), header);
        }

        public OutgoingMessageFix44 CreateTestReqId(SessionQualifier sessionQualifier, int testReqId)
        {
            var header = CreateHeader(sessionQualifier, SessionMessageCode(SessionMessageType.TestRequest));

            var body = new StringBuilder();
            _heartbeatLog.Info("Answering to TestRequest " + this);
            body.Append(AddValue(112, testReqId.ToString()));

            return new OutgoingMessageFix44(body.ToString(), header);
        }

        private string CreateHeader(SessionQualifier qualifier, string type)
        {
            var header = new StringBuilder();
            // Protocol version. FIX.4.4 (Always unencrypted, must be first field 
            // in message.

            var message = new StringBuilder();
            // Message type. Always unencrypted, must be third field in message.
            message.Append(AddValue(35, type));
            //message.Append(AddValue(35, type));
            // ID of the trading party in following format: <BrokerUID>.<Trader Login> 
            // where BrokerUID is provided by cTrader and Trader Login is numeric 
            // identifier of the trader account.
            message.Append(AddValue(49, _senderCompID));
            // Message target. Valid value is "CSERVER"
            message.Append(AddValue(56, _targetCompID));
            // Additional session qualifier. Possible values are: "QUOTE", "TRADE".
            message.Append(AddValue(57, qualifier.ToString())); // TODO [2018-01-15 Robert Nogal] : 
            // Assigned value used to identify specific message originator.
            message.Append(AddValue(50, _senderSubID)); // TODO [2018-01-15 Robert Nogal] : 
            // Message Sequence Number
            
            // Time of message transmission (always expressed in UTC(Universal Time 
            // Coordinated, also known as 'GMT').
            message.Append(AddValue(52, DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss")));
            
            header.Append(message);
            return header.ToString();
        }

        public static string AddValue(int id, string value)
        {
            return $"{id}={value}|";
        }
    
        private string SessionMessageCode(SessionMessageType type)
        {
            switch (type)
            {
                case SessionMessageType.Heartbeat:
                    return "0";
                    break;

                case SessionMessageType.Logon:
                    return "A";
                    break;

                case SessionMessageType.Logout:
                    return "5";
                    break;
                case SessionMessageType.SecurityDefinitionRequest:
                    return "c";
                    break;
                case SessionMessageType.UserCompanyRequest:
                    return "UCP";
                    break;

                case SessionMessageType.Reject:
                    return "3";
                    break;

                case SessionMessageType.Resend:
                    return "2";
                    break;

                case SessionMessageType.SequenceReset:
                    return "4";
                    break;

                case SessionMessageType.TestRequest:
                    return "1";
                    break;
                case SessionMessageType.TradeCaptureReportRequest:
                    return "AD";
                    break;

                default:
                    return "0";
            }
        }
        
    }
}