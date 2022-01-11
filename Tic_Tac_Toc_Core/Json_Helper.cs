using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;

namespace Tic_Tac_Toc_Core
{
    static public class Json_Helper
    {
        static public string ToJson(this object data)
        {
            return JsonConvert.SerializeObject(data);
        }
        static public dynamic FromJson(this string data){
            return JsonConvert.DeserializeObject(data);
        }
    }
}
