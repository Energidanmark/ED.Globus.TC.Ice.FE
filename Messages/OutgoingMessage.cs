using System.Text;

namespace ED.Atlas.Svc.TC.Ice.FE.Messages
{
    public class OutgoingMessageFix44 : OutgoingMessage
    {
        public OutgoingMessageFix44(string body) : base("FIX.4.4")
        {
            Body = body;
        }

        public OutgoingMessageFix44(string body, string header) : base("FIX.4.4")
        {
            Body = body;
            Header = header;
        }
    }
    public abstract class OutgoingMessage 
    {
        public string Version { get; }
        protected OutgoingMessage(string version)
        {
            Version = version;
        }
        
        
        public string Body { get; set; }
        public string Header { get; set; }
        public string Trailer { get; set; }

        public string CreateFixMessage(int sequenceNumber)
        {
            string header = ConstructHeader(Header, Body, sequenceNumber);

            var trailer = ConstructTrailer(header + Body);

            //string header = ConstructHeader(Header, Body, sequenceNumber);

            //var headerBuilder = new StringBuilder(header);
            //string trailer1 = ConstructTrailer(headerBuilder.ToString());

            var fixMessage = header + Body + trailer;
            fixMessage = fixMessage.Replace("|", "\u0001");
            //string bar = @"8=FIX.4.4\u00019=119\u000135=AD\u000149=13379\u000156=ICE\u000134=2\u000152=20180115-10:21:31.847\u0001568=2563484\u0001569=0\u0001263=1\u0001580=1\u000175=20171102\u000160=20171102-00:00:00.000\u000110=148\u0001";
            //string foo =
            //    @"8=FIX.4.4\u00019=133\u000135=AD\u000149=13379\u000156=ICE\u000157=QUOTE\u000150=13379wwew\u000152=20180115-11:49:52\u000134=2\u0001263=1\u0001568=2563484\u0001569=0\u0001580=1\u000175=20180115\u000160=20180115-12:00:00\u000110=198\u0001";
            return fixMessage;
        }

        private string ConstructHeader(string header, string bodyMessage, int sequenceNumber)
        {

            var motherHeader = new StringBuilder($"8={Version}|");
            var sb = new StringBuilder(header);

            sb.Append($"34={sequenceNumber}|");
            
            
            var length = sb.ToString().Length + bodyMessage.Length;

            motherHeader.Append($"9={length}|");
            motherHeader.Append(sb);

            return motherHeader.ToString();
        }
        /// <summary>
        /// Header and body
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string ConstructTrailer(string headerAndBody)
        {
            //Three byte, simple checksum. Always last field in message; i.e. serves,
            //with the trailing<SOH>, 
            //as the end - of - message delimiter. Always defined as three characters
            //(and always unencrypted).
            var trailer = "10=" + CalculateChecksum(headerAndBody.Replace("|", "\u0001").ToString()).ToString().PadLeft(3, '0') + "|";
            return trailer;
        }

        private int CalculateChecksum(string dataToCalculate)
        {
            byte[] byteToCalculate = Encoding.ASCII.GetBytes(dataToCalculate);
            int checksum = 0;
            foreach (byte chData in byteToCalculate)
            {
                checksum += chData;
            }
            return checksum % 256;
        }

    }
}