using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ActivityJournal.Models
{
    public partial class ActivityLog
    {
        public int LogId { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Ended { get; set; }
        public int ActivityType { get; set; }
        public int User { get; set; }
        public int? Difficulty { get; set; }
        public int? Feeling { get; set; }

        public virtual ActivityActivity ActivityTypeNavigation { get; set; } = null!;
       
        public virtual ActivityUser UserNavigation { get; set; } = null!;


    }

    public class LogModel
    {
        public int? LogId { get; set; }
        public string ActivityType { get; set; }
        public string Created { get; set; }
        public string? Ended { get; set; }
        public int? Difficulty { get; set; }
        public int? Feeling { get; set; }


    }

    public class UpdateLogModel
    {
        public int LogId { get; set;}
        public string ActivityType { get; set; }
        public string? Ended { get; set; }
        public int? Difficulty { get; set; }
        public int? Feeling { get; set; }

    }
}
