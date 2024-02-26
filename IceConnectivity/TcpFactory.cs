using System.Net;
using System.Net.Sockets;

namespace ED.Atlas.Svc.TC.Ice.FE.IceConnectivity
{
    public class TcpFactory
    {
        private readonly string _host;
        private readonly int _port;

        public TcpFactory(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public TcpClient CreateTcpClient()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
            var tcpClient = new TcpClient(_host, _port);
            
            return tcpClient;
        }
    }
}