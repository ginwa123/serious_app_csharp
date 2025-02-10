

// using System.Diagnostics;
// using System.Threading;

// namespace App.Utils;

// class MetricsCollector
// {
//     private readonly PerformanceCounter _cpuCounter;

//     public MetricsCollector()
//     {
//         _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
//     }

//     public void Start()
//     {
//         while (true)
//         {
//             var cpuUsage = _cpuCounter.NextValue();
//             var managedMemory = GC.GetTotalMemory(false);
//             var workingSet = Process.GetCurrentProcess().WorkingSet64;

//             Console.WriteLine($"CPU Usage: {cpuUsage:0.00}%");
//             Console.WriteLine($"Managed Memory: {managedMemory / 1024 / 1024} MB");
//             Console.WriteLine($"Working Set: {workingSet / 1024 / 1024} MB");

//             Thread.Sleep(1000); // Update every second
//         }
//     }
// }