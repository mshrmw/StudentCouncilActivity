//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StudentCouncilActivity
{
    using System;
    using System.Collections.Generic;
    
    public partial class Registrations
    {
        public int IDRegistration { get; set; }
        public int IDStudent { get; set; }
        public int IDTask { get; set; }
        public System.DateTime RegistrationDate { get; set; }
        public string RegistrationStatus { get; set; }
    
        public virtual EventTasks EventTasks { get; set; }
        public virtual Students Students { get; set; }
    }
}
