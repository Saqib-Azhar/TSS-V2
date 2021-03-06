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
    
    public partial class ContactConfirmation
    {
        public int Id { get; set; }
        public Nullable<int> ContactId { get; set; }
        public Nullable<int> ConfirmationStatusId { get; set; }
        public string ConfirmationToken { get; set; }
        public Nullable<System.DateTime> TokenCreationTime { get; set; }
        public Nullable<System.DateTime> TokenExpiryTime { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
    
        public virtual ContactConfirmationStatu ContactConfirmationStatu { get; set; }
        public virtual CustomerContact CustomerContact { get; set; }
    }
}
