namespace BackendAPI.Models;

public class CaseQueryModel
{
    public int PageNumber { get; set; } = 1;
    public int RowsPerPage { get; set; } = 10;
    public string? CaseStatus { get; set; } = "";
    public string? CaseID { get; set; } = "";
    public string? CaseSubject { get; set; } = "";
    public string? CaseSev { get; set; } = "";
    
    public string? WhoFollowed { get; set; } = "JingPing Gao";
}
