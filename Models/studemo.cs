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
    
    public partial class studemo
    {
        public studemo()
        {
            this.stuscheds = new HashSet<stusched>();
            this.Users = new HashSet<User>();
        }
    
        public int suniq { get; set; }
        public string ident { get; set; }
        public Nullable<int> stuuniq { get; set; }
        public string namesfx { get; set; }
        public string nickname { get; set; }
        public string genderc { get; set; }
        public string ethnicc { get; set; }
        public System.DateTime birthdate { get; set; }
        public string birthplace { get; set; }
        public Nullable<System.DateTime> regdate { get; set; }
        public string ssn { get; set; }
        public Nullable<decimal> gradyear { get; set; }
        public string homelangc { get; set; }
        public string primlangc { get; set; }
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
        public string emailaddr { get; set; }
        public string url { get; set; }
        public string phnnumber { get; set; }
        public string phntypec { get; set; }
        public Nullable<decimal> phnunlist { get; set; }
        public Nullable<decimal> phnmsg { get; set; }
        public string bverbasc { get; set; }
        public string bverdocnum { get; set; }
        public string citizenc { get; set; }
        public string countryc { get; set; }
        public Nullable<System.DateTime> dUsEntry { get; set; }
        public string geocode { get; set; }
        public string resschoolc { get; set; }
        public string resdistc { get; set; }
        public string chcschoolc { get; set; }
        public string gradreqc { get; set; }
        public Nullable<int> careeruniq { get; set; }
        public Nullable<int> hlduniq { get; set; }
        public Nullable<int> buspuniq { get; set; }
        public Nullable<int> busduniq { get; set; }
        public string maritalc { get; set; }
        public string migrantnum { get; set; }
        public string legalbind { get; set; }
        public string passwd { get; set; }
        public Nullable<decimal> retainflag { get; set; }
        public Nullable<decimal> norank { get; set; }
        public Nullable<int> CurrentStudentRankID { get; set; }
        public string inforelc { get; set; }
        public Nullable<int> counsfuniq { get; set; }
        public string memberc { get; set; }
        public string chgby { get; set; }
        public Nullable<System.DateTime> chgdt { get; set; }
        public string stunotes { get; set; }
        public Nullable<System.DateTime> graddate { get; set; }
        public string compstatc { get; set; }
        public byte outofstate { get; set; }
        public Nullable<int> SSID { get; set; }
        public int SSIDChangeFlag { get; set; }
        public Nullable<System.DateTime> LastVerifiedOn { get; set; }
        public bool RaceObserverIdentified { get; set; }
        public Nullable<System.DateTime> CommunicationInfoChangedOn { get; set; }
        public Nullable<System.DateTime> USEnrolledDate { get; set; }
        public string PreferredFirstName { get; set; }
        public string PreferredMiddleName { get; set; }
        public string PreferredLastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string CensusNumber { get; set; }
        public string LegalFirstName { get; set; }
        public string LegalMiddleName { get; set; }
        public string LegalLastName { get; set; }
        public string LegalName { get; set; }
        public string LegalNameLastNameFirst { get; set; }
        public bool BirthCertificateVerified { get; set; }
        public bool ShotRecordsVerified { get; set; }
        public bool VisionScreeningVerified { get; set; }
        public bool UtahSchoolSystemVerified { get; set; }
        public bool SpecifyPreferredNames { get; set; }
        public bool IsConditionalAdmission { get; set; }
        public bool OverrideGraduationYear { get; set; }
    
        public virtual ICollection<stusched> stuscheds { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
