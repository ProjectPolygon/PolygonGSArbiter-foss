using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Net.Http;

namespace PolygonGSArbiter
{
    class WebManager
    {
        private static readonly HttpClient WebClient = new HttpClient();

        public static string GetGameserverScript(string JobID, int PlaceID, int Port)
        {
            return $"{Config.BaseUrlHTTP}/gameserver?jobId={JobID}&placeId={PlaceID}&port={Port}&maxPlayers=10&{Config.AccessKey}";
        }

        static int GetAvailableMemory()
        {
            PerformanceCounter performance = new PerformanceCounter("Memory", "Available MBytes");
            return (int)performance.NextValue();
        }

        static int GetCpuUsage()
        {
            PerformanceCounter performance = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            performance.NextValue(); // this is always gonna be zero
            Thread.Sleep(500);
            return (int)Math.Round(performance.NextValue());
        }

        public static void SetMarker(bool Online)
        {
            int OnlineInt = Online ? 1 : 0;
            WebClient.GetAsync($"{Config.BaseUrl}/set-marker?{Config.AccessKey}&GameserverID={Config.GameserverID}&Online={OnlineInt}");
        }

        public static void StartResourceReporter()
        {
            while (true)
            {
                string AvailableMemory = GetAvailableMemory().ToString();
                string CpuUsage = GetCpuUsage().ToString();

                Dictionary<string, string> FormData = new Dictionary<string, string>
                {
                    { "GameserverID", Config.GameserverID.ToString() },
                    { "CpuUsage", CpuUsage },
                    { "AvailableMemory", AvailableMemory },
                };

                FormUrlEncodedContent FormContent = new FormUrlEncodedContent(FormData);

                WebClient.PostAsync($"{Config.BaseUrl}/report-gameserver-resources?{Config.AccessKey}", FormContent);

                Thread.Sleep(60000);
            }
        }

        public static void UpdateJob(string JobID, string Status, int Port = 0)
        {
            if (Port == 0)
            {
                WebClient.GetAsync($"{Config.BaseUrl}/update-job?{Config.AccessKey}&JobID={JobID}&Status={Status}");
            }
            else
            {
                WebClient.GetAsync($"{Config.BaseUrl}/update-job?{Config.AccessKey}&JobID={JobID}&Status={Status}&MachineAddress={Config.MachineAddress}&ServerPort={Port}");

            }
        }
    }
}
