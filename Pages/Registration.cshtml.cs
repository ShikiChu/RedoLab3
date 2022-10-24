using AcademicManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RedoLab3.Pages
{
    public class RegistrationModel : PageModel
    {
        [BindProperty]
        public string SeletedStudentID { get; set; } = "-1";
        public string Alert { get; set; }

        [BindProperty]
        public List<SelectListItem> CourseSelections { get; set; }

        [BindProperty]
        public List<AcademicRecord> records { get; set; }
       
        public List<Course> courseTitle { get; set; }

        public void OnGet(string orderBy) 
        {
            if (orderBy!=null) 
            {
                HttpContext.Session.SetString("orderBy", orderBy); // set session * remember to set it 5-10 miuntes in Program.cs

                SeletedStudentID = HttpContext.Session.GetString("SeletedStudentID"); // retrieve session

                if (SeletedStudentID!=null && SeletedStudentID != "-1") 
                {
                    records = DataAccess.GetAcademicRecordsByStudentId(SeletedStudentID); // initial var records

                    CourseSelections = new List<SelectListItem>(); // initial var CourseSelections, before submitting grade
                    foreach (var c in DataAccess.GetAllCourses())
                    {
                        CourseSelections.Add(new SelectListItem(c.CourseTitle, c.CourseCode, false));
                    }

                    if (orderBy == "code")
                    {
                        CourseSelections.Sort((a,b)=>a.Value.CompareTo(b.Value));
                        records.Sort((a, b) => a.CourseCode.CompareTo(b.CourseCode));
                        courseTitle = new List<Course>();
                        foreach (AcademicRecord record in records)
                        {
                            courseTitle.Add(DataAccess.GetAllCourses().First(c => c.CourseCode == record.CourseCode)); //grab the course info obj that matches
                        }
                    }
                    else if (orderBy == "title")
                    {
                        courseTitle = new List<Course>();
                        foreach (AcademicRecord record in records)
                        { 
                            courseTitle.Add(DataAccess.GetAllCourses().First(c => c.CourseCode == record.CourseCode));
                        }
                        courseTitle.Sort((a, b) => a.CourseTitle.CompareTo(b.CourseTitle));

                        CourseSelections.Sort((a, b) => a.Text.CompareTo(b.Text));


                    }
                    else if (orderBy == "grade") 
                    {
                        records.Sort((a,b)=>a.Grade.CompareTo(b.Grade));
                        courseTitle = new List<Course>();
                        foreach (AcademicRecord record in records)
                        {
                            courseTitle.Add(DataAccess.GetAllCourses().First(c => c.CourseCode == record.CourseCode));
                        }
                    }
                }
            }
        }
        public void OnPostRegistration()
        {
            if (SeletedStudentID == "-1")
            {
                Alert = "you need to choose a student";
                CourseSelections = null;
                HttpContext.Session.Remove("SeletedStudentID");
            }
            else 
            {
                HttpContext.Session.SetString("SeletedStudentID", SeletedStudentID);
                if (DataAccess.GetAcademicRecordsByStudentId(SeletedStudentID).Count == 0)
                {
                    Alert = "Selected student has not registered any courses! Make choice!";
                    records = DataAccess.GetAcademicRecordsByStudentId(SeletedStudentID);
                    CourseSelections = new List<SelectListItem>();
                    foreach (var c in DataAccess.GetAllCourses())
                    {
                        CourseSelections.Add(new SelectListItem(c.CourseTitle, c.CourseCode, false));
                    }
                }
                else 
                {
                    Alert = "Selected student has  registered the following courses, you can edit grade";
                    records = DataAccess.GetAcademicRecordsByStudentId(SeletedStudentID);
                    courseTitle = new List<Course>();
                    foreach (AcademicRecord record in records)
                    {
                        courseTitle.Add(DataAccess.GetAllCourses().First(c => c.CourseCode == record.CourseCode));
                    }

                    string orderBy = HttpContext.Session.GetString("orderBy"); // it shows the user's previous choice.
                    if (SeletedStudentID != null && SeletedStudentID != "-1")
                    {
                        records = DataAccess.GetAcademicRecordsByStudentId(SeletedStudentID); // initial var records

                        if (orderBy == "code")
                        {
                            records.Sort((a, b) => a.CourseCode.CompareTo(b.CourseCode));
                            courseTitle = new List<Course>();
                            foreach (AcademicRecord record in records)
                            {
                                courseTitle.Add(DataAccess.GetAllCourses().First(c => c.CourseCode == record.CourseCode)); //grab the course info obj that matches
                            }
                        }
                        else if (orderBy == "title")
                        {
                            courseTitle = new List<Course>();
                            foreach (AcademicRecord record in records)
                            {
                                courseTitle.Add(DataAccess.GetAllCourses().First(c => c.CourseCode == record.CourseCode));
                            }
                            courseTitle.Sort((a, b) => a.CourseTitle.CompareTo(b.CourseTitle));
                        }
                        else if (orderBy == "grade")
                        {
                            records.Sort((a, b) => a.Grade.CompareTo(b.Grade));
                            courseTitle = new List<Course>();
                            foreach (AcademicRecord record in records)
                            {
                                courseTitle.Add(DataAccess.GetAllCourses().First(c => c.CourseCode == record.CourseCode));
                            }
                        }
                    }
                }
            }
        }


        public void OnPostRegister() 
        {
            Alert = "Selected student has  registered the following courses, you can edit grade";
            records = new List<AcademicRecord>();
            foreach (SelectListItem item in CourseSelections)
            {
                if (item.Selected)
                {
                    DataAccess.AddAcademicRecord(new AcademicRecord(SeletedStudentID, item.Value));
                    records.Add(new AcademicRecord(SeletedStudentID, item.Value));
                }    
            }
            courseTitle = new List<Course>();
            foreach (AcademicRecord record in records)
            {
                courseTitle.Add(DataAccess.GetAllCourses().First(c => c.CourseCode == record.CourseCode));
            }

            if (records.Count()==0) 
            {
                Alert = "you need to make your choice";
                CourseSelections = new List<SelectListItem>();
                foreach (var c in DataAccess.GetAllCourses())
                {
                    CourseSelections.Add(new SelectListItem(c.CourseTitle, c.CourseCode, false));
                }
            }
        }

        public void OnPostSubmitGrade() 
        {
            foreach (AcademicRecord record in records) 
            {
                DataAccess.GetAcademicRecordsByStudentId(SeletedStudentID).First(c=>c.CourseCode == record.CourseCode).Grade = record.Grade;
            }
            courseTitle = new List<Course>();
            foreach (AcademicRecord record in records)
            {
                courseTitle.Add(DataAccess.GetAllCourses().First(c => c.CourseCode == record.CourseCode));
            }
        }
    }
}
