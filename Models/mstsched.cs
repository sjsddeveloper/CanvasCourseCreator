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
    
    public partial class mstsched
    {
        public mstsched()
        {
            this.mstmeets = new HashSet<mstmeet>();
        }
    
        public int mstuniq { get; set; }
        public int trkcrsuniq { get; set; }
        public decimal sectionn { get; set; }
        public decimal sectsize { get; set; }
        public Nullable<decimal> tasize { get; set; }
        public Nullable<int> funiq { get; set; }
        public Nullable<decimal> isschedule { get; set; }
        public Nullable<decimal> conflictok { get; set; }
        public Nullable<decimal> flag1 { get; set; }
        public Nullable<decimal> flag2 { get; set; }
        public Nullable<decimal> posttohist { get; set; }
        public Nullable<decimal> asgngrades { get; set; }
        public Nullable<decimal> varcr { get; set; }
        public Nullable<int> teamuniq { get; set; }
        public Nullable<decimal> scangrdbk { get; set; }
        public Nullable<decimal> scanrptcrd { get; set; }
        public Nullable<decimal> scanprgrpt { get; set; }
        public Nullable<decimal> snreserve1 { get; set; }
        public Nullable<decimal> snreserve2 { get; set; }
        public string markdefc { get; set; }
        public string mststatc { get; set; }
        public string wheredist { get; set; }
        public string whereschool { get; set; }
        public string creditfrom { get; set; }
        public string campus { get; set; }
        public string instructset { get; set; }
        public string collegecr { get; set; }
        public int iselementary { get; set; }
        public byte MarksPerClass { get; set; }
    
        public virtual facdemo facdemo { get; set; }
        public virtual ICollection<mstmeet> mstmeets { get; set; }
        public virtual trkcr trkcr { get; set; }
    }
}