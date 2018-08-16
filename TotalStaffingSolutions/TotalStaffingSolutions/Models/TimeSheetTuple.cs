using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TotalStaffingSolutions.Models
{
    public class TimeSheetSummaryTuple
    {
        public Timesheet_summaries TimeSheetSummary { get; set; }

        public string PO_Number { get; set; }
    }

    public class TimesheetObjectTuple
    {
        public Timesheet TimeSheetGeneralDetails { get; set; }
        public List<Timesheet_summaries> TimeSheetSummary { get; set; }

    }
}