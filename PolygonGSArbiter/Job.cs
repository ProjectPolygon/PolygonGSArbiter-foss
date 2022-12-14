using System;
using System.Diagnostics;

namespace PolygonGSArbiter
{
    public enum JobStatus
    {
        Pending = 0,
        Started = 1,
        Monitored = 2,
        Closed = 3,
        Crashed = 4,
    }

    public class Job
    {
        public string ID { get; set; }
        public int Version { get; set; }
        public int PlaceID { get; set; }
        public int Port { get; set; }
        public JobStatus Status { get; set; }
        public int TimeCreated { get; set; }
        public int TimeStarted { get; set; }
        public bool HasShutdown { get; set; }
        public Process Process { get; set; }

        public Job(string JobID, int Version, int PlaceID, int Port)
        {
            this.ID = JobID;
            this.Version = Version;
            this.PlaceID = PlaceID;
            this.Port = Port;
            this.Status = JobStatus.Pending;
            this.HasShutdown = false;
            this.TimeCreated = UnixTime.GetTimestamp();
        }

        internal void Start()
        {
            ConsoleEx.WriteLine($"[Job] Starting '{ID}' on port {Port}...", ConsoleColor.Yellow);

            // ExternalPortOffset is used for the connection port that is declared in the joinscript
            // the internal port is the port networkserver should listen on
           
            WebManager.UpdateJob(ID, "Loading", Port + Config.ExternalPortOffset);

            string GameserverScript = WebManager.GetGameserverScript(ID, PlaceID, Port);
            string[] CommandLine = JobManager.GetCommandLine(ID, Version, GameserverScript);

            Process = new Process();
            Process.StartInfo.FileName = CommandLine[0];
            Process.StartInfo.Arguments = CommandLine[1];
            Process.Start();
            Process.WaitForInputIdle();

            Status = JobStatus.Started;
            this.TimeStarted = UnixTime.GetTimestamp();

            ConsoleEx.WriteLine($"[Job] Started '{ID}'!", ConsoleColor.Green);
        }

        internal void Close()
        {
            try
            {
                if (HasShutdown || Process.HasExited)
                {
                    if (HasShutdown) Process.CloseMainWindow(); 
                    Process.Close();
                    ConsoleEx.WriteLine($"[Job] Closed '{ID}'! (Job closed by game:Shutdown())", ConsoleColor.Green);
                    WebManager.UpdateJob(ID, "Closed");
                }
                else if (Status == JobStatus.Crashed || !Process.Responding)
                {
                    Process.Kill();
                    Process.Close();
                    ConsoleEx.WriteLine($"[Job] Closed '{ID}'! (Job encountered a soft crash)", ConsoleColor.Red);
                    WebManager.UpdateJob(ID, "Crashed");
                }
                else
                {
                    Process.CloseMainWindow();
                    Process.Close();
                    ConsoleEx.WriteLine($"[Job] Closed '{ID}'! (Job closed by request)", ConsoleColor.Green);
                    WebManager.UpdateJob(ID, "Closed");
                }
            }
            catch (InvalidOperationException)
            {
                ConsoleEx.WriteLine($"[Job] Closed '{ID}'! (Job encountered a hard crash)", ConsoleColor.Red);
                WebManager.UpdateJob(ID, "Crashed");
            }
        }
    }
}
