using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PolygonGSArbiter
{
    class ArbiterService
    {
        private static TcpListener Service = new TcpListener(IPAddress.Any, Config.ServicePort);

        public static int Start()
        {
            Service.Start();
            Task.Run(() => ListenForConnections());
            return Config.ServicePort;
        }

        public static void Stop()
        {
            Service.Stop();
        }

        private static void ListenForConnections()
        {
            while (true)
            {
                TcpClient Client = Service.AcceptTcpClient();
                string ClientAddress = GetClientAddress(Client);
                //ConsoleEx.WriteLine($"[ArbiterService] Service received a new connection from '{ClientAddress}'", ConsoleColor.Blue);

                Task.Run(() => HandleConnection(Client, ClientAddress));
            }
        }

        private static void HandleConnection(TcpClient Client, string ClientAddress)
        {
            NetworkStream Stream = Client.GetStream();
            StreamReader Reader = new StreamReader(Stream);

            try
            {
                while (Client.Connected)
                {
                    string IncomingCommand = Reader.ReadLine();

                    if (IncomingCommand == null)
                    {
                        //ConsoleEx.WriteLine($"[ArbiterService/{ClientAddress}] Client disconnected", ConsoleColor.Blue);
                        Stream.Close();
                        Client.Close();
                    }
                    else
                    {
                        //ConsoleEx.WriteLine($"[ArbiterService/{ClientAddress}] Received command '{IncomingCommand}'", ConsoleColor.Blue);
                        ProcessCommand(Stream, ClientAddress, IncomingCommand);
                    }
                }
            }
            catch (IOException)
            {
                //ConsoleEx.WriteLine($"$[ArbiterService/{ClientAddress}] Client disconnected", ConsoleColor.Blue);
            }
        }

        private static void WriteToClient(NetworkStream Stream, string ClientAddress, string Response)
        {
            //ConsoleEx.WriteLine($"[ArbiterService/{ClientAddress}] Writing to client with '{Response}'", ConsoleColor.Blue);
            StreamWriter Writer = new StreamWriter(Stream);
            Writer.WriteLine(Response);
            Writer.Flush();
        }

        private static void ProcessCommand(NetworkStream Stream, string ClientAddress, string Message)
        {
            PolygonGSP Request = JsonConvert.DeserializeObject<PolygonGSP>(Message);

            switch (Request.Operation)
            {
                case "OpenJob":
                    if (JobManager.GetJobFromID(Request.JobID) != null)
                    {
                        WriteToClient(Stream, ClientAddress, "{\"Operation\":\"CloseJob\", \"Status\":\"Error\", \"Message\":\"Job already exists\"}");
                    }
                    else
                    {
                        Task.Run(() => JobManager.OpenJob(Request.JobID, Request.Version, Request.PlaceID));
                        WriteToClient(Stream, ClientAddress, "{\"Operation\":\"OpenJob\", \"Status\":\"OK\"}");
                    }

                    break;

                case "CloseJob":
                    if (JobManager.GetJobFromID(Request.JobID) == null)
                    {
                        WriteToClient(Stream, ClientAddress, "{\"Operation\":\"CloseJob\", \"Status\":\"Error\", \"Message\":\"Job does not exist\"}");
                    }
                    else
                    {
                        Task.Run(() => JobManager.CloseJob(Request.JobID));
                        WriteToClient(Stream, ClientAddress, "{\"Operation\":\"CloseJob\", \"Status\":\"OK\"}");
                    }

                    break;

                default:
                    ConsoleEx.WriteLine($"[ArbiterService/{ClientAddress}] Invalid command received", ConsoleColor.Blue);
                    break;
            }
        }

        private static string GetClientAddress(TcpClient Client)
        {
            return Client.Client.RemoteEndPoint.ToString();
        }
    }
}
