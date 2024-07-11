namespace DBTest.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class FollowedCaseQuery
    {
        public string CaseStatus { get; set; }
        public bool? IsArchive { get; set; }
        public DateTime? FollowedTimeStart { get; set; }
        public DateTime? FollowedTimeEnd { get; set; }
        public string CaseSev { get; set; }
        public int PageNumber { get; set; } = 1;
        public int RowsPerPage { get; set; } = 10;
    }
}
