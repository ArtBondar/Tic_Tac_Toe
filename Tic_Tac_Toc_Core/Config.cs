using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_Tac_Toc_Core
{
    static public class Config
    {
        static public string AppKey => "asdfhjkqwruzxcn";
        static public byte[] AppKeyByte => Encoding.ASCII.GetBytes(AppKey);
    }
}
