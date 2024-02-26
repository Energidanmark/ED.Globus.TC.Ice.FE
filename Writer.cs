using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using System.Threading;
using ED.Atlas.Svc.TC.Ice.FE.Messages;

namespace ED.Atlas.Svc.TC.Ice.FE
{
    public class Writer
    {
        private readonly SslStream _sslStream;
        private readonly HeartBeatTimer _heartbeatTimer;
        private volatile int _seqNumber = 1;
        private readonly object _writerLock = new object();
        public Writer(SslStream sslStream, HeartBeatTimer heartbeatTimer)
        {
            _sslStream = sslStream;
            _heartbeatTimer = heartbeatTimer;

            _heartbeatTimer.Run();
            _heartbeatTimer.TimesUp += _heartbeatTimer_TimesUp;
        }

        public string Logon(string logonmessage)
        {
            ConsoleWriter.WriteSent(logonmessage);
            var byteArray = Encoding.ASCII.GetBytes(logonmessage);
            _sslStream.Write(byteArray, 0, byteArray.Length);
            var buffer = new byte[32000];
           
                Thread.Sleep(300);
                _sslStream.Read(buffer, 0, 32000);
           

            var returnMessage = Encoding.ASCII.GetString(buffer);
            var bar = returnMessage.TrimEnd(new char[] { (char)0 });
            bar = string.IsNullOrEmpty(bar) ? "Empty" : bar;
            _seqNumber++;
            return bar;
        }

        public void Run()
        {
            _heartbeatTimer.Run();
        }
        public void HandleMessage(List<OutgoingMessageFix44> outgoingMessages)
        {
            foreach (var message in outgoingMessages)
            {
                SendMessage(message, _sslStream);
            }

            outgoingMessages.Clear();
        }

        private void SendMessage(OutgoingMessageFix44 message, SslStream stream, bool readResponse = true)
        {
            lock (_writerLock)
            {
                ConsoleWriter.WriteSent($"Sending message -->> {message}");
                var byteArray = Encoding.ASCII.GetBytes(message.CreateFixMessage(_seqNumber));
                stream.Write(byteArray, 0, byteArray.Length);
              
                _seqNumber++;
            }
            
        }

        private void _heartbeatTimer_TimesUp(object sender, OutgoingMessageFix44 outgoingMessage)
        {
             SendMessage(outgoingMessage, _sslStream);
            _heartbeatTimer.Run();
        }
    }
}