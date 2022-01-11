using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tic_Tac_Toc_Core.Models;

namespace Tic_Tac_Toe_Server.Models
{
    class Connected_Client
    {
        public Client_Model Client { get; set; }
        public TcpClient Tcp { get; set; }
        public string Code { get; set; }
        public bool Is_Creater { get; set; } = false;
    }
}
