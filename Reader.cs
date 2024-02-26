using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE
{
    public class Reader
    {
        private SslStream _sslStream;

        public Reader(SslStream sslStream)
        {
            if (!sslStream.IsAuthenticated)
            {
                throw new ArgumentException("Ssl stream must be connected and authenticated before using the reader");
            }

            _sslStream = sslStream;
         }

        public void BeginRead()
        {
            _buffer = new byte[32000];
            _sslStream.BeginRead(_buffer, 0, _buffer.Length, ReadCallBack, _sslStream);
        }

        public async Task ReadTasks()
        {
            while (true)
            {
                int byteCount = await _sslStream.ReadAsync(_buffer, 0, _buffer.Length, new CancellationToken());

                if (byteCount > 0)
                {
                    _sslStream.Read(_buffer, 0, 32000);
                    var foo = Encoding.ASCII.GetString(_buffer);

                    string bar = foo.TrimEnd(new char[] {(char) 0});
                    ConsoleWriter.Write("Data : " + bar);
                }

                _buffer = new byte[32000];
            }
        }

        private byte[] _buffer;
        public event EventHandler<RawMessage> MessageReceived;
        private void ReadCallBack(IAsyncResult asyncResult)
        {
            var stream = (SslStream)asyncResult.AsyncState;

            stream.EndRead(asyncResult);

            var rawReturnMessage = Encoding.ASCII.GetString(_buffer);
            string returnMessage = rawReturnMessage.TrimEnd(new char[] { (char)0 });
            if (!string.IsNullOrEmpty(returnMessage))
            {
                MessageReceived?.Invoke(this, new RawMessage(returnMessage));
            }


            _buffer = new byte[32000];

            stream.Flush();
            stream.BeginRead(_buffer, 0, _buffer.Length, ReadCallBack,
                    stream);
            
        }

        protected virtual void OnMessageReceived(RawMessage e)
        {
            MessageReceived?.Invoke(this, e);
        }
    }

    public class SslConnection
    {
        private TcpClient _tcpClient;
        private SslStream _sslStream;
        private string _host;
        private readonly Client _client;

        private int _seqNumber = 1;
        private int _testReqId = 1;
        public SslConnection(string host, int port)
        {

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
            _host = host;

            _tcpClient = new TcpClient(_host, port);
            _sslStream = new SslStream(_tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
            _sslStream.AuthenticateAsClient(host);
        }

     
        public SslStream GetStream()
        {
            return _sslStream;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }

    
}