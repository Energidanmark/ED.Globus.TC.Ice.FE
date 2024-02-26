using System;
using System.Collections.Generic;
using System.Diagnostics;
using ED.Atlas.Svc.TC.Ice.FE.Handlers;
using ED.Atlas.Svc.TC.Ice.FE.Heartbeats;
using ED.Atlas.Svc.TC.Ice.FE.IceConnectivity;
using ED.Atlas.Svc.TC.Ice.FE.Logging;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using ED.Atlas.Svc.TC.Ice.FE.SecurityDefinitions;
using ED.Atlas.Svc.TC.Ice.FE.TestReqIdMessages;
using ED.Atlas.Svc.TC.Ice.FE.Trades;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using log4net;

namespace ED.Atlas.Svc.TC.Ice.FE.Main
{
    public class Runner
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(
            LoggerNames.Ice);
        private FixClient _fixClient;
        private readonly FixMessageConstructor _fixMessageConstructor;
        private readonly IceDomService _iceDomService;
        private readonly List<string> _ignoreTradesByPriceBasis;
        private readonly IBusService _busService;
        private readonly SecurityDefinitionParser _securityDefinitionParser;
        private readonly TradeCaptureReportMapping _tradeCaptureReportMapping;
        private readonly bool retryingLogin = false;
        private readonly List<IHandle> _handlers = new List<IHandle>();
        public List<OutgoingMessageFix44> OutgoingMessages { get; } = new List<OutgoingMessageFix44>();
        // TODO [2018-02-22 Robert Nogal] : Internal messages? instead of topic, use interface type?
        
        private readonly List<string> _tradeCaputreReports = new List<string>();
        private Dictionary<string, SecurityDefinition> _securityDefinitions = new Dictionary<string, SecurityDefinition>();
        private static readonly log4net.ILog _securityLog = LogManager.GetLogger(LoggerNames.SecurityDefLoggerName);
        private static readonly log4net.ILog _iceLog = log4net.LogManager.GetLogger(
            LoggerNames.Ice);

        public Runner(FixClient fixClient, 
            FixMessageConstructor fixMessageConstructor, 
            TradeCaptureReportMapping tradeCaptureReportMapping, 
            IBusService busService, 
            SecurityDefinitionParser securityDefinitionParser,
            IceDomService iceDomService,
            List<string> ignoreTradesByPriceBasis)
        {
            _fixClient = fixClient;
            _fixMessageConstructor = fixMessageConstructor;
            _iceDomService = iceDomService;
            _ignoreTradesByPriceBasis = ignoreTradesByPriceBasis;
            _busService = busService;
            _securityDefinitionParser = securityDefinitionParser;
            _tradeCaptureReportMapping = tradeCaptureReportMapping;
            TryLogin();
        }

        private void AddHandlers(FixMessageConstructor fixMessageConstructor, SecurityDefinitionParser securityDefinitionParser, TradeCaptureReportMapping tradeCaptureReportMapping, IBusService busService, FixClient fixClient)
        {
            _handlers.Add(new HeartbeatHandler());
            _handlers.Add(new TestReqIdHandler(fixMessageConstructor, OutgoingMessages));
            _handlers.Add(new LogonHandler(fixMessageConstructor, OutgoingMessages)); // TODO [2018-02-22 Robert Nogal] : The outgoingMessages is added so the logonHandler can put in SecurityDefinitions. Do it another way?
            _handlers.Add(new TradeCaptureReportHandler(_tradeCaputreReports, tradeCaptureReportMapping, busService, _securityDefinitions, _iceDomService, _ignoreTradesByPriceBasis));
            _handlers.Add(new SecurifyDefintionReceiverHandler(securityDefinitionParser, _securityDefinitions));
            _handlers.Add(new LogoutHandler(fixClient));
            _handlers.Add(new MessageLoggingHandler());
        }

        public void TryLogin()
        {
            try
            {
                AddHandlers(_fixMessageConstructor, _securityDefinitionParser, _tradeCaptureReportMapping, _busService,
                    _fixClient);
                _fixClient.MessageReceived += _fixClient_MessageReceived;

                _fixClient.Logon();
                AddSecurityDefinitionRequest();
                AddTradeCaptureRequest();
                _fixClient.SendSubscriptionRequest();
            }
            catch (Exception ex)
            {
                _log.Debug($"Failed TryLogin(). Ex: {ex.Message}");
            }

        }

        public DateTime GetTimeStamp()
        {
            DateTime time = Convert.ToDateTime(DateTime.Now);
            return time;
        }

        public List<string> GetTrades()
        {
            return _tradeCaputreReports;
            
        }
        public void LogonToIce()
        {
            _fixClient.Logon();
        }

        private FixClient CreateNewFixClient()
        {
            _fixClient.MessageReceived -= _fixClient_MessageReceived;

            FixClient fixClient = null;
            try
            {
                var appSetting = new AppSettings();
                var port = appSetting.IcePort;
                var host = appSetting.IceIp;
                var factory = new TcpFactory(host, port);
                fixClient = new FixClient(factory, _fixMessageConstructor,
                    new HeartBeatTimer(_fixMessageConstructor, 25));


                return fixClient;
                
            }
            catch(Exception ex)
            {
                _log.Debug($"Failed to create new fix client. Ex: {ex.Message}");
            }
            
            return fixClient;
        }

        private void _fixClient_MessageReceived(object sender, RawMessage rawMessage)
        {
            HandleRawMessage(rawMessage);
            HandleMessages();
        }

        public void Close()
        {
            _tradeCaputreReports.Clear();
            _fixClient.MessageReceived -= _fixClient_MessageReceived;
            _fixClient.Close();
            
        }

        private void HandleRawMessage(RawMessage rawMessage)
        {
            var tradeCaptures = new BlockingCollection<string>();
            var fixParser = new FixParser(tradeCaptures);
            var fixMessages = rawMessage.ParseIntoMessages();
            var factory = new FixMessageFactory(fixParser);

            while (fixMessages.Count != 0)
            {
                var fixMessage = fixMessages.Take();
                var message = factory.Create(fixMessage);

                foreach (var handler in _handlers)
                {
                    handler.Handle(message);
                }

                if (message != null)
                {
                    var namedMessageType = message.GetType().Name;
                    if (namedMessageType.Equals("Logout"))
                    {
                        Close();
                        //Thread.Sleep(20000); //20 seconds timeout
                        //_fixClient = CreateNewFixClient();
                        //AddHandlers(_fixMessageConstructor, _securityDefinitionParser, _tradeCaptureReportMapping, _busService,
                        //    _fixClient);
                        _fixClient.MessageReceived += _fixClient_MessageReceived;

                        //_fixClient.Logon();
                        //AddSecurityDefinitionRequest();
                        //AddTradeCaptureRequest();
                        return;
                    }
                }
            }

            if (TimeToReset())
            {
                try
                {
                    AddLogoutRequest();
                    Thread.Sleep(60000); //20 seconds timeout

                    TimeSpan ts = new TimeSpan(4, 0, 0);
                    var t = new System.Threading.Timer(o =>
                    {
                        _fixClient = CreateNewFixClient();
                        AddHandlers(_fixMessageConstructor, _securityDefinitionParser, _tradeCaptureReportMapping,
                            _busService,
                            _fixClient);
                        _fixClient.MessageReceived += _fixClient_MessageReceived;

                        _fixClient.Logon();
                        AddSecurityDefinitionRequest();
                        //Thread.Sleep(10000);
                        AddTradeCaptureRequest();

                    }, null, TimeSpan.Zero, ts);
                }
                catch (Exception ex)
                {
                    _iceLog.Debug($"TimeToReset failed: {ex.Message}");
                }
            }
        }

        private void HandleMessages()
        {
            foreach (var outgoingMessage in OutgoingMessages)
            {
                _fixClient.SendMessage(outgoingMessage,retryingLogin);
            }

            OutgoingMessages.Clear();
        }
        private volatile bool _isLogoutRequestSent = false;
        private bool TimeToReset()
        {
            var newTimeStamp = DateTime.Now;
            if (newTimeStamp.Hour == 21 && newTimeStamp.Minute == 30)
            {
                return true;
            }
            return false;
        }

        public void AddTradeCaptureRequest()
        {
            Debug.WriteLine("AddTradeCaptureRequest...");
            OutgoingMessages.Add(_fixMessageConstructor.CreateCaptureRequest(SessionQualifier.QUOTE, DateTime.Today.AddDays(-2)));
            HandleMessages();
        }

        public void AddLogoutRequest()
        {
            Debug.WriteLine("AddLogoutRequest...");
            OutgoingMessages.Add(_fixMessageConstructor.CreateLogoutMessage(SessionQualifier.QUOTE));
            HandleMessages();
            Close();
        }
      

        public void AddSecurityDefinitionRequest()
        {
            Debug.WriteLine("AddSecurityDefinitionRequest...");
            var random = new Random();
            var  randomId = random.Next(0, 1000);
            
            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, 223)); // 5 UKA
            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "101", randomId, 223)); // 5 UKA
            
            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, 224)); // 5 UKA
            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "101", randomId, 224)); // 5 UKA

            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, 226)); // 5 UKA
            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "101", randomId, 226)); // 5 UKA


            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, 5)); // 5 IPE Brent Futures.
            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, 9)); // 9 ICE WTI Crude Futures.
            OutgoingMessages.Add(_fixMessageConstructor.CreateHeartBeatMessage(SessionQualifier.QUOTE));
            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "101", randomId, 17)); // 17 for ICE ROtterdam Coal Futures
            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, 17)); // 17 for ICE ROtterdam Coal Futures
                                                                                                                                        // Task: disable gas.
                                                                                                                                        //OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, 3)); // 3 IPE Natural Gas Futures
                                                                                                                                        //OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, 48)); // 48 for Dutch TTF Gas
                                                                                                                                        //OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "101", randomId, 48)); // 48 for Dutch TTF Gas
                                                                                                                                        //OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, 44)); // 48 for Henry Hub Gas
                                                                                                                                        //OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "101", randomId, 44)); // 48 for Henry Hub Gas

            OutgoingMessages.Add(_fixMessageConstructor.CreateHeartBeatMessage(SessionQualifier.QUOTE));
            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, 10)); // 10 for Newcastle coal
            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "101", randomId, 10)); // 10 for Newcastle coal

            OutgoingMessages.Add(_fixMessageConstructor.CreateSecurityDefinitionRequest(SessionQualifier.QUOTE, "3", randomId, securityId: 69)); // 69 For physical environmental



            HandleMessages();

        }
        public void AddHeartBeatRequest()
        {
            Debug.WriteLine("AddHeartBeatRequest...");
            OutgoingMessages.Add(_fixMessageConstructor.CreateHeartBeatMessage(SessionQualifier.QUOTE));
            HandleMessages();
        }

        public void EmptyOutgoing()
        {
            HandleMessages();
        }


        public void AddCompanyRequest()
        {
            OutgoingMessages.Add(_fixMessageConstructor.CreateCompanyRequest(SessionQualifier.QUOTE));
        }

        public void SendSubscriptionRequest()
        {
            _log.Debug("SendSubscriptionRequest called");
            _fixClient.SendSubscriptionRequest();
        }
    }
}