using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Tic_Tac_Toc_Core;
using Serilog;
using Tic_Tac_Toe_Server.Services;

namespace Tic_Tac_Toe_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            Tcp_Server.Run();
        }
    }
}
