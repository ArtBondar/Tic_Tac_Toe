//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tic_Tac_Toe_Server
{
    using System;
    using System.Collections.Generic;
    
    public partial class Users
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int Win_Count { get; set; }
        public int Lose_Count { get; set; }
        public int Draw_Count { get; set; }
    }
}
