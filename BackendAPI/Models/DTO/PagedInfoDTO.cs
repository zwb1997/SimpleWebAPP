namespace BackendAPI.Models.DTO;

public class PagedInfoDTO
{
    public long PageNumber { get; set; }
    public long PageSize { get; set; }
    public long TotalPages { get; set; }
    public long TotalRecords { get; set; }
}
