//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CanvasCourseCreator.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class facdemo
    {
        public facdemo()
        {
            this.facstats = new HashSet<facstat>();
            this.mstscheds = new HashSet<mstsched>();
            this.Users = new HashSet<User>();
        }
    
        public int funiq { get; set; }
        public Nullable<int> facuniq { get; set; }
        public string ident { get; set; }
        public string title { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public Nullable<System.DateTime> birthdate { get; set; }
        public Nullable<System.DateTime> hiredate { get; set; }
        public string ssn { get; set; }
        public string genderc { get; set; }
        public string ethnicc { get; set; }
        public string citizenc { get; set; }
        public string countryc { get; set; }
        public string maritalc { get; set; }
        public string passwd { get; set; }
        public string homeaddr1 { get; set; }
        public string homeaddr2 { get; set; }
        public string homecity { get; set; }
        public string homestate { get; set; }
        public string homezip { get; set; }
        public string mailaddr1 { get; set; }
        public string mailaddr2 { get; set; }
        public string mailcity { get; set; }
        public string mailstate { get; set; }
        public string mailzip { get; set; }
        public Nullable<decimal> unlistaddr { get; set; }
        public string emailaddr { get; set; }
        public string url { get; set; }
        public string edulvlc { get; set; }
        public Nullable<decimal> addhours { get; set; }
        public string barunitc { get; set; }
        public Nullable<System.DateTime> sndate { get; set; }
        public Nullable<decimal> autodate { get; set; }
        public Nullable<decimal> sysadmin { get; set; }
        public bool SensitiveData { get; set; }
        public Nullable<decimal> facdemo1 { get; set; }
        public Nullable<decimal> facdemo2 { get; set; }
        public Nullable<decimal> facdemo3 { get; set; }
        public Nullable<decimal> facdemo4 { get; set; }
        public Nullable<decimal> facdemo5 { get; set; }
        public Nullable<decimal> facdemo6 { get; set; }
        public string facnotes { get; set; }
    
        public virtual facstat facstat { get; set; }
        public virtual ICollection<facstat> facstats { get; set; }
        public virtual ICollection<mstsched> mstscheds { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
