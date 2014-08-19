using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CanvasCourseCreator.Models;
using System.Net;
using System.IO;
using Microsoft.Data.OData.Query.SemanticAst;
using Newtonsoft.Json;

namespace CanvasCourseCreator.Controllers
{
    public class HomeController : Controller
    {
        private readonly SJSDDBEntities SISdb = new SJSDDBEntities();
        private const string OAuthKey = Globals.API_KEY;
        private const string CanvasUrl = Globals.CANVAS_URL;
        private static readonly Dictionary<string, int> AccountIDs = Globals.accountIDs;

        public ActionResult Index()
        {
            //calculate the school year
            int schoolYear = Convert.ToInt32(SISdb.schools.Max(s => s.schyear));

            var vSchools = from s in SISdb.schools
                           where s.schyear == schoolYear
                           where AccountIDs.Keys.Contains(s.schoolc)
                           orderby s.schname
                           select s
                           ;


            ViewBag.Schools = vSchools.ToList();
            return View();
        }

        public JsonResult GetCoursesBySchoolCode(int schuniq)
        {
            //calculate the school year
            int schoolYear = Convert.ToInt32(SISdb.schools.Max(s => s.schyear));

            school school = SISdb.schools.Find(schuniq);

            var vSchedules = from c in SISdb.courses
                           join tc in SISdb.trkcrs on c.crsuniq equals tc.crsuniq
                           join tr in SISdb.tracks on tc.trkuniq equals tr.trkuniq
                           join ms in SISdb.mstscheds on tc.trkcrsuniq equals ms.trkcrsuniq
                           join fd in SISdb.facdemoes on ms.funiq equals fd.funiq
                           where tr.schoolc == school.schoolc
                           where tr.schyear == schoolYear
                           where !fd.lastname.Contains("College")
                           where !fd.lastname.Contains("Admin")
                           orderby c.descript
                           select ms;

            List<mstsched> schedules = vSchedules.ToList();
            List<object> courseInfoes = new List<object>();

            foreach (mstsched schedule in schedules)
            {
                course course = schedule.trkcr.course;

                object courseInfo = new { text = course.descript + " : " + schedule.facdemo.firstname + " " + schedule.facdemo.lastname , value = course.crsuniq + ":" + schedule.facdemo.funiq};
                courseInfoes.Add(courseInfo);
            }

            return Json(new { success = "true", courses = courseInfoes.Distinct() });
            
        }

        public JsonResult CreateCourse(int crsuniq, int funiq, int schuniq)
        {
            course sisCourse = SISdb.courses.Find(crsuniq);
            school sisSchool = SISdb.schools.Find(schuniq);
            facdemo courseTeacher = SISdb.facdemoes.Find(funiq);
            int schoolYear = Convert.ToInt32(SISdb.schools.Max(s => s.schyear));
            string sisCourseId = schoolYear + ":" + crsuniq + ":" + funiq;

            //get the account id
            int accountId;
            AccountIDs.TryGetValue(sisSchool.schoolc, out accountId);

            //make sure the course doesn't exist
            CanvasCourseModel vCourse = GetCourse(crsuniq, funiq, schuniq);
            if (vCourse != null)
            {
                //the course exists return the id
                return Json(new
                {
                    success = "true",
                    courseId = vCourse.id
                });
            }

            CanvasCourseModel vSisCourse = GetCourseBySISId(sisCourseId);
            if (vSisCourse != null)
            {
                //the course exists return the id
                return Json(new
                {
                    success = "true",
                    courseId = vSisCourse.id
                });
            }


            string courseJson = "{ \"account_id\": \"" + accountId + "\"," +
                                "\"course\": {" +
                                    "\"name\": \"" + sisCourse.descript + " : " + courseTeacher.lastname.Trim() + " " + courseTeacher.firstname.Trim() + "\"," +
                                    "\"course_code\": \"" + sisCourse.descript + " : " + courseTeacher.lastname.Trim() + " " + courseTeacher.firstname.Trim() + "\"," +
                                    "\"sis_course_id\": \"" + sisCourseId + "\"}}";
       

            //send the data
            string createCourseURL = CanvasUrl + "accounts/" + accountId + "/courses?access_token=" + OAuthKey;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(createCourseURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            CanvasCourseModel newCourse = new CanvasCourseModel();

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                
                streamWriter.Write(courseJson);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var serializer = new JsonSerializer();
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        newCourse = serializer.Deserialize<CanvasCourseModel>(jsonTextReader);
                    }
                }

            }

            //attach the teacher
            AssignTeacherToCourse(funiq, newCourse.id, schuniq);

            return Json(new
            {
                success = "true",
                courseId = newCourse.id
            });

        }

        private void AssignTeacherToCourse(int funiq, string courseId, int schuniq)
        {
            User facUser = SISdb.Users.FirstOrDefault(u => u.funiq == funiq);
            CanvasUserModel teacher = GetUserBySISId(facUser.Username);
            if (teacher == null)
            {
                teacher = CreateCanvasTeacher(funiq, schuniq);
            }

            string enrollmentJson = "{\"enrollment\": {" +
                                    "\"user_id\": \"" + teacher.id + "\"," +
                                    "\"type\": \"TeacherEnrollment\"," +
                                    "\"enrollment_state\": \"active\"," +
                //when we go live this should be true.
                                    "\"notify\": \"false\"}}";


            //send the data
            string createEnrollmentURL = CanvasUrl + "courses/" + courseId + "/enrollments?access_token=" + OAuthKey;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(createEnrollmentURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            CanvasEnrollmentModel newEnrollment = new CanvasEnrollmentModel();

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {

                streamWriter.Write(enrollmentJson);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var serializer = new JsonSerializer();
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        newEnrollment = serializer.Deserialize<CanvasEnrollmentModel>(jsonTextReader);
                    }
                }

            }
        }


        public JsonResult lookUpCourse(int crsuniq, int funiq, int schuniq)
        {
            CanvasCourseModel course = GetCourse(crsuniq, funiq, schuniq);
            if (course != null)
            {
                return Json(new
                {
                    success = "true",
                    course = new
                    {
                        name = course.name,
                        id = course.id,
                        sis_course_id = course.sis_course_id,
                        course_code = course.course_code
                    }
                });
            }
            else
            {
                //check by sis course id
                int schoolYear = Convert.ToInt32(SISdb.schools.Max(s => s.schyear));
                string sisCourseId = schoolYear + ":" + crsuniq + ":" + funiq;
                course = GetCourseBySISId(sisCourseId);
                if (course != null)
                {
                    return Json(new
                    {
                        success = "true",
                        course = new
                        {
                            name = course.name,
                            id = course.id,
                            sis_course_id = course.sis_course_id,
                            course_code = course.course_code
                        }
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = "false",
                        message = "Course does not exist!"
                    });
                }
            }
        }

        private CanvasCourseModel GetCourse(int crsuniq, int funiq, int schuniq)
        {
            school sisSchool = SISdb.schools.Find(schuniq);
            int schoolYear = Convert.ToInt32(SISdb.schools.Max(s => s.schyear));
            string sisCourseId = schoolYear + ":" + crsuniq + ":" + funiq;

            //get the account id
            int accountId;
            AccountIDs.TryGetValue(sisSchool.schoolc, out accountId);
            using (var client = new WebClient())
            {
                string getCourseURL = CanvasUrl + "accounts/" + accountId + "/courses/?per_page=999999999999&access_token=" + OAuthKey;
                var json = client.DownloadString(getCourseURL);
                var serializer = new JavaScriptSerializer();
                List<CanvasCourseModel> courses = serializer.Deserialize<List<CanvasCourseModel>>(json);
                var vCourse =
                    courses.Where(c => c.sis_course_id == sisCourseId);
                if (vCourse.Any())
                {
                    return vCourse.First();
                }

            }

            return null;
        }

        private CanvasCourseModel GetCourseBySISId(string sisId)
        {
            using (var client = new WebClient())
            {
                try
                {
                    string getCourseURL = CanvasUrl + "courses/sis_course_id:" + sisId +
                                          "/?per_page=999999999999&access_token=" + OAuthKey;
                    var json = client.DownloadString(getCourseURL);
                    var serializer = new JavaScriptSerializer();
                    List<CanvasCourseModel> courses = serializer.Deserialize<List<CanvasCourseModel>>(json);
                    var vCourse =
                        courses.Where(c => c.sis_course_id == sisId);
                    if (vCourse.Any())
                    {
                        return vCourse.First();
                    }
                }
                catch (Exception e)
                {
                    return null;
                }

            }

            return null;
        }

        public JsonResult LookUpSISPeriods(int crsuniq, int funiq)
        {
            //calculate the school year
            int schoolYear = Convert.ToInt32(SISdb.schools.Max(s => s.schyear));
            List<SISPeriodModel> sisPeriods = new List<SISPeriodModel>();
            var periods = SISdb.mstscheds.Where(m => m.funiq == funiq && m.trkcr.crsuniq == crsuniq && m.trkcr.track.schyear == schoolYear);

            if (periods.Any())
            {
                foreach (mstsched period in periods)
                {
                    //check to see if it has a canvas section
                    CanvasSectionModel section = GetSectionBySISId(schoolYear + ":" + period.mstuniq + ":" + funiq);

                    SISPeriodModel sisPeriod = new SISPeriodModel
                    {
                        mstuniq= period.mstuniq,
                        period = Convert.ToInt32(period.mstmeets.First().periodn),
                        termc = period.mstmeets.First().termc,
                        funiq = Convert.ToInt32(period.funiq),
                        section = section
                    };

                    sisPeriods.Add(sisPeriod);
                }
            }

            return Json(new
            {
                success = "true",
                periods = sisPeriods
            });
        }

        public JsonResult LookUpCourseSections(string courseId)
        {
            return Json(new
            {
                success = "true",
                sections = GetCourseSections(courseId)
            });
        }

        private CanvasSectionModel GetSectionBySISId(string sectionId)
        {
            CanvasSectionModel section = new CanvasSectionModel();
            try {
                using (var client = new WebClient())
                {
                    string getCourseURL = CanvasUrl + "sections/sis_section_id:" + sectionId + "?access_token=" + OAuthKey;
                    var json = client.DownloadString(getCourseURL);
                    var serializer = new JavaScriptSerializer();
                    section = serializer.Deserialize<CanvasSectionModel>(json);
                    return section;
                }
            }
            catch (Exception e)
            {
                //EmailSender.EmailSender.sendImportantMessage("CanvasError@sjsd.org", Globals.EMAIL_ADDRESSES,"Error getting section: " + sectionId, "Exception: " + e.Message);
                return null;
            }
        }

        private CanvasSectionModel GetSectionById(string sectionId)
        {
            CanvasSectionModel section = new CanvasSectionModel();
            try
            {
                using (var client = new WebClient())
                {
                    string getCourseURL = CanvasUrl + "sections/" + sectionId + "?access_token=" + OAuthKey;
                    var json = client.DownloadString(getCourseURL);
                    var serializer = new JavaScriptSerializer();
                    section = serializer.Deserialize<CanvasSectionModel>(json);
                    return section;
                }
            }
            catch (Exception e)
            {
                //EmailSender.EmailSender.sendImportantMessage("CanvasError@sjsd.org", Globals.EMAIL_ADDRESSES,"Error getting section: " + sectionId, "Exception: " + e.Message);
                return null;
            }
        }

        private List<CanvasSectionModel> GetCourseSections(string courseId)
        {
            //get all the course sections
            string getSectionUrl = CanvasUrl + "courses/" + courseId + "/sections?access_token=" + OAuthKey;
            try
            {
                using (var client = new WebClient())
                {
                    var json = client.DownloadString(getSectionUrl);
                    var serializer = new JavaScriptSerializer();
                    List<CanvasSectionModel> sections = serializer.Deserialize<List<CanvasSectionModel>>(json);
                    return sections;

                }
            }
            catch (Exception e)
            {
                EmailSender.EmailSender.sendImportantMessage("CanvasError@sjsd.org", Globals.EMAIL_ADDRESSES,
                    "Error getting sections", "Exception: " + e.Message);
                return null;
            }

        }

        private CanvasCourseModel GetCourseById(string courseId)
        {
            CanvasCourseModel course = new CanvasCourseModel();
            using (var client = new WebClient())
            {
                try
                {
                    string getCourseURL = CanvasUrl + "courses/" + courseId + "?access_token=" + OAuthKey;
                    var json = client.DownloadString(getCourseURL);
                    var serializer = new JavaScriptSerializer();
                    course = serializer.Deserialize<CanvasCourseModel>(json);
                    return course;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public JsonResult UpdateStudentsInSection(string mstuniq)
        {
            int usersAdded = 0;
            int usersRemoved = 0;
            int usersLeftAlone = 0;
            int usersWithErrors = 0;
            List<studemo> errorUsers = new List<studemo>();
            //calculate the school year
            int schoolYear = Convert.ToInt32(SISdb.schools.Max(s => s.schyear));
            
            mstsched period = SISdb.mstscheds.Find(Convert.ToInt32(mstuniq));
            
            if (period == null)
            {
                return Json(new
                {
                    success = "false",
                    message = "Period does not exist in SIS!"
                });
            }

            CanvasSectionModel section = GetSectionBySISId(schoolYear + ":" + mstuniq + ":" + period.facdemo.funiq);

            if (section == null)
            {
                return Json(new
                {
                    success = "false",
                    message = "Section does not exist in Canvas!"
                });
            }
            
            //get the students in the section in canvas
            List<CanvasUserModel> users = GetUsersInSection(section.id);
            List<CanvasUserModel> usersNotInSIS = GetUsersInSection(section.id);
            
            //get the students in the period in sis
            DateTime tomorrow = DateTime.Now.AddDays(-365);
            var vStuSched = SISdb.stuscheds.Where(s => s.mstmeet.mstuniq == period.mstuniq && (s.xdate >=  tomorrow || s.xdate == null));
            foreach (stusched student in vStuSched)
            {
                //check to see if they are in the section
                CanvasUserModel canvasStudent = users.Find(u => u.sis_user_id == student.suniq.ToString());
                if (canvasStudent == null)
                {
                    //add the student
                    //see if student exists in canvas
                    canvasStudent = GetUserBySISId(student.suniq.ToString());
                    if (canvasStudent == null)
                    {
                        //create the user
                        school sisSchool = SISdb.schools.First(s => s.schoolc == period.trkcr.track.schoolc);
                        canvasStudent = CreateCanvasUser(student.suniq, sisSchool.schuniq);

                    }

                    if (canvasStudent != null)
                    {
                        //enroll the current user
                        EnrollStudentInSection(canvasStudent.id, section.id);
                        usersAdded++;
                    }
                    else
                    {
                        errorUsers.Add(student.studemo);
                        usersWithErrors++;
                    }
                }
                else
                {
                    //student is in sis and the course in canvas do nothing with them.
                    usersLeftAlone++;
                }

                if (canvasStudent != null)
                {
                    usersNotInSIS.RemoveAll(c => c.id == canvasStudent.id);
                }
            }

            foreach (CanvasUserModel canvasStudent in usersNotInSIS)
            {
                UnEnrollStudentFromSection(canvasStudent.id, section.course_id);
                usersRemoved++;
            }

            if (errorUsers.Any())
            {
                string message = "These users failed to enroll, likely because they are deleted in canvas: <br /><ul>";
                foreach (studemo errorUser in errorUsers)
                {
                    message += "<li>" + errorUser.FirstName + " " + errorUser.LastName + " Email Address:" + errorUser.emailaddr + "</li>";
                }
                message += "</ul>";

                EmailSender.EmailSender.sendTemplatedMessage("CanvasEnrollmentError@sjsd.org",
                    Globals.EMAIL_ADDRESSES,"Failed to enroll students in class", message, "High");
            }


            return Json(new
            {
                success = "true",
                studentsAdded = usersAdded,
                studentsRemoved = usersRemoved,
                studentsLeftAlone = usersLeftAlone,
                studentsCurrentlyEnrolled = vStuSched.Count(),
                studentsWithErrors = usersWithErrors
            });

        }

        private CanvasEnrollmentModel UnEnrollStudentFromSection(string canvasUserId, string canvasCourseId)
        {
            //get the enrollmentId
            CanvasEnrollmentModel enrollment = GetUserEnrollment(canvasUserId, canvasCourseId);
            if (enrollment != null)
            {
                string unEnrollmentJson = "{\"task\": \"conclude\"}";

                //send the data
                string removeEnrollmentURL = CanvasUrl + "courses/" + canvasCourseId + "/enrollments/" + enrollment.id +
                                             "?access_token=" + OAuthKey;
                var httpWebRequest = (HttpWebRequest) WebRequest.Create(removeEnrollmentURL);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "DELETE";

                CanvasEnrollmentModel newEnrollment = new CanvasEnrollmentModel();

                try
                {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {

                        streamWriter.Write(unEnrollmentJson);
                        streamWriter.Flush();
                        streamWriter.Close();

                        var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var serializer = new JsonSerializer();
                            using (var jsonTextReader = new JsonTextReader(streamReader))
                            {
                                newEnrollment = serializer.Deserialize<CanvasEnrollmentModel>(jsonTextReader);
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    newEnrollment = null;
                }

                return newEnrollment;
            }
            return null;
        }

        private CanvasEnrollmentModel EnrollStudentInSection(string canvasUserId, string canvasSectionId)
        {
            string enrollmentJson = "{\"enrollment\": {" +
                                    "\"user_id\": \"" + canvasUserId + "\"," +
                                    "\"type\": \"StudentEnrollment\"," +
                                    "\"enrollment_state\": \"active\"," +
                                    //when we go live this should be true.
                                    "\"notify\": \"false\"}}";
       

            //send the data
            string createEnrollmentURL = CanvasUrl + "sections/" + canvasSectionId + "/enrollments?access_token=" + OAuthKey;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(createEnrollmentURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            CanvasEnrollmentModel newEnrollment = new CanvasEnrollmentModel();

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                
                streamWriter.Write(enrollmentJson);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var serializer = new JsonSerializer();
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        newEnrollment = serializer.Deserialize<CanvasEnrollmentModel>(jsonTextReader);
                    }
                }

            }

            return newEnrollment;
        }

        private CanvasUserModel CreateCanvasTeacher(int funiq , int schuniq)
        {
            school sisSchool = SISdb.schools.Find(schuniq);

            //get the account id
            int accountId;
            AccountIDs.TryGetValue(sisSchool.schoolc, out accountId);

            //make sure the user doesn't exist
            CanvasUserModel user = GetUserBySISId("F" + funiq);
            if (user != null)
            {
                //the user exists
                return user;
            }

            //the user does not exist
            facdemo teacher = SISdb.facdemoes.Find(funiq);
            string password = teacher.firstname.Substring(0, 1).ToUpper() +
                              teacher.lastname.Substring(0, 2).ToLower() + funiq;
            string userJson = "{\"user\": {" +
                                    "\"name\": \"" + teacher.firstname.Trim() + " " + teacher.lastname.Trim() + "\"," +
                                    "\"short_name\": \"" + teacher.firstname.Trim() + "\"," +
                                    "\"sortable_name\": \"" + teacher.lastname.Trim() + ", " + teacher.firstname.Trim() + "\"}," +
                                "\"pseudonym\": {" +
                                    "\"unique_id\": \"" + teacher.emailaddr + "\"," +
                                    "\"password\": \"" + password + "\"," +
                                    "\"sis_user_id\": \"F" + funiq + "\"," +
                //this should be set to true on live
                                    "\"send_confirmation\": \"false\"}," +
                                "\"communication_channel\": {" +
                                    "\"type\":\"email\"," +
                                    "\"address\": \"" + teacher.emailaddr + "\"," +
                                    "\"skip_confirmation\": \"true\"}}";


            //send the data
            string createUserURL = CanvasUrl + "accounts/" + accountId + "/users?access_token=" + OAuthKey;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(createUserURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            CanvasUserModel newUser = new CanvasUserModel();

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {

                streamWriter.Write(userJson);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var serializer = new JsonSerializer();
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        newUser = serializer.Deserialize<CanvasUserModel>(jsonTextReader);
                    }
                }

            }

            return newUser;
        }

        private CanvasUserModel CreateCanvasUser(int suniq, int schuniq)
        {
            school sisSchool = SISdb.schools.Find(schuniq);

            //get the account id
            int accountId;
            AccountIDs.TryGetValue(sisSchool.schoolc, out accountId);

            //make sure the user doesn't exist
            CanvasUserModel user = GetUserBySISId(suniq.ToString());
            if (user != null)
            {
                //the user exists
                return user;
            }

            //the user does not exist
            studemo student = SISdb.studemoes.Find(suniq);
            string password = student.FirstName.Substring(0, 1).ToUpper() +
                              student.LastName.Substring(0, 1).ToLower() + student.suniq.ToString().Substring(student.suniq.ToString().Length - 4);
            string userJson = "{\"user\": {" +
                                    "\"name\": \"" + student.FirstName.Trim() + " " + student.LastName.Trim() + "\"," +
                                    "\"short_name\": \"" + student.PreferredFirstName.Trim() + "\"," +
                                    "\"sortable_name\": \"" + student.LastName.Trim() + ", " + student.FirstName.Trim() + "\"}," +
                                "\"pseudonym\": {" +
                                    "\"unique_id\": \"" + student.emailaddr + "\"," +
                                    "\"password\": \"" + password + "\"," +
                                    "\"sis_user_id\": \"" + suniq + "\"," +
                                    //this should be set to true on live
                                    "\"send_confirmation\": \"false\"}," +
                                "\"communication_channel\": {" +
                                    "\"type\":\"email\"," +
                                    "\"address\": \"" + student.emailaddr + "\"," +
                                    "\"skip_confirmation\": \"true\"}}";
       

            //send the data
            string createUserURL = CanvasUrl + "accounts/" + accountId + "/users?access_token=" + OAuthKey;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(createUserURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            CanvasUserModel newUser = new CanvasUserModel();
            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {

                    streamWriter.Write(userJson);
                    streamWriter.Flush();
                    streamWriter.Close();

                    var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var serializer = new JsonSerializer();
                        using (var jsonTextReader = new JsonTextReader(streamReader))
                        {
                            newUser = serializer.Deserialize<CanvasUserModel>(jsonTextReader);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                newUser = null;
            }

            return newUser;
        }

        private CanvasEnrollmentModel GetUserEnrollment(string canvasUserId, string canvasCourseId)
        {
            CanvasEnrollmentModel enrollment = new CanvasEnrollmentModel();
            using (var client = new WebClient())
            {
                try
                {
                    string getUserURL = CanvasUrl + "users/" + canvasUserId + "/enrollments?access_token=" + OAuthKey;
                    var json = client.DownloadString(getUserURL);
                    var serializer = new JavaScriptSerializer();
                    List<CanvasEnrollmentModel> enrollments = serializer.Deserialize<List<CanvasEnrollmentModel>>(json);
                    enrollment = enrollments.First(e => e.course_id == canvasCourseId);
                    return enrollment;
                }
                catch (Exception e)
                {
                    return null;
                }
            }

        }

        private CanvasUserModel GetUserById(string Id)
        {
            CanvasUserModel user = new CanvasUserModel();
            using (var client = new WebClient())
            {
                try
                {
                    string getUserURL = CanvasUrl + "users/" + Id + "/profile?access_token=" + OAuthKey;
                    var json = client.DownloadString(getUserURL);
                    var serializer = new JavaScriptSerializer();
                    user = serializer.Deserialize<CanvasUserModel>(json);
                    return user;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        private CanvasUserModel GetUserBySISId(string sisId)
        {
            CanvasUserModel user = new CanvasUserModel();
            using (var client = new WebClient())
            {
                try
                {
                    string getUserURL = CanvasUrl + "users/sis_user_id:" + sisId + "/profile?access_token=" + OAuthKey;
                    var json = client.DownloadString(getUserURL);
                    var serializer = new JavaScriptSerializer();
                    user = serializer.Deserialize<CanvasUserModel>(json);
                    return user;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        private List<CanvasUserModel> GetUsersInSection(string canvasSectionId)
        {

            using (var client = new WebClient())
            {
                string getCourseURL = CanvasUrl + "sections/" + canvasSectionId + "/enrollments/?per_page=999999999999&access_token=" + OAuthKey;
                var json = client.DownloadString(getCourseURL);
                var serializer = new JavaScriptSerializer();
                List<CanvasEnrollmentModel> enrollments = serializer.Deserialize<List<CanvasEnrollmentModel>>(json);

                List<CanvasUserModel> users = new List<CanvasUserModel>();

                foreach (CanvasEnrollmentModel enrollment in enrollments)
                {
                    CanvasUserModel user = GetUserById(enrollment.user_id);
                    if (user != null)
                    {
                        users.Add(user);
                    }
                }
                

                return users;
            }

            return null;

            
        } 

        public JsonResult CreateCourseSection(string courseId, string mstUniq)
        {
            mstsched period = SISdb.mstscheds.Find(Convert.ToInt32(mstUniq));
            //calculate the school year
            int schoolYear = Convert.ToInt32(SISdb.schools.Max(s => s.schyear));

            if (period != null)
            {
                //check to see that the section isn't already created
                CanvasSectionModel section = GetSectionBySISId(schoolYear + ":" + period.mstuniq + ":" + period.funiq);

                if (section != null)
                {
                    return Json(new
                    {
                        success = "true",
                        sectionId = section.id
                    });
                }

                //get the course
                CanvasCourseModel course = GetCourseById(courseId);
                if (course != null)
                {
                    string sectionJson = "{ \"course_section\": {" +
                                    "\"name\": \"" + period.trkcr.course.descript + " : " + period.facdemo.lastname.Trim() + " " + period.facdemo.firstname.Trim() +  " : " + period.mstmeets.First().periodn + "\"," +
                                    "\"sis_section_id\": \"" + schoolYear + ":" + period.mstuniq + ":" + period.funiq + "\"}}";

                    string createSectionUrl = CanvasUrl + "courses/" + courseId + "/sections?access_token=" + OAuthKey;
                    var httpWebRequest = (HttpWebRequest) WebRequest.Create(createSectionUrl);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";

                    CanvasSectionModel newSection = new CanvasSectionModel();

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {

                        streamWriter.Write(sectionJson);
                        streamWriter.Flush();
                        streamWriter.Close();

                        var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var serializer = new JsonSerializer();
                            using (var jsonTextReader = new JsonTextReader(streamReader))
                            {
                                newSection = serializer.Deserialize<CanvasSectionModel>(jsonTextReader);
                            }
                        }

                    }

                    return Json(new
                    {
                        success = "true",
                        sectionId = newSection.id,
                        mstUniq = mstUniq
                    });
                }

                return Json(new
                {
                    success = "false",
                    message = "Could not get course!"
                });
            }

            return Json(new
            {
                success = "false",
                message = "Could not get mstsched!"
            });
        }
    }
}
