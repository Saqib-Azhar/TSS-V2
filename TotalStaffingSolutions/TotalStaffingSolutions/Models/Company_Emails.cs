//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TotalStaffingSolutions.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Company_Emails
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public Nullable<int> Company_id { get; set; }
        public System.DateTime Created_at { get; set; }
        public System.DateTime Updated_at { get; set; }
    
        public virtual Company Company { get; set; }
    }
}
