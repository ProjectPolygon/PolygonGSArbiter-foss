using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolygonGSArbiter
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleEx.WriteLine($"Access Key read: {Config.AccessKey}");
            ConsoleEx.WriteLine($"Current Access key: {Config.AccessKey}");

            ConsoleEx.WriteLine("Service starting...");

            Task.Run(() => JobManager.MonitorUnresponsiveJobs());
            Task.Run(() => WebManager.StartResourceReporter());

            WebManager.SetMarker(true);

            ConsoleEx.WriteLine("Initializing Project Polygon Arbiter Service");
            int ServicePort = ArbiterService.Start();
            ConsoleEx.WriteLine($"Service Started on port {ServicePort}");

            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("Service shutting down...");
                
                ArbiterService.Stop();
                WebManager.SetMarker(false);
                JobManager.CloseAllJobs();

                // wait for web requests to finish
                Thread.Sleep(10000);
            }; 

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }

    public class UnixTime
    {
        public static int GetTimestamp()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
