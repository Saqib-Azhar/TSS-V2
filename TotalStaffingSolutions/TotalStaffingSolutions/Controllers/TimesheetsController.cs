using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using TotalStaffingSolutions.Models;
using Excel = Microsoft.Office.Interop.Excel;

namespace TotalStaffingSolutions.Controllers
{
    public class TimesheetsController : Controller
    {
        private static TSS_Sql_Entities db = new TSS_Sql_Entities();
        private static List<Employee> EmployeesStaticList = null;
        private static string SenderEmailId = WebConfigurationManager.AppSettings["DefaultEmailId"];
        private static string SenderEmailPassword = WebConfigurationManager.AppSettings["DefaultEmailPassword"];
        private static int SenderEmailPort = Convert.ToInt32(WebConfigurationManager.AppSettings["DefaultEmailPort"]);
        private static string SenderEmailHost = WebConfigurationManager.AppSettings["DefaultEmailHost"];
        private static string TSSLiveSiteURL = WebConfigurationManager.AppSettings["TSSLiveSiteURL"];
        private static string LogoPath = WebConfigurationManager.AppSettings["LogoPath"];
        private enum WeekDays
        {
            Sun = 0,
            Mon = 1,
            Tue = 2,
            Wed = 3,
            Thu = 4,
            Fri = 5,
            Sat = 6
        }


        #region AddTimesheetRegion
        [Authorize(Roles = "Admin")]
        public ActionResult AddTimeSheet(Int32 CustomerId)
        {
            if (EmployeesStaticList == null)
            {
                EmployeesStaticList = db.Employees.ToList();
            }
            ViewBag.PONumbers = db.Po_Numbers.Where(s => s.ClientId == CustomerId).ToList();
            return View(db.Customers.FirstOrDefault(a => a.Id == CustomerId));
        }



        [Authorize]
        public JsonResult AddTimeSheetDetails(Timesheet timesheet, List<Timesheet_summaries> timeSheet_summary)/*, List<Timesheet_details> timeSheet_DetailsList)*/
        {
            try
            {
                if (timeSheet_summary.Count == 1 && timeSheet_summary[0].Employee_id == null)
                {
                    return Json("Please fill the form first.", JsonRequestBehavior.AllowGet);
                }
                timesheet.Created_at = DateTime.Now;

                //var db = new TSS_Sql_Entities();
                var NewTimeSheet = new Timesheet();
                NewTimeSheet.Created_at = timesheet.Created_at;
                NewTimeSheet.Customer_id = timesheet.Customer_id;
                NewTimeSheet.End_date = timesheet.End_date;
                NewTimeSheet.For_internal_employee = timesheet.For_internal_employee;
                NewTimeSheet.Note = timesheet.Note;
                NewTimeSheet.Organization_id = timesheet.Organization_id;
                NewTimeSheet.Po_number = timesheet.Po_number;
                NewTimeSheet.Sent = timesheet.Sent;
                NewTimeSheet.Signature = timesheet.Signature;
                NewTimeSheet.Total_employees = timesheet.Total_employees;
                NewTimeSheet.Total_hours = timesheet.Total_hours;
                NewTimeSheet.Updated_at = timesheet.Updated_at;
                NewTimeSheet.Submit_by_client = false;
                NewTimeSheet.Sent = false;
                NewTimeSheet.Created_By = User.Identity.GetUserId();
                var customerIdTs = timesheet.Customer_id;
                var customer = db.Customers.FirstOrDefault(s => s.Id == customerIdTs);
                if (customer != null)
                    NewTimeSheet.Customer_Id_Generic = customer.Customer_id;
                NewTimeSheet.Status_id = 1;
                var checkPo = db.Po_Numbers.FirstOrDefault(s => s.Client_Generic_Id == NewTimeSheet.Customer_Id_Generic && s.PoNumber == timesheet.Po_number);
                if (checkPo == null)
                {
                    Po_Numbers newPONo = new Po_Numbers();
                    newPONo.ClientId = timesheet.Customer_id;
                    newPONo.Client_Generic_Id = NewTimeSheet.Customer_Id_Generic;
                    newPONo.PoNumber = timesheet.Po_number;
                    db.Po_Numbers.Add(newPONo);
                }
                db.Timesheets.Add(NewTimeSheet);
                db.SaveChanges();

                //var timesheetObj = db.Timesheets.FirstOrDefault(s => s.Created_at == timesheet.Created_at && s.Customer_id == timesheet.Customer_id);
                var timesheetObj = db.Timesheets.OrderByDescending(s => s.Id).FirstOrDefault(s => s.Id == NewTimeSheet.Id);

                foreach (var item in timeSheet_summary)
                {
                    item.Created_at = DateTime.Now;
                    var NewTimeSheetSummary = new Timesheet_summaries();
                    //NewTimeSheetSummary.Created_at = timeSheet_summary.Created_at;
                    NewTimeSheetSummary.Employee_id = item.Employee_id;
                    NewTimeSheetSummary.Enitial = item.Enitial;
                    NewTimeSheetSummary.Timesheet_id = timesheetObj.Id;
                    NewTimeSheetSummary.Rate = item.Rate;
                    NewTimeSheetSummary.Enitial = item.Enitial;
                    NewTimeSheetSummary.Total_hours = item.Total_hours;
                    NewTimeSheetSummary.Created_at = DateTime.Now;
                    NewTimeSheetSummary.Updated_at = DateTime.Now;
                    NewTimeSheetSummary.Rating_by_client = 0;
                    NewTimeSheetSummary.Hours_day_1 = item.Hours_day_1;
                    NewTimeSheetSummary.Hours_day_2 = item.Hours_day_2;
                    NewTimeSheetSummary.Hours_day_3 = item.Hours_day_3;
                    NewTimeSheetSummary.Hours_day_4 = item.Hours_day_4;
                    NewTimeSheetSummary.Hours_day_5 = item.Hours_day_5;
                    NewTimeSheetSummary.Hours_day_6 = item.Hours_day_6;
                    NewTimeSheetSummary.Hours_day_7 = item.Hours_day_7;
                    NewTimeSheetSummary.Starting_date = item.Starting_date;
                    NewTimeSheetSummary.Ending_date = item.Ending_date;
                    NewTimeSheetSummary.Ending_day_of_week = item.Ending_day_of_week;
                    NewTimeSheetSummary.Starting_day_of_week = item.Starting_day_of_week;

                    db.Timesheet_summaries.Add(NewTimeSheetSummary);
                    db.SaveChanges();
                }

                //foreach (var item in timeSheet_DetailsList)
                //{
                //    item.Created_at = DateTime.Now;

                //    var NewTimeSheetDetailsObj = new Timesheet_details();
                //    NewTimeSheetDetailsObj.Created_at = item.Created_at;
                //    NewTimeSheetDetailsObj.Updated_at = item.Created_at;
                //    NewTimeSheetDetailsObj.Day = item.Day;
                //    NewTimeSheetDetailsObj.Employee_id = item.Employee_id;
                //    NewTimeSheetDetailsObj.Hours = item.Hours;
                //    NewTimeSheetDetailsObj.Timesheet_id = timesheetObj.Id;
                //    db.Timesheet_details.Add(NewTimeSheetDetailsObj);
                //    db.SaveChanges();
                //}


                var timesheetId = NewTimeSheet.Id.ToString();
                return Json(timesheetId, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                return Json("failure", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region ViewTimesheetRegion


        [Authorize]
        public ActionResult TimeSheetDetails(int id)
        {
            TimesheetObjectTuple timeSheetDetailsTuple = new TimesheetObjectTuple();
            timeSheetDetailsTuple.TimeSheetGeneralDetails = db.Timesheets.Find(id);
            timeSheetDetailsTuple.TimeSheetSummary = db.Timesheet_summaries.Where(s => s.Timesheet_id == id).ToList();
            //var timeSheetDetailsList = db.Timesheet_details.Where(x => x.Timesheet_id == id).ToList();
            //timeSheetDetailsTuple.TimeSheetDetails = timeSheetDetailsList;

            if (User.IsInRole("User"))
            {
                var contact = db.CustomerContacts.FirstOrDefault(s => s.Customer_id == timeSheetDetailsTuple.TimeSheetGeneralDetails.Customer_Id_Generic);

                string initials = "";
                contact.Contact_name.Split(' ').ToList().ForEach(i => initials = initials + i[0]);
                ViewBag.Initials = initials;
            }
            else
            {
                ViewBag.Initials = "";
            }

            try
            {
                var userObject = db.AspNetUsers.FirstOrDefault(s => s.Customer_id == timeSheetDetailsTuple.TimeSheetGeneralDetails.Customer_Id_Generic);
                ViewBag.DisplayPicture = userObject.DisplayPicture;
            }
            catch (Exception)
            {
                ViewBag.DisplayPicture = "";
            }
            return View(timeSheetDetailsTuple);
        }



        public JsonResult SubmitTimeSheetDetails(Timesheet timesheet, List<Timesheet_summaries> timeSheet_summary)
        {

            try
            {
                timesheet.Created_at = DateTime.Now;
                
                var NewTimeSheet = db.Timesheets.FirstOrDefault(s => s.Id == timesheet.Id);
                //NewTimeSheet.Created_at = timesheet.Created_at;
                //NewTimeSheet.Customer_id = timesheet.Customer_id;
                //NewTimeSheet.End_date = timesheet.End_date;
                //NewTimeSheet.For_internal_employee = timesheet.For_internal_employee;
                NewTimeSheet.Note = timesheet.Note;
                //NewTimeSheet.Organization_id = timesheet.Organization_id;
                //NewTimeSheet.Po_number = timesheet.Po_number;
                //NewTimeSheet.Sent = timesheet.Sent;
                NewTimeSheet.Signature = timesheet.Signature;
                NewTimeSheet.Total_employees = timesheet.Total_employees;
                NewTimeSheet.Total_hours = timesheet.Total_hours;
                NewTimeSheet.Updated_at = DateTime.Now;
                NewTimeSheet.Submit_by_client = true;
                NewTimeSheet.Sent = true;
                //var customer = db.Customers.FirstOrDefault(s => s.Id == timesheet.Customer_id);
                //if (customer != null)
                //    NewTimeSheet.Customer_Id_Generic = customer.Customer_id;
                NewTimeSheet.Status_id = 3;

                // db.Timesheets.Add(NewTimeSheet);
                db.SaveChanges();

                //var timesheetObj = db.Timesheets.FirstOrDefault(s => s.Created_at == timesheet.Created_at && s.Customer_id == timesheet.Customer_id);
                var timesheetObj = db.Timesheets.OrderByDescending(s => s.Created_at).FirstOrDefault(s => s.Customer_id == timesheet.Customer_id);

                foreach (var item in timeSheet_summary)
                {
                    var NewTimeSheetSummary = db.Timesheet_summaries.FirstOrDefault(s => s.Id == item.Id);
                    NewTimeSheetSummary.Enitial = item.Enitial;
                    NewTimeSheetSummary.Rate = item.Rate;
                    NewTimeSheetSummary.Total_hours = item.Total_hours;
                    NewTimeSheetSummary.Updated_at = DateTime.Now;
                    NewTimeSheetSummary.Rating_by_client = item.Rating_by_client;
                    NewTimeSheetSummary.Hours_day_1 = item.Hours_day_1;
                    NewTimeSheetSummary.Hours_day_2 = item.Hours_day_2;
                    NewTimeSheetSummary.Hours_day_3 = item.Hours_day_3;
                    NewTimeSheetSummary.Hours_day_4 = item.Hours_day_4;
                    NewTimeSheetSummary.Hours_day_5 = item.Hours_day_5;
                    NewTimeSheetSummary.Hours_day_6 = item.Hours_day_6;
                    NewTimeSheetSummary.Hours_day_7 = item.Hours_day_7;
                    db.SaveChanges();
                }

               


                ///////////////////////////////////ADMIN EMAIL UPDATE/////////////////////////////////////
                #region ADMIN EMAIL UPDATE
                if (User.IsInRole("User"))
                {
                    var AdminId = NewTimeSheet.Created_By;
                    var admin = db.AspNetUsers.FirstOrDefault(s => s.Id == AdminId);
                    var user = db.AspNetUsers.Find(User.Identity.GetUserId());
                    try
                    {
                        var fromAddress = new MailAddress(SenderEmailId, "Total Staffing Solution");
                        var toAddress = new MailAddress("sazhar@viretechnologies.com", admin.Email);
                        string fromPassword = SenderEmailPassword;
                        string subject = "Total Staffing Solution: Timesheet Update";
                        string body = "<b>Hello " + admin.UserName + "!</b><br />Client has submitted the timesheet<br /> <a href='" + TSSLiveSiteURL + "/Timesheets/TimeSheetDetails/" + timesheet.Id + "'>Timesheet Link</a><br />Thanks for joining and have a great day! <br />Total Staffing Solutions";

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
                    }
                    catch (Exception ex)
                    {

                        ExceptionHandlerController.infoMessage(ex.Message);
                        ExceptionHandlerController.writeErrorLog(ex);
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////
                    /////////////////////////////////////////////////////////////////////////////////////////
                    /////////////////////////////////////////////////////////////////////////////////////////
                    try
                    {
                        var fromAddress = new MailAddress(SenderEmailId, "Total Staffing Solution");
                        var toAddress = new MailAddress("sazhar@viretechnologies.com", user.Email);
                        string fromPassword = SenderEmailPassword;
                        string subject = "Total Staffing Solution: Timesheet Update";
                        string body = "<b>Hello " + user.UserName + "!</b><br />Following timesheet has been submitted<br />"
                            + "<a href='" + TSSLiveSiteURL + "/Timesheets/TimeSheetDetails/" + timesheet.Id + "'>"
                            + "Timesheet Link</a><br />Week Ending Date: " + timesheet.End_date
                            + "<br />Total Employees:" + timesheet.Total_employees
                            + "<br />Total Hours:" + timesheet.Total_hours
                            + "<br />Thanks for joining and have a great day! <br />Total Staffing Solutions";


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
                        ///


                    }
                    catch (Exception ex)
                    {

                        ExceptionHandlerController.infoMessage(ex.Message);
                        ExceptionHandlerController.writeErrorLog(ex);
                    }
                }
                #endregion
                //////////////////////////////////////////////////////////////////////////////////////////
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                return Json("failure", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region DeleteTimesheetRegion

        public JsonResult DeleteTimeSheet(int id)
        {
            var db = new TSS_Sql_Entities();
            try
            {
                var timesheet = db.Timesheets.Find(id);

                var timesheetSummariesList = db.Timesheet_summaries.Where(s => s.Timesheet_id == id).ToList();
                

                foreach (var item in timesheetSummariesList)
                {
                    db.Timesheet_summaries.Remove(item);
                }

                db.Timesheets.Remove(timesheet);


                db.SaveChanges();
                return Json("Timesheet Deleted successfully", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                return Json("Something Went wrong..!", JsonRequestBehavior.AllowGet);

            }


        }
        #endregion


        #region CopyTimesheetRegion

        public JsonResult CopyTimeSheet(int id)
        {
            try
            {

                if (EmployeesStaticList == null)
                {
                    EmployeesStaticList = db.Employees.ToList();

                }
                TimesheetObjectTuple timeSheetDetailsTuple = new TimesheetObjectTuple();
                timeSheetDetailsTuple.TimeSheetGeneralDetails = db.Timesheets.Find(id);
                timeSheetDetailsTuple.TimeSheetSummary = db.Timesheet_summaries.Where(s => s.Timesheet_id == id).ToList();
                timeSheetDetailsTuple.TimeSheetGeneralDetails.Signature = "";
                timeSheetDetailsTuple.TimeSheetGeneralDetails.Note = "";
                AddTimeSheetDetails(timeSheetDetailsTuple.TimeSheetGeneralDetails, timeSheetDetailsTuple.TimeSheetSummary);


                return Json("Copy of Timesheet created successfully");
            }
            catch (Exception)
            {

                return Json("Something Went wrong.");
            }
        }


        #endregion


        #region ExportTimesheetRegion
        public bool ExportInExcel(string ids)
        {
            try
            {

                var deserialized = Regex.Split(ids, ",");

                var grid = new GridView();
                var db = new TSS_Sql_Entities();
                List<TimeSheetSummaryTuple> summariesList = new List<TimeSheetSummaryTuple>();
                TimesheetObjectTuple timeSheetDetailsTuple = new TimesheetObjectTuple();
                foreach (var item in deserialized)
                {
                    if (item == "")
                        continue;
                    var id = Convert.ToInt32(item);
                    timeSheetDetailsTuple.TimeSheetGeneralDetails = db.Timesheets.Find(id);
                    timeSheetDetailsTuple.TimeSheetSummary = db.Timesheet_summaries.Where(s => s.Timesheet_id == id).ToList();
                    foreach (var summary in timeSheetDetailsTuple.TimeSheetSummary)
                    {
                        TimeSheetSummaryTuple summaryItem = new Models.TimeSheetSummaryTuple();
                        summaryItem.TimeSheetSummary = summary;
                        summaryItem.PO_Number = timeSheetDetailsTuple.TimeSheetGeneralDetails.Po_number;
                        summariesList.Add(summaryItem);
                    }
                    
                }


                grid.DataSource = from d in summariesList
                                  select new
                                  {
                                      Timeslip_ID = "000000"+d.TimeSheetSummary.Timesheet_id,
                                      Job_Order_Number = "",
                                      Customer_ID = d.TimeSheetSummary.Timesheet.Customer_Id_Generic,
                                      Customer_Name = d.TimeSheetSummary.Timesheet.Customer.Name,
                                      Site_Code = "",
                                      Employee_ID = d.TimeSheetSummary.Employee_id,
                                      Employee_Last_Name = d.TimeSheetSummary.Employee.Last_name,
                                      Rate_Code = d.TimeSheetSummary.Rate,
                                      Work_Date = "",
                                      Batch_Date = d.TimeSheetSummary.Timesheet.End_date,
                                      hour_Type = "",
                                      Regular_Pay_hours = d.TimeSheetSummary.Total_hours,
                                      Regular_Pay_Rate = d.TimeSheetSummary.Rate,
                                      Regular_Bill_hours = "",
                                      Regular_Bill_Rate = "",
                                      Overtime_Pay_hours = "",
                                      Overtime_Pay_Rate = "",
                                      Overtime_Bill_hours = "",
                                      Overtime_Bill_Rate = "",
                                      Double_Time_Pay_hours = "",
                                      Double_Time_Pay_Rate = "",
                                      Double_Time_Bill_hours = "",
                                      Double_Time_Bill_Rate = "",
                                      Comp_Code = "",
                                      Sales_Tax_Code = "",
                                      PO_Number = d.PO_Number,
                                      Release = "",
                                      Project = "",
                                      Department_Code = "",
                                      Office_Code = "",
                                      Location_Code = "",
                                      Saleman_1_Code = "",
                                      Salesman_2_Code = "",
                                      Pay_Frequency = "",
                                      Number_of_Days = "",
                                      Pay_hold = "",
                                      Bill_hold = "",
                                      Separate_Check = "",
                                      Misc_Pay = "",
                                      Amount_1 = "",
                                      Misc_Bill_1 = "",
                                      Misc_Pay_Amount_2 = "",
                                      Misc_Bill_2 = "",
                                      Misc_Pay_Amount_3 = "",
                                      Misc_Bill_3 = "",
                                      Misc_Pay_Amount_4 = "",
                                      Misc_Bill_4 = "",
                                      Misc_Pay_Amount_5 = "",
                                      Misc_Bill_5 = "",
                                      Misc_Pay_Amount_6 = "",
                                      Misc_Bill_6 = "",
                                      Misc_Pay_Amount_7 = "",
                                      Misc_Bill_7 = "",
                                      Misc_Pay_Amount_8 = "",
                                      Misc_Bill_8 = "",
                                      Misc_Pay_Amount_9 = "",
                                      Misc_Bill_9 = "",
                                      Permanent_TimeSlip = "",
                                      Expires_On = "",

                                  };
                
                grid.DataBind();
                

                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=TimeSheet.xls");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Write(sw.ToString());

                Response.End();



                return true;

            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                return false;

            }
        }



        #region ExportToPDFRegion
        public static iTextSharp.text.Font GetCalibri()
        {
            var fontName = "Calibri";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = (TSSLiveSiteURL+"\\assets\\fonts\\Calibri.ttf").Replace('\\','/');
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        }

        public FileResult ExportInPDF(string ids)
        {
            try
            {

                var deserialized = Regex.Split(ids, ",");
                var grid = new GridView();
                var db = new TSS_Sql_Entities();
                MemoryStream workStream = new MemoryStream();
                StringBuilder status = new StringBuilder("");
                DateTime dTime = DateTime.Now;
                //file name to be created   
                string strPDFFileName = string.Format("TSSTimeSheet" + dTime.ToString("yyyyMMdd") + "-" + ".pdf");

                Document doc = new Document();
                doc.SetMargins(10, 10, 10, 10);
                doc.PageCount = deserialized.Count() - 1;
                //doc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                //doc.SetMargins(10, 10, 10, 10);
                string strAttachment = Server.MapPath("~/Downloadss/" + strPDFFileName);

                PdfWriter.GetInstance(doc, workStream).CloseStream = false;
                doc.Open();
                var a =GetCalibri();
                 int counter = 0;
                foreach (var item in deserialized)
                {
                    counter++;
                    if (item == "")
                        continue;
                    
                    PdfPTable tableLayout = new PdfPTable(13);
                    doc.Add(Add_Content_To_PDF(tableLayout, item));

                    doc.NewPage();


                }
               
                // Closing the document  
                doc.Close();

                byte[] byteInfo = workStream.ToArray();
                workStream.Write(byteInfo, 0, byteInfo.Length);
                workStream.Position = 0;
                return File(workStream, "application/pdf", strPDFFileName);

                //return true;

            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                // return false;
                return File("application/pdf", "a");
            }
        }
        protected PdfPTable Add_Content_To_PDF(PdfPTable tableLayout, string deserialized)
        {
            try
            {

                float[] headers = { 10, 10, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 10 }; //Header Widths  
                tableLayout.SetWidths(headers); //Set the pdf headers  
                tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
                tableLayout.HeaderRows = 1;
                //Add Title to the PDF file at the top  
                int tsid = Convert.ToInt32(deserialized);
                var TimesheetSummaries = db.Timesheet_summaries.Where(t => t.Timesheet_id == tsid).ToList();
                string CustomerDetails = TimesheetSummaries[0].Timesheet.Customer.Name + "-" + TimesheetSummaries[0].Timesheet.Customer.Id;
                if (TimesheetSummaries[0].Timesheet.Customer.Address1 != "")
                {
                    CustomerDetails = CustomerDetails +
                    "\n" + TimesheetSummaries[0].Timesheet.Customer.Address1;
                }
                if (TimesheetSummaries[0].Timesheet.Customer.Address2 != "")
                {
                    CustomerDetails = CustomerDetails + "\n" +
                    TimesheetSummaries[0].Timesheet.Customer.Address2;


                }
                if (TimesheetSummaries[0].Timesheet.Customer.PhoneNumber != "")
                {
                    CustomerDetails = CustomerDetails + "\n\n" +
                    TimesheetSummaries[0].Timesheet.Customer.PhoneNumber;

                }
                if (TimesheetSummaries[0].Timesheet.End_date != null)
                {
                    CustomerDetails = CustomerDetails + "\nWeek Ending:" +
                    TimesheetSummaries[0].Timesheet.End_date.Value.ToString("MM/dd/yyyy");
                }
                CustomerDetails = CustomerDetails +"\n\n" ;

                tableLayout.AddCell(new PdfPCell(new Phrase("\n\n", new Font(Font.FontFamily.UNDEFINED, 14, Font.NORMAL, iTextSharp.text.BaseColor.BLACK)))
                {
                    Colspan = 13,
                    Border = 0,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)

                });

                //tableLayout.AddCell(new PdfPCell(new Phrase("Total Staffing Solutions", new Font(Font.FontFamily.TIMES_ROMAN, 16, 2, new iTextSharp.text.BaseColor(0, 0, 0))))
                //{
                //    Colspan = 6,
                //    Border = 0,
                //    PaddingBottom = 5,
                //    HorizontalAlignment = Element.ALIGN_CENTER
                //});
                tableLayout.AddCell(createImageCell());
                tableLayout.AddCell(new PdfPCell(new Phrase(CustomerDetails, new Font(Font.FontFamily.UNDEFINED, 20, Font.NORMAL, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 7,
                    Border = 0,
                    PaddingBottom = 5,
                    PaddingLeft = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });



                tableLayout.AddCell(new PdfPCell(new Phrase("__________________________________________________________________________________________________________________\n\n", new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, iTextSharp.text.BaseColor.BLACK)))
                {
                    Colspan = 13,
                    Border = 0,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)

                });

                Array values = Enum.GetValues(typeof(WeekDays));


                ////Add header  
                AddCellToHeader(tableLayout, "Last Name");
                AddCellToHeader(tableLayout, "First Name");
                AddCellToHeader(tableLayout, "Emp#");
                AddCellToHeader(tableLayout, "RT");
                var weekDay = Convert.ToInt32(TimesheetSummaries[0].Starting_day_of_week);
                AddCellToHeader(tableLayout, values.GetValue(weekDay).ToString() + "\n" + TimesheetSummaries[0].Starting_date.Value.ToString("MMM/dd") + "\n\n",10);
                weekDay = (weekDay == 6) ? 0 : weekDay + 1;
                AddCellToHeader(tableLayout, values.GetValue(weekDay).ToString() + "\n" + TimesheetSummaries[0].Starting_date.Value.AddDays(1).ToString("MMM/dd") + "\n\n",10);
                weekDay = (weekDay == 6) ? 0 : weekDay + 1;
                AddCellToHeader(tableLayout, values.GetValue(weekDay).ToString() + "\n" + TimesheetSummaries[0].Starting_date.Value.AddDays(2).ToString("MMM/dd") + "\n\n",10);
                weekDay = (weekDay == 6) ? 0 : weekDay + 1;
                AddCellToHeader(tableLayout, values.GetValue(weekDay).ToString() + "\n" + TimesheetSummaries[0].Starting_date.Value.AddDays(3).ToString("MMM/dd") + "\n\n",10);
                weekDay = (weekDay == 6) ? 0 : weekDay + 1;
                AddCellToHeader(tableLayout, values.GetValue(weekDay).ToString() + "\n" + TimesheetSummaries[0].Starting_date.Value.AddDays(4).ToString("MMM/dd") + "\n\n",10);
                weekDay = (weekDay == 6) ? 0 : weekDay + 1;
                AddCellToHeader(tableLayout, values.GetValue(weekDay).ToString() + "\n" + TimesheetSummaries[0].Starting_date.Value.AddDays(5).ToString("MMM/dd") + "\n\n",10);
                weekDay = (weekDay == 6) ? 0 : weekDay + 1;
                AddCellToHeader(tableLayout, values.GetValue(weekDay).ToString() + "\n" + TimesheetSummaries[0].Starting_date.Value.AddDays(6).ToString("MMM/dd") + "\n\n",10);
                AddCellToHeader(tableLayout, "Total");
                AddCellToHeader(tableLayout, "Rate Performance",10);

                ////Add body  
                bool b = true;

                int TotalDay1 = 0;
                int TotalDay2 = 0;
                int TotalDay3 = 0;
                int TotalDay4 = 0;
                int TotalDay5 = 0;
                int TotalDay6 = 0;
                int TotalDay7 = 0;
                int TotalHours = 0;

                int TotalDayEmp1 = 0;
                int TotalDayEmp2 = 0;
                int TotalDayEmp3 = 0;
                int TotalDayEmp4 = 0;
                int TotalDayEmp5 = 0;
                int TotalDayEmp6 = 0;
                int TotalDayEmp7 = 0;
                int TotalEmps = 0;

                foreach (var ts in TimesheetSummaries)
                {


                    AddCellToBody(tableLayout, ts.Employee.Last_name, b);
                    AddCellToBody(tableLayout, ts.Employee.First_name, b);
                    AddCellToBody(tableLayout, ts.Employee_id.ToString(), b);
                    AddCellToBody(tableLayout, ts.Rate, b);
                    AddCellToBody(tableLayout, ts.Hours_day_1.ToString(), b);
                    AddCellToBody(tableLayout, ts.Hours_day_2.ToString(), b);
                    AddCellToBody(tableLayout, ts.Hours_day_3.ToString(), b);
                    AddCellToBody(tableLayout, ts.Hours_day_4.ToString(), b);
                    AddCellToBody(tableLayout, ts.Hours_day_5.ToString(), b);
                    AddCellToBody(tableLayout, ts.Hours_day_6.ToString(), b);
                    AddCellToBody(tableLayout, ts.Hours_day_7.ToString(), b);
                    AddCellToBody(tableLayout, ts.Total_hours.ToString(), b);
                    AddCellToBody(tableLayout, ts.Rating_by_client.ToString(), b);

                    TotalDay1 = TotalDay1 + Convert.ToInt32(ts.Hours_day_1);
                    TotalDay2 = TotalDay2 + Convert.ToInt32(ts.Hours_day_2);
                    TotalDay3 = TotalDay3 + Convert.ToInt32(ts.Hours_day_3);
                    TotalDay4 = TotalDay4 + Convert.ToInt32(ts.Hours_day_4);
                    TotalDay5 = TotalDay5 + Convert.ToInt32(ts.Hours_day_5);
                    TotalDay6 = TotalDay6 + Convert.ToInt32(ts.Hours_day_6);
                    TotalDay7 = TotalDay7 + Convert.ToInt32(ts.Hours_day_7);
                    TotalHours = TotalHours + Convert.ToInt32(ts.Total_hours);

                    TotalDayEmp1 = (ts.Hours_day_1 > 0) ? (TotalDayEmp1 + 1) : TotalDayEmp1;
                    TotalDayEmp2 = (ts.Hours_day_2 > 0) ? (TotalDayEmp2 + 1) : TotalDayEmp2;
                    TotalDayEmp3 = (ts.Hours_day_3 > 0) ? (TotalDayEmp3 + 1) : TotalDayEmp3;
                    TotalDayEmp4 = (ts.Hours_day_4 > 0) ? (TotalDayEmp4 + 1) : TotalDayEmp4;
                    TotalDayEmp5 = (ts.Hours_day_5 > 0) ? (TotalDayEmp5 + 1) : TotalDayEmp5;
                    TotalDayEmp6 = (ts.Hours_day_6 > 0) ? (TotalDayEmp6 + 1) : TotalDayEmp6;
                    TotalDayEmp7 = (ts.Hours_day_7 > 0) ? (TotalDayEmp7 + 1) : TotalDayEmp7;
                    TotalEmps = (ts.Total_hours > 0) ? (TotalEmps + 1) : TotalEmps;

                    b = !b;
                }

                tableLayout.AddCell(new PdfPCell(new Phrase("Total Hours", new Font(Font.FontFamily.UNDEFINED, 12, 1, iTextSharp.text.BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 3,
                    Colspan = 4,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
                });
                
                AddCellToBody(tableLayout, TotalDay1.ToString(), false);
                AddCellToBody(tableLayout, TotalDay2.ToString(), false);
                AddCellToBody(tableLayout, TotalDay3.ToString(), false);
                AddCellToBody(tableLayout, TotalDay4.ToString(), false);
                AddCellToBody(tableLayout, TotalDay5.ToString(), false);
                AddCellToBody(tableLayout, TotalDay6.ToString(), false);
                AddCellToBody(tableLayout, TotalDay7.ToString(), false);
                AddCellToBody(tableLayout, TotalHours.ToString(), false);
                AddCellToBody(tableLayout, "", false);

                tableLayout.AddCell(new PdfPCell(new Phrase("No of People", new Font(Font.FontFamily.UNDEFINED, 12, 1, iTextSharp.text.BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Colspan = 4,
                    Padding = 3,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
                });
                

                AddCellToBody(tableLayout, TotalDayEmp1.ToString(), false);
                AddCellToBody(tableLayout, TotalDayEmp2.ToString(), false);
                AddCellToBody(tableLayout, TotalDayEmp3.ToString(), false);
                AddCellToBody(tableLayout, TotalDayEmp4.ToString(), false);
                AddCellToBody(tableLayout, TotalDayEmp5.ToString(), false);
                AddCellToBody(tableLayout, TotalDayEmp6.ToString(), false);
                AddCellToBody(tableLayout, TotalDayEmp7.ToString(), false);
                AddCellToBody(tableLayout, TotalEmps.ToString(), false);
                AddCellToBody(tableLayout, "", false);
                AddCellToFooter(tableLayout, tsid);
                return tableLayout;
            }
            catch (Exception ex)
            {

                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                return tableLayout;

            }

        }


        public static PdfPCell createImageCell()
        {
            String path = TSSLiveSiteURL + LogoPath;
            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(path.Replace('\\', '/'));
            PdfPCell cell = new PdfPCell(img, true)
            {
                Colspan = 6,
                Border = 0,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            return cell;
        }
        // Method to add single cell to the Header  
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText, int fontSize = 12)
        {
            try
            {

                tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.UNDEFINED, fontSize, Font.BOLD, iTextSharp.text.BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 1,
                    BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)

                });
            }
            catch (Exception ex)
            {

                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                throw;
            }

        }
        // Method to add single cell to the Footer  
        private static void AddCellToFooter(PdfPTable tableLayout, int tsid)
        {
            try
            {

                var ts = db.Timesheets.FirstOrDefault(s => s.Id == tsid);

                tableLayout.AddCell(new PdfPCell(new Phrase("\n\n\n\nAuthorized Signature: " + ts.Signature, new Font(Font.FontFamily.UNDEFINED, 16, Font.UNDERLINE, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 13,
                    Border = 0,
                    Padding = 5,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                tableLayout.AddCell(new PdfPCell(new Phrase("Please e-mail to payroll@4tssi.com on Monday’s before 10:00am ", new Font(Font.FontFamily.UNDEFINED, 12, Font.NORMAL, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 13,
                    Border = 0,
                    Padding = 5,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }
            catch (Exception ex)
            {

                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                throw;
            }

        }

        // Method to add single cell to the body  
        private static void AddCellToBody(PdfPTable tableLayout, string cellText, bool color)
        {
            try
            {
                var rowColor = (color) ? new iTextSharp.text.BaseColor(247, 248, 249) : new iTextSharp.text.BaseColor(255, 255, 255);

                tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.UNDEFINED, 12, 1, iTextSharp.text.BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 3,
                    BackgroundColor = rowColor
                });
                
            }
            catch (Exception ex)
            {

                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                throw;
            }

        }

        #endregion




        #endregion


        #region EditTimesheetRegion

        public ActionResult EditTimeSheet(int id)
        {
          
            if (EmployeesStaticList == null)
            {
                EmployeesStaticList = db.Employees.ToList();

            }
            TimesheetObjectTuple timeSheetDetailsTuple = new TimesheetObjectTuple();
            timeSheetDetailsTuple.TimeSheetGeneralDetails = db.Timesheets.Find(id);
            timeSheetDetailsTuple.TimeSheetSummary = db.Timesheet_summaries.Where(s => s.Timesheet_id == id).ToList();
            var userObject = db.AspNetUsers.FirstOrDefault(s => s.Customer_id == timeSheetDetailsTuple.TimeSheetGeneralDetails.Customer_Id_Generic);
            ViewBag.DisplayPicture = userObject.DisplayPicture;
            return View(timeSheetDetailsTuple);
        }



        public JsonResult EditTimeSheetDetails(Timesheet timesheet, List<Timesheet_summaries> timeSheet_summary, List<Timesheet_summaries> timeSheet_summary_NewEntries)
        {

            try
            {
                //timesheet.Created_at = DateTime.Now;


                var NewTimeSheet = db.Timesheets.FirstOrDefault(s => s.Id == timesheet.Id);
                //NewTimeSheet.Created_at = timesheet.Created_at;
                //NewTimeSheet.Customer_id = timesheet.Customer_id;
                //NewTimeSheet.End_date = timesheet.End_date;
                //NewTimeSheet.For_internal_employee = timesheet.For_internal_employee;
                //NewTimeSheet.Note = timesheet.Note;
                //NewTimeSheet.Organization_id = timesheet.Organization_id;
                //NewTimeSheet.Po_number = timesheet.Po_number;
                //NewTimeSheet.Sent = timesheet.Sent;
                NewTimeSheet.Signature = timesheet.Signature;
                if (timeSheet_summary_NewEntries != null)
                    NewTimeSheet.Total_employees = timeSheet_summary == null ? timeSheet_summary_NewEntries.Count : timeSheet_summary_NewEntries.Count + timeSheet_summary.Count;
                else
                    NewTimeSheet.Total_employees = timeSheet_summary.Count;
                //NewTimeSheet.Total_hours = timesheet.Total_hours;
                NewTimeSheet.Updated_at = DateTime.Now;
                NewTimeSheet.Submit_by_client = false;
                NewTimeSheet.Sent = false;
                //var customer = db.Customers.FirstOrDefault(s => s.Id == timesheet.Customer_id);
                //if (customer != null)
                //    NewTimeSheet.Customer_Id_Generic = customer.Customer_id;
               // NewTimeSheet.Status_id = 1;

                // db.Timesheets.Add(NewTimeSheet);
                db.SaveChanges();

                //var timesheetObj = db.Timesheets.FirstOrDefault(s => s.Created_at == timesheet.Created_at && s.Customer_id == timesheet.Customer_id);
                //var timesheetObj = db.Timesheets.OrderByDescending(s => s.Created_at).FirstOrDefault(s => s.Customer_id == timesheet.Customer_id);
                if (timeSheet_summary != null)
                {
                    foreach (var item in timeSheet_summary)
                    {
                        //item.Created_at = DateTime.Now;
                        var NewTimeSheetSummary = db.Timesheet_summaries.FirstOrDefault(s => s.Id == item.Id);
                        NewTimeSheetSummary.Employee_id = item.Employee_id;
                        NewTimeSheetSummary.Enitial = item.Enitial;
                        NewTimeSheetSummary.Rate = item.Rate;
                        NewTimeSheetSummary.Total_hours = item.Total_hours;
                        NewTimeSheetSummary.Updated_at = DateTime.Now;
                        NewTimeSheetSummary.Rating_by_client = item.Rating_by_client;
                        NewTimeSheetSummary.Hours_day_1 = item.Hours_day_1;
                        NewTimeSheetSummary.Hours_day_2 = item.Hours_day_2;
                        NewTimeSheetSummary.Hours_day_3 = item.Hours_day_3;
                        NewTimeSheetSummary.Hours_day_4 = item.Hours_day_4;
                        NewTimeSheetSummary.Hours_day_5 = item.Hours_day_5;
                        NewTimeSheetSummary.Hours_day_6 = item.Hours_day_6;
                        NewTimeSheetSummary.Hours_day_7 = item.Hours_day_7;
                        NewTimeSheetSummary.Starting_date = item.Starting_date;
                        NewTimeSheetSummary.Ending_date = item.Ending_date;
                        NewTimeSheetSummary.Ending_day_of_week = item.Ending_day_of_week;
                        NewTimeSheetSummary.Starting_day_of_week = item.Starting_day_of_week;

                        //db.Timesheet_summaries.Add(NewTimeSheetSummary);
                        db.SaveChanges();
                    }

                }

              


                if (timeSheet_summary_NewEntries != null)
                {


                    foreach (var item in timeSheet_summary_NewEntries)
                    {
                        item.Created_at = DateTime.Now;
                        var NewTimeSheetSummary = new Timesheet_summaries();
                        NewTimeSheetSummary.Employee_id = item.Employee_id;
                        NewTimeSheetSummary.Enitial = item.Enitial;
                        NewTimeSheetSummary.Timesheet_id = timesheet.Id;
                        NewTimeSheetSummary.Rate = item.Rate;
                        NewTimeSheetSummary.Enitial = item.Enitial;
                        NewTimeSheetSummary.Total_hours = item.Total_hours;
                        NewTimeSheetSummary.Created_at = DateTime.Now;
                        NewTimeSheetSummary.Updated_at = DateTime.Now;
                        NewTimeSheetSummary.Rating_by_client = 0;
                        NewTimeSheetSummary.Hours_day_1 = item.Hours_day_1;
                        NewTimeSheetSummary.Hours_day_2 = item.Hours_day_2;
                        NewTimeSheetSummary.Hours_day_3 = item.Hours_day_3;
                        NewTimeSheetSummary.Hours_day_4 = item.Hours_day_4;
                        NewTimeSheetSummary.Hours_day_5 = item.Hours_day_5;
                        NewTimeSheetSummary.Hours_day_6 = item.Hours_day_6;
                        NewTimeSheetSummary.Hours_day_7 = item.Hours_day_7;
                        NewTimeSheetSummary.Starting_date = item.Starting_date;
                        NewTimeSheetSummary.Ending_date = item.Ending_date;
                        NewTimeSheetSummary.Ending_day_of_week = item.Ending_day_of_week;
                        NewTimeSheetSummary.Starting_day_of_week = item.Starting_day_of_week;

                        db.Timesheet_summaries.Add(NewTimeSheetSummary);
                        db.SaveChanges();
                    }
                }

                

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionHandlerController.infoMessage(ex.Message);
                ExceptionHandlerController.writeErrorLog(ex);
                return Json("failure", JsonRequestBehavior.AllowGet);
            }
        }

        //public void textExcel()
        //{


        //    string path = Server.MapPath("~/assets/csharp-Excel.xls");
        //    System.IO.FileInfo file = new System.IO.FileInfo(path);
        //    string Outgoingfile = "FileName.xlsx";
        //    if (file.Exists)
        //    {

        //        Response.Clear();
        //        Response.ClearContent();
        //        Response.ClearHeaders();
        //        Response.AddHeader("Content-Disposition", "attachment; filename=" + Outgoingfile);
        //        Response.AddHeader("Content-Length", file.Length.ToString());
        //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        Response.WriteFile(file.FullName);
        //        Response.Flush();
        //        Response.Close();

        //    }
        //    else
        //    {
        //        Response.Write("This file does not exist.");
        //    }

        //}




        public MemoryStream Download()
        {
            MemoryStream memStream;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("New Sheet");

                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[2, 1].Value = "1";
                worksheet.Cells[2, 2].Value = "One";
                worksheet.Cells[3, 1].Value = "2";
                worksheet.Cells[3, 2].Value = "Two";

                memStream = new MemoryStream(package.GetAsByteArray());
            }

            return memStream;
        }


        public FileStreamResult Download()
        {
            var memStream = BusinessLogic.Download();
            result File(memStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        #endregion
    }
}
