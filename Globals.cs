using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CanvasCourseCreator
{
    public static class Globals
    {
        //the url for your canvas instance api something like http://district.instructure.com/api/v1/ please include the /api/v1/ portion of the url
        public const string CANVAS_URL = "YourUrlHere";
        //put your canvas api key here, this should be a key that will have rights to create courses, sections, users and enrollments
        public const string API_KEY = "YourAPIKeyHere";
        //email addresses of those that will recieve emails error emails as well as creation details emails
        public static readonly string[] EMAIL_ADDRESSES = {"list your email addresses here comma separated","second email here","third email here"};

        //schools and accounts
        /*
         * Should look like this where 708 is the schoolc from sis school table (the school number) and 90289 is the account id of that schools sub account
         * { "708", 90289 },
            { "704", 90290 },            
            { "148", 103474 }
         * 
         */
        public static readonly Dictionary<string, int> accountIDs = new Dictionary<string, int>()
                    { 
                        {"SchoolId", accountId},
                        {"SchoolId", accountId}
                    };
    }
}