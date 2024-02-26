using System;

namespace ED.Atlas.Svc.TC.Ice.FE.Common
{
    public static class ConsoleWriter
    {
        public static void Write(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}{Environment.NewLine}");
        }

        public static void WriteReceived(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] Received <<-- : {message}{Environment.NewLine}");
        }
        public static void WriteSent(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] Sending -->> : {message}{Environment.NewLine}");
        }
    }
}