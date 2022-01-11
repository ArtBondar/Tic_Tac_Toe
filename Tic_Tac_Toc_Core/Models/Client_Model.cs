using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_Tac_Toc_Core.Models
{
    public class Client_Model
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int Win_Count { get; set; }
        public int Lose_Count { get; set; }
        public int Draw_Count { get; set; }
    }
}
