using System.Net.Security;
using System.Text;
using ED.Atlas.Svc.TC.Ice.FE.Trades;

namespace ED.Atlas.Svc.TC.Ice.FE.IceConnectivity
{
    public class Client
    {
        private readonly SslStream _sslStream;
        private readonly MessageConstructor _messageConstructor;
        private int _seqNumber = 1;
        //public Client(SslStream sslStream, MessageConstructor messageConstructor)
        //{
        //    _sslStream = sslStream;
        //    _messageConstructor = messageConstructor;
        //}

        //public void SendMessage(string message, SslStream stream, bool readResponse = true)
        //{
        //    var byteArray = Encoding.ASCII.GetBytes(message);
        //    stream.Write(byteArray, 0, byteArray.Length);
        //    //var buffer = new byte[1024];
        //    //if (readResponse)
        //    //{
        //    //    Thread.Sleep(100);
        //    //    stream.Read(buffer, 0, 1024);
        //    //}
        //    _seqNumber++;

        //    //var returnMessage = Encoding.ASCII.GetString(buffer);
            
        //    //return new Message(_seqNumber, message, returnMessage);
        //}
        //public void SendLogonMessage()
        //{
        //    var message = _messageConstructor.LogonMessage(SessionQualifier.QUOTE, _seqNumber, 2, false);
        //    SendMessage(message, _sslStream, true);
        //}

        //public void SendLogoutMessage()
        //{
        //    var message = _messageConstructor.LogoutMessage(SessionQualifier.QUOTE, _seqNumber);
        //    SendMessage(message, _sslStream);
        //}

    }
}