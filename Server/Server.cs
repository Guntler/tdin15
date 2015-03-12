using System;
using System.Runtime.Remoting;


/// <summary>
/// Server setup and execution as is from the teacher's example
/// </summary>
class Server
{

    static void Main(string[] args)
    {
        RemotingConfiguration.Configure("Server.exe.config", false);
        Console.WriteLine("Press Return to terminate.");
        Console.ReadLine();
    }
}