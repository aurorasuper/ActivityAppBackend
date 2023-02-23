using System;
using System.Collections.Generic;

namespace ActivityJournal.Models
{
    public partial class ActivityActivity
    {
        public ActivityActivity()
        {
            ActivityLogs = new HashSet<ActivityLog>();
        }

        public int Id { get; set; }
        public string ActivityType { get; set; } = null!;

        public virtual ICollection<ActivityLog> ActivityLogs { get; set; }
    }
}
