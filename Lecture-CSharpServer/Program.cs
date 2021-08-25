using System;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace Lecture_CSharpServer
{
    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS ( Domain Name System )
            // Server Address   → Name Address
            // ex) 172.217.26.4 → www.google.com
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // ipAddr: 식당 주소,  7777: 식당 문 위치.

            _listener.Init(endPoint, () => { return new ClientSession(); });

            while (true)
            {
                ;
            }
        }
    }
}
