using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ED.Atlas.Svc.TC.Ice.FE.Common;
using ED.Atlas.Svc.TC.Ice.FE.Heartbeats;
using ED.Atlas.Svc.TC.Ice.FE.Main;
using ED.Atlas.Svc.TC.Ice.FE.Messages;
using ED.Atlas.Svc.TC.Ice.FE.Trades;

namespace ED.Atlas.Svc.TC.Ice.FE.IceConnectivity
{
    public class FixClient
    {
        private  SslStream _sslStream;
        private readonly TcpFactory _tcpFactory;
        private readonly FixMessageConstructor _fixMessageConstructor;
        private readonly HeartBeatTimer _heartBeatTimer;

        private readonly object _writerLock = new object();
        private readonly object _readerLock = new object();
        private int _seqNumber = 1;
        private DateTime prevTimeStamp = DateTime.Now;
        const int BufferSize = 232000;
        private bool _retryingLogin = false;
        //const int BufferSize = int.MaxValue;
        private byte[] _buffer;
        public event EventHandler<RawMessage> MessageReceived;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public FixClient(TcpFactory tcpFactory, FixMessageConstructor fixMessageConstructor, HeartBeatTimer heartBeatTimer)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
            _tcpFactory = tcpFactory;
            
            _buffer = new byte[BufferSize];
          
            _fixMessageConstructor = fixMessageConstructor;
            _heartBeatTimer = heartBeatTimer;
            
        }

        private CancellationTokenSource _cancellationTokenSource;
        
        public void Logon()
        {
            _heartBeatTimer.TimesUp += _heartBeatTimer_TimesUp;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;

            //var tcpClient = new TcpClient("63.247.113.201", 433);
            _sslStream = new SslStream(_tcpFactory.CreateTcpClient().GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), SelectLocalCertificate);
            //_sslStream = new SslStream(tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), SelectLocalCertificate);
            //_sslStream.AuthenticateAsClient("63.247.113.201"); // Test
            _sslStream.AuthenticateAsClient("63.247.112.207"); // This is production

            if (!_sslStream.IsAuthenticated)
            {
                throw new ArgumentException("Ssl stream must be connected and authenticated before using the reader");
            }
            
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            Task.Factory.StartNew(BeginRead, token);

            try
            {
                SendMessage(_fixMessageConstructor.CreateLogonMessage(SessionQualifier.QUOTE, _heartBeatTimer.Duration, true),_retryingLogin);
            }
            catch (Exception e)
            {
                _log.Debug($"[FixClient] Failed to login. {e.Message}");
            }
            
            _heartBeatTimer.Run();
        }

        public void SendSubscriptionRequest()
        {
            SendMessage(_fixMessageConstructor.CreateCaptureRequest(SessionQualifier.QUOTE,DateTime.Today.AddDays(-2)), _retryingLogin);
        }

        public void Close()
        {
            // TODO [2018-02-12 Robert Nogal] : Close heartbeat timer, close readCallBack, detach timer eventcallback.
            _cancellationTokenSource.Cancel();
            _heartBeatTimer.TimesUp -= _heartBeatTimer_TimesUp;
            _sslStream.Close();
            _sslStream.Dispose();
            _heartBeatTimer.Stop();

        }
        public void BeginRead()
        {
            lock (_readerLock)
            {
                //_buffer = new byte[BufferSize];
                _sslStream.BeginRead(_buffer, 0, _buffer.Length, ReadCallBack, _sslStream);
            }
        }

        private StringBuilder _intermediateMessage = new StringBuilder();
       
        private Regex _messageParserRegex = new Regex("8=FIX\\.4\\.4.*?\u000110=\\d+\u0001");
        private Regex _testReqIdPRegex = new Regex("112=?.\u0001");

        private void _heartBeatTimer_TimesUp(object sender, OutgoingMessageFix44 heartbeatOutGoingMessage)
        {
            SendMessage(heartbeatOutGoingMessage, _retryingLogin);
        }
        
        private void ReadCallBack(IAsyncResult asyncResult)
        {
          
            var stream = (SslStream)asyncResult.AsyncState;
            Regex regex = new Regex(@"(?:112\=)(\d *)");
            int readCount = stream.EndRead(asyncResult);
            var rawMessage = Encoding.ASCII.GetString(_buffer).Substring(0, readCount);

            //var rawMessage = Encoding.Unicode.GetString(_buffer).Substring(0, readCount);



            //var matches123 = regex.Matches(rawMessage);
            //foreach (Match m in matches123)
            //{
            //    _seqNumber = Int32.Parse(m.Groups[1].Value);
            //}

            try
            {
                if (!string.IsNullOrEmpty(rawMessage))
                {
                    _intermediateMessage.Append(rawMessage);

                    var matches = _messageParserRegex.Matches(_intermediateMessage.ToString());
                    foreach (var match in matches)
                    {
                        ConsoleWriter.WriteReceived(match.ToString());
                        MessageReceived?.Invoke(this, new RawMessage(match.ToString()));
                        _intermediateMessage.Replace(match.ToString(), string.Empty);
                    }
                }

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    ConsoleWriter.Write("Readcallback cancelled");
                    _sslStream.Close();
                    return;
                }

                stream.BeginRead(_buffer, 0, _buffer.Length, ReadCallBack,
                    stream);
            }
            catch (Exception ex)
            {
                _log.Debug($"[FixClient] ReadCall Fallback failed. {ex.Message}");
            }

        }
        public void SendMessage(OutgoingMessageFix44 outgoingMessageFix44, bool retryingLogin)
        {
            lock (_readerLock)
            {
                var message = outgoingMessageFix44.CreateFixMessage(_seqNumber);
                ConsoleWriter.WriteSent(message);
                var byteArray = Encoding.ASCII.GetBytes(message);
                _sslStream.Write(byteArray, 0, byteArray.Length);
                _seqNumber++;
                //_sslStream.Flush();
            }
            //Read();
        }
        #region certificate code
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            ConsoleWriter.Write($"Certificate error: {sslPolicyErrors}");
            return true;
        }

        public static X509Certificate SelectLocalCertificate(
            object sender,
            string targetHost,
            X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate,
            string[] acceptableIssuers)
        {
            ConsoleWriter.Write("Client is selecting a local certificate.");
            if (acceptableIssuers != null && acceptableIssuers.Length > 0 && localCertificates != null && localCertificates.Count > 0)
            {
                // Use the first certificate that is from an acceptable issuer.
                foreach (X509Certificate certificate in localCertificates)
                {
                    string issuer = certificate.Issuer;
                    if (Array.IndexOf(acceptableIssuers, issuer) != -1)
                        return certificate;
                }
            }
            if (localCertificates != null &&
                localCertificates.Count > 0)
                return localCertificates[0];

            return null;
        }

        #endregion certificate code
    }
}