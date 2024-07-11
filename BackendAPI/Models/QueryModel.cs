namespace BackendAPI.Models;

using BackendAPI.Models.Tables;
using System;

public class QueryModel
{
    public string CaseId { get; set; }
    public string CaseStatus { get; set; }
    public bool? IsArchive { get; set; }
    public DateTime? FollowedTimeStart { get; set; }
    public DateTime? FollowedTimeEnd { get; set; }
    public string CaseSev { get; set; }
    public string WhoFollowed { get; set; }
    public int PageNumber { get; set; } = 1;
    public int RowsPerPage { get; set; } = 10;
    public CaseFollowModel? CaseFollowModel { get; set; }
    public CaseModel? CaseModel { get; set; }
}
