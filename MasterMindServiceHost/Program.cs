using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using MasterMindLibrary;
using System.Net;
using System.Net.Sockets;

namespace CardsServiceHost
{
    class Program
    {
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "";
        }

        public static string GetExternalIPAddress()
        {
            return new WebClient().DownloadString("http://icanhazip.com");
        }

        static void Main(string[] args)
        {
            ServiceHost servHost = null;

            try
            {
                servHost = new ServiceHost(typeof(CodeMaker));

                // Run the service
                servHost.Open();
                Console.WriteLine("Local IP: {0} | External IP: {1}", GetLocalIPAddress(), GetExternalIPAddress());
                Console.WriteLine("Service started. Press any key to quit.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Wait for a keystroke
                Console.ReadKey();
                if (servHost != null)
                    servHost.Close();
            }
        }
    }
}
