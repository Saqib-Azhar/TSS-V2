using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TotalStaffingSolutions.Models;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

namespace TotalStaffingSolutions.Controllers
{

    public class TSSManageController : Controller
    {
        private static List<Employee> EmployeesStaticList = null;

        private static string SenderEmailId = WebConfigurationManager.AppSettings["DefaultEmailId"];
        private static string SenderEmailPassword = WebConfigurationManager.AppSettings["DefaultEmailPassword"];
        private static int SenderEmailPort = Convert.ToInt32(WebConfigurationManager.AppSettings["DefaultEmailPort"]);
        private static string SenderEmailHost = WebConfigurationManager.AppSettings["DefaultEmailHost"];

        private static string TSSLiveSiteURL = WebConfigurationManager.AppSettings["TSSLiveSiteURL"];
        // GET: Manage
        [Authorize(Roles = "Admin")]
        public ActionResult Dashboard1() //IMPORTANT UPDATE ALL FOLLOWING METHODS (Dashboard, TimeSheetsByBranch, TimeSheetsByClient, Timesheetsbyperiod)
        {
            try
            {
                var db = new TSS_Sql_Entities();
                List<Branch> bl = db.Branches.ToList();
                Branch b = new Branch();
                b.Id = 0;
                b.Name = "Select Branch";
                bl.Add(b);
                var orderedbranches = bl.OrderBy(s => s.Id);
                var branchlist = new SelectList(orderedbranches, "Id", "Name");
                ViewBag.BranchsList = branchlist;

                List<Customer> cl = db.Customers.ToList();
                Customer c = new Customer();
                c.Id = 0;
                c.Name = "Select Client";
                cl.Add(c);
                var orderedClients = cl.OrderBy(s => s.Id);
                var clientlist = new SelectList(orderedClients, "Id", "Name");
                ViewBag.ClientsList = clientlist;

                ViewBag.SelectedBranchId = "";
                ViewBag.SelectedClientId = "";

                //ViewBag.RejectedTimeSheets = db.RejectedTimesheets.ToList();

                return View(db.Timesheets.ToList()); 
            }
            catch (Exception ex)
            {
               ExceptionHandlerController.infoMessage(ex.Message);
               ExceptionHandlerController.writeErrorLog(ex);
            }
            var list = new List<Timesheet>();

            return View(list);
        }
        
        [Authorize(Roles = "Admin")]
        public ActionResult Branches()
        {
            var db = new TSS_Sql_Entities();

            return View(db.Branches.ToList());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddBranch()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddNewBranch(Branch branch)
        {
            try
            {
                var db = new TSS_Sql_Entities();
                branch.Created_at = DateTime.Now;
                branch.Updated_at = DateTime.Now;
                db.Branches.Add(branch);
                db.SaveChanges();
            }
            catch (Exception)
            {
            }
            return RedirectToAction("Branches", "TSSManage");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EditBranch(int Id)
        {
            try
            {
                var db = new TSS_Sql_Entities();
                return View(db.Branches.Find(Id));
            }
            catch (Exception)
            {
                return RedirectToAction("Branches", "TSSManage");
            }
        }
        
        [Authorize(Roles = "Admin")]
        public ActionResult EditExistingBranch(Branch branch)
        {
            try
            {
                var db = new TSS_Sql_Entities();
                var findBranch = db.Branches.FirstOrDefault(s => s.Id == branch.Id);
                findBranch.Name = branch.Name;
                findBranch.Organization_id = branch.Organization_id;
                findBranch.Updated_at = DateTime.Now;
                findBranch.Email = branch.Email;
                findBranch.Created_at = DateTime.Now;
                findBranch.Branch_Manager = branch.Branch_Manager;
                findBranch.Phone = branch.Phone;
                findBranch.Address = branch.Address;
                db.SaveChanges();

            }
            catch (Exception)
            {
            }
            return RedirectToAction("Branches", "TSSManage");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddClient()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddNewCompany(Company company)
        {
            try
            {
                var db = new TSS_Sql_Entities();
                company.Created_at = DateTime.Now;
                db.Companies.Add(company);
                db.SaveChanges();
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Dashboard", "TSSManage");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AllClients()
        {
            var db = new TSS_Sql_Entities();

            return View(db.Customers.ToList());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ClientDetails(int Id)
        {
            var db = new TSS_Sql_Entities();
            try
            {
                var a = db.Customers.FirstOrDefault(x => x.Id == Id);
                var userObject = db.AspNetUsers.FirstOrDefault(s => s.Customer_id == a.Customer_id);
                ViewBag.ClientContacts = db.CustomerContacts.Where(s => s.Customer_id == a.Customer_id).ToList();
                ViewBag.DisplayPicture = userObject.DisplayPicture;
                var timeSheets = db.Timesheets.Where(s => s.Customer_id == Id).ToList();
                if (timeSheets.Count == 0)
                {
                    var tempTimeSheet = new Timesheet();
                    tempTimeSheet.Customer = db.Customers.FirstOrDefault(s => s.Id == Id);
                    timeSheets.Add(tempTimeSheet);
                }
                return View(timeSheets);
            }
            catch
            {
                var timeSheets = new List<Timesheet>();
                var tempTimeSheet = new Timesheet();
                tempTimeSheet.Customer = db.Customers.FirstOrDefault(s => s.Id == Id);
                timeSheets.Add(tempTimeSheet);
                return View(timeSheets);
            }
        }

       
        [Authorize(Roles = "Admin")]
        public ActionResult UpdateCustomerInfo(int id)
        {
            var db = new TSS_Sql_Entities();
            ViewBag.Branch_id = new SelectList(db.Branches, "Id", "Name");
            return View(db.Customers.Find(id));
        }

        [Authorize(Roles = "Admin")]
        public ActionResult UpdateCustomerInfoFunc(Customer customer)
        {
            var db = new TSS_Sql_Entities();

            var customerExistingObj = db.Customers.Find(customer.Id);
            customerExistingObj.Email = customer.Email;
            customerExistingObj.PhoneNumber = customer.PhoneNumber;
            customerExistingObj.Branch_id = customer.Branch_id;
            db.SaveChanges();
            return RedirectToAction("AllClients");
        }

        


        [System.Web.Mvc.HttpGet]
        public JsonResult SearchEmployees(String query)
        {
            var db = new TSS_Sql_Entities();
            
            int n;
            bool isNumeric = int.TryParse(query, out n);

            if (EmployeesStaticList == null)
            {
                EmployeesStaticList = db.Employees.ToList();
            }

            if (isNumeric == true)
            {
                var integerValue = Convert.ToInt32(query);
                var results = (from obj in EmployeesStaticList where obj.User_id == integerValue select new { Id = obj.Id, Name = obj.User_id + "-" + obj.First_name + " " + obj.Last_name }).Take(25).ToList();
                return Json(results, JsonRequestBehavior.AllowGet);
            }
            else
            {

                var splitStr = Regex.Split(query, " ");
                if (splitStr.Count() == 2)
                {
                    splitStr[0] = splitStr[0].First().ToString().ToUpper() + splitStr[0].Substring(1);
                    splitStr[1] = splitStr[1].First().ToString().ToUpper() + splitStr[1].Substring(1);
                    var results = (from obj in EmployeesStaticList where obj.First_name.Contains(splitStr[0]) || obj.Last_name.Contains(splitStr[1]) select new { Id = obj.Id, Name = obj.User_id + "-" + obj.First_name + " " + obj.Last_name }).Take(25).ToList();
                    return Json(results, JsonRequestBehavior.AllowGet);
                }
                else if (splitStr.Count() == 3)
                {
                    splitStr[0] = splitStr[0].First().ToString().ToUpper() + splitStr[0].Substring(1);
                    splitStr[2] = splitStr[2].First().ToString().ToUpper() + splitStr[1].Substring(1);
                    var results = (from obj in EmployeesStaticList where obj.First_name.Contains(splitStr[0]) || obj.Last_name.Contains(splitStr[2]) select new { Id = obj.Id, Name = obj.User_id + "-" + obj.First_name + " " + obj.Last_name }).Take(25).ToList();
                    return Json(results, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    query = query.First().ToString().ToUpper() + query.Substring(1);
                    var results = (from obj in EmployeesStaticList where obj.First_name.Contains(query) select new { Id = obj.Id, Name = obj.User_id + "-" + obj.First_name + " " + obj.Last_name }).Take(25).ToList();
                    return Json(results, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AllEmployees()
        {
            var db = new TSS_Sql_Entities();
            var employeesList = db.Employees.ToList();
            return View(employeesList);
        }

        

        [Authorize(Roles ="Admin")]
        public ActionResult UpdateEmployee(int id)
        {
            var db = new TSS_Sql_Entities();
            ViewBag.Branch_id = new SelectList(db.Branches, "Id", "Name");
            var employee = db.Employees.Find(id);
            return View(employee);
        }

        public ActionResult UpdateEmployeeEntry(Employee employee)
        {
            var db = new TSS_Sql_Entities();
            var employeeObj = db.Employees.Find(employee.Id);
            employeeObj.Address1 = employee.Address1;
            employeeObj.Address2 = employee.Address2;
            employeeObj.Anniversary_date = employee.Anniversary_date;
            employeeObj.Branch_id = employee.Branch_id;
            employeeObj.City = employee.City;
            employeeObj.Country = employee.Country;
            employeeObj.DateOfBirth = employee.DateOfBirth;
            employeeObj.EmployeeType = employee.EmployeeType;
            employeeObj.First_name = employee.First_name;
            employeeObj.Gender = employee.Gender;
            employeeObj.Hire = employee.Hire;
            employeeObj.Last_name = employee.Last_name;
            employeeObj.Marital_status = employee.Marital_status;
            employeeObj.Middle_name = employee.Middle_name;
            employeeObj.ReHire = employee.ReHire;
            employeeObj.State = employee.State;
            employeeObj.Status = employee.Status;
            employeeObj.Updated_at = DateTime.Now;
            employeeObj.ZipCode = employee.ZipCode;
            db.SaveChanges();
            return RedirectToAction("AllEmployees", "TSSManage");
        }

        public ActionResult SendConfirmationStatus(string email, int Id)
        {
            var db = new TSS_Sql_Entities();
            var emailId = db.CustomerContacts.FirstOrDefault(s => s.Email_id == email);
            var response = SendTimeSheetLink(Id, email, 1);
            return RedirectToAction("SendAccountEmail", "TSSManage",new { id = emailId.Id});
        }



        [Authorize(Roles = "Admin")]
        public ActionResult SendAccountEmail(int id)
        {
            var db = new TSS_Sql_Entities();

            var ConfirmationToken = Membership.GeneratePassword(10, 0);
            ConfirmationToken = Regex.Replace(ConfirmationToken, @"[^a-zA-Z0-9]", m => "9");
            var savedContactObj = db.CustomerContacts.OrderByDescending(s => s.Id).FirstOrDefault(s => s.Id == id);
            var contactStatusObj = new ContactConfirmation();
            contactStatusObj.ContactId = savedContactObj.Id;
            contactStatusObj.LastUpdate = DateTime.Now;
            contactStatusObj.TokenCreationTime = contactStatusObj.LastUpdate;
            contactStatusObj.TokenExpiryTime = DateTime.Now.AddHours(24);
            contactStatusObj.ConfirmationToken = ConfirmationToken;
            contactStatusObj.ConfirmationStatusId = 1;
            db.ContactConfirmations.Add(contactStatusObj);
            db.SaveChanges();
            try
            {
                var fromAddress = new MailAddress(SenderEmailId, "Total Staffing Solution");
                var toAddress = new MailAddress("sazhar@viretechnologies.com", savedContactObj.Contact_name);
                string fromPassword = SenderEmailPassword;
                string subject = "Total Staffing Solution: Account Confirmation";
                string body = "<b>Hello " + savedContactObj.Email_id + "!</b><br />Someone has invited you to http://total-staffing.raisenit.com/, you can accept it through the link below.<br /> <a href='" + TSSLiveSiteURL + "/ClientDashboard/ConfirmAccount?token=" + ConfirmationToken + "'>Accept invitation</a><br /><small style='text-align:center;'>(If you don't want to accept the invitation, please ignore this email.Your account won't be created until you access the link above and set your password.<br/><b>This Link is active for next 24 hours, Please make sure to Enable your account before " + DateTime.Now.AddHours(24) + "</b>)</small>";

                var smtp = new SmtpClient
                {
                    Host = SenderEmailHost,
                    Port = SenderEmailPort,
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = body,


                })
                {
                    //message.CC.Add("jgallelli@4tssi.com");

                    smtp.Send(message);
                }
                ///
                var savedContactConfirmationObj = db.ContactConfirmations.OrderByDescending(s => s.Id).FirstOrDefault(s => s.ContactId == contactStatusObj.ContactId);
                savedContactConfirmationObj.ConfirmationStatusId = 2;
                savedContactConfirmationObj.LastUpdate = DateTime.Now;
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                var savedContactConfirmationObj = db.ContactConfirmations.OrderByDescending(s => s.Id).FirstOrDefault(s => s.ContactId == contactStatusObj.ContactId);
                savedContactConfirmationObj.ConfirmationStatusId = 4;
                savedContactConfirmationObj.LastUpdate = DateTime.Now;
                db.SaveChanges();
            }

            var clientContactObj = db.CustomerContacts.FirstOrDefault(s => s.Id == id);
            var clientObject = db.Customers.FirstOrDefault(s =>s.Customer_id == clientContactObj.Customer_id);
            var previousURL = System.Web.HttpContext.Current.Request.UrlReferrer;
            if(previousURL.LocalPath == "/TSSManage/Dashboard")
            {
                return RedirectToAction("Dashboard", "TSSManage");
            }
            else
            {
                return RedirectToAction("ClientDetails", "TSSManage", new { clientObject.Id });
            }
        }


        public ActionResult GoToDashboard()
        {
            if(User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "TSSManage");
            }
            else
            {
                return RedirectToAction("AllTimeSheets", "ClientDashboard");
            }
        }
      
        public JsonResult SendTimeSheetLink(int id, string email, int checkUser = 0)
        {
            var db = new TSS_Sql_Entities();
            var timesheet = db.Timesheets.Find(id);
            //var customerId = timesheet.Customer_Id_Generic;
            timesheet.Status_id = 2;
            db.SaveChanges();
            var user = db.AspNetUsers.FirstOrDefault(s => s.Email == email);
            if(user == null && checkUser == 0)
            {
                return Json("Customer Doesn't Exists", JsonRequestBehavior.AllowGet);
            }

            try
            {
                var fromAddress = new MailAddress(SenderEmailId, "Total Staffing Solution");
                var toAddress = new MailAddress("sazhar@viretechnologies.com", email);
                string fromPassword = SenderEmailPassword;
                string subject = "Total Staffing Solution: New Timesheet";
                DateTime saturday = DateTime.Now.AddDays(6 - Convert.ToDouble(DateTime.Now.DayOfWeek));
                string dateString = String.Format("{0:MM/dd/yyyy}", saturday);
                string body = "<b>Hello "+ email + "!</b><br />Below you'll find the link to your timesheets for the upcoming week. Please enter the hours worked for the employees listed and return to us by " + dateString + ".<br /><br /><a href='"
                    + TSSLiveSiteURL + "/Timesheets/TimeSheetDetails/" + id + "'>Timesheet Link</a> <br />Thanks for joining and have a great day! <br />Total Staffing Solutions";
                
                var smtp = new SmtpClient
                {
                    Host = SenderEmailHost,
                    Port = SenderEmailPort,
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = body,

                })
                {
                    //message.CC.Add("jgallelli@4tssi.com");
                    ////message.CC.Add("payroll@4tssi.com");
                    smtp.Send(message);
                }

                return Json("Link Sent successfully", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                return Json("Something Went wrong..!", JsonRequestBehavior.AllowGet);

            }

            
        }

     





        public bool updateentriesoftimesheets()
        {
            try
            {
                var db = new TSS_Sql_Entities();

                var tss = db.Timesheets.ToList();
                foreach (var item in tss)
                {
                    var ts = db.Timesheets.Find(item.Id);
                    var sumlist = db.Timesheet_summaries.Where(s => s.Timesheet_id == ts.Id);
                    ts.Total_employees = sumlist.Count();
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }



        public ActionResult TimeSheetsByBranch(int? id=0) //IMPORTANT UPDATE ALL FOLLOWING METHODS (Dashboard, TimeSheetsByBranch, TimeSheetsByClient, Timesheetsbyperiod)
        {
            try
            {
                if (id > 0)
                {
                    var db = new TSS_Sql_Entities();

                    List<Branch> bl = db.Branches.ToList();
                    Branch b = new Branch();
                    b.Id = 0;
                    b.Name = "Select Branch";
                    bl.Add(b);
                    var a = bl.OrderBy(s => s.Id);
                    var branchlist = new SelectList(a, "Id", "Name");
                    ViewBag.BranchsList = branchlist;
                    var timesheets = db.Timesheets.Where(s => s.Customer.Branch_id == id).ToList();
                    ViewBag.SelectedBranchId = id;
                    List<Customer> cl = db.Customers.ToList();
                    Customer c = new Customer();
                    c.Id = 0;
                    c.Name = "Select Client";
                    cl.Add(c);
                    var orderedClients = cl.OrderBy(s => s.Id);
                    var clientlist = new SelectList(orderedClients, "Id", "Name");
                    ViewBag.ClientsList = clientlist;
                    
                    ViewBag.SelectedClientId = null;
                    return View(timesheets);
                }
                else
                    return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
            }
            var list = new List<Timesheet>();

            return View(list);
        }

        public ActionResult Timesheetsbyperiod(DateTime start_date, DateTime end_date) //IMPORTANT UPDATE ALL FOLLOWING METHODS (Dashboard, TimeSheetsByBranch, TimeSheetsByClient, Timesheetsbyperiod)
        {
            try
            {
                var db = new TSS_Sql_Entities();

                List<Branch> bl = db.Branches.ToList();
                Branch b = new Branch();
                b.Id = 0;
                b.Name = "Select Branch";
                bl.Add(b);
                var a = bl.OrderBy(s => s.Id);
                var branchlist = new SelectList(a, "Id", "Name");
                ViewBag.BranchsList = branchlist;
                var timesheets = db.Timesheets.Where(s => s.End_date >= start_date && s.End_date <= end_date).ToList();
                List<Customer> cl = db.Customers.ToList();
                Customer c = new Customer();
                c.Id = 0;
                c.Name = "Select Client";
                cl.Add(c);
                var orderedClients = cl.OrderBy(s => s.Id);
                var clientlist = new SelectList(orderedClients, "Id", "Name");
                ViewBag.ClientsList = clientlist;

                ViewBag.SelectedBranchId = null;
                ViewBag.SelectedClientId = null;
                return View(timesheets);

            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
            }
            var list = new List<Timesheet>();

            return View(list);
        }



        public ActionResult TimeSheetsByClient(int id) //IMPORTANT UPDATE ALL FOLLOWING METHODS (Dashboard, TimeSheetsByBranch, TimeSheetsByClient, Timesheetsbyperiod)
        {
            try
            {
                var db = new TSS_Sql_Entities();

                List<Branch> bl = db.Branches.ToList();
                Branch b = new Branch();
                b.Id = 0;
                b.Name = "Select Branch";
                bl.Add(b);
                var orderedbranches = bl.OrderBy(s => s.Id);
                var branchlist = new SelectList(orderedbranches, "Id", "Name");
                ViewBag.BranchsList = branchlist;
                var timesheets = db.Timesheets.Where(s => s.Customer_id == id).ToList();
                List<Customer> cl = db.Customers.ToList();
                Customer c = new Customer();
                c.Id = 0;
                c.Name = "Select Client";
                cl.Add(c);
                var orderedClients = cl.OrderBy(s => s.Id);
                var clientlist = new SelectList(orderedClients, "Id", "Name");
                ViewBag.ClientsList = clientlist;

                ViewBag.SelectedBranchId = null;
                ViewBag.SelectedClientId = id;

                return View(timesheets);

            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
            }
            var list = new List<Timesheet>();

            return View(list);
        }


        public JsonResult GetClientEmails(int timesheetId)
        {
            var db = new TSS_Sql_Entities();
            var timesheet = db.Timesheets.FirstOrDefault(s => s.Id == timesheetId);
            //var availableUsers = db.AspNetUsers.Where(s => s.Customer_id == timesheet.Customer_Id_Generic).ToList();
           // var availableEmails = availableUsers.Select(s => s.Email).ToList();

            var availableAddresses = db.CustomerContacts.Where(s => s.Customer_id == timesheet.Customer_Id_Generic).ToList();
            var availableEmails = availableAddresses.Select(s => s.Email_id).ToList();

            if (availableEmails.Count < 1)
            {
                availableEmails.Add("No Email Available");
            }
            return Json(availableEmails, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetRejectionReason(int timesheetId)
        {
            var db = new TSS_Sql_Entities();
            var reason = db.RejectedTimesheets.FirstOrDefault(s => s.TimeSheetId == timesheetId);
            return Json(reason, JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Dashboard(DateTime? start_date, DateTime? end_date,int? branch_id, int? client_id) //IMPORTANT UPDATE ALL FOLLOWING METHODS (Dashboard, TimeSheetsByBranch, TimeSheetsByClient, Timesheetsbyperiod)
        {
            try
            {
                branch_id = (branch_id == 0)  ? null  : branch_id;
                client_id = (client_id == 0)  ? null : client_id;

                var db = new TSS_Sql_Entities();
                List<Branch> bl = db.Branches.ToList();
                Branch b = new Branch();
                b.Id = 0;
                b.Name = "Select Branch";
                bl.Add(b);
                var orderedbranches = bl.OrderBy(s => s.Id);
                var branchlist = new SelectList(orderedbranches, "Id", "Name");
                ViewBag.BranchsList = branchlist;

                List<Customer> cl = db.Customers.ToList();
                Customer c = new Customer();
                c.Id = 0;
                c.Name = "Select Client";
                cl.Add(c);
                var orderedClients = cl.OrderBy(s => s.Id);
                var clientlist = new SelectList(orderedClients, "Id", "Name");
                ViewBag.ClientsList = clientlist;
                List<Timesheet> timesheets = new List<Timesheet>();

                //ViewBag.RejectedTimeSheets = db.RejectedTimesheets.ToList();

                if (end_date != null && start_date != null && branch_id != null && client_id != null)
                {
                    timesheets = db.Timesheets.Where(s => s.End_date >= start_date && s.End_date <= end_date && s.Customer_id == client_id && s.Customer.Branch_id == branch_id).ToList();

                    ViewBag.SelectedBranchId = branch_id;
                    ViewBag.SelectedClientId = client_id;
                }
                else if (start_date != null && end_date != null && client_id != null)
                {
                    timesheets = db.Timesheets.Where(s => s.End_date >= start_date && s.End_date <= end_date && s.Customer_id == client_id).ToList();

                    ViewBag.SelectedBranchId = null;
                    ViewBag.SelectedClientId = client_id;
                }
                else if (branch_id != null && end_date != null && start_date != null )
                {
                    timesheets = db.Timesheets.Where(s => s.End_date >= start_date && s.End_date <= end_date && s.Customer.Branch_id == branch_id).ToList();

                    ViewBag.SelectedBranchId = branch_id;
                    ViewBag.SelectedClientId = null;
                }
                else if (branch_id != null && client_id != null)
                {
                    timesheets = db.Timesheets.Where(s => s.Customer.Branch_id == branch_id && s.Customer_id == client_id).ToList();
                    ViewBag.SelectedBranchId = branch_id;
                    ViewBag.SelectedClientId = client_id;
                }
                else if (end_date != null && start_date != null)
                {
                    timesheets = db.Timesheets.Where(s => s.End_date >= start_date && s.End_date <= end_date).ToList();
                    ViewBag.SelectedBranchId = null;
                    ViewBag.SelectedClientId = null;
                }
                else if (branch_id != null)
                {
                    timesheets = db.Timesheets.Where(s => s.Customer.Branch_id == branch_id).ToList();
                    ViewBag.SelectedBranchId = branch_id;
                    ViewBag.SelectedClientId = null;
                }
                else if (client_id != null)
                {
                    timesheets = db.Timesheets.Where(s => s.Customer_id == client_id).ToList();
                    ViewBag.SelectedBranchId = null;
                    ViewBag.SelectedClientId = client_id;
                }
                else
                {
                    timesheets = db.Timesheets.ToList();
                    ViewBag.SelectedBranchId = null;
                    ViewBag.SelectedClientId = null;
                }




                return View("Dashboard",timesheets);
            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
            }
            var list = new List<Timesheet>();

            return View(list);
        }


       



    }
}