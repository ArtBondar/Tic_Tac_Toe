using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_Tac_Toc_Core.Models
{
    public class Request_Model
    {
        public string Method { get; set; }
        public object Data { get; set; }
    }
}
