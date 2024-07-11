namespace BackendAPI.Models.DTO;

public class PagedResultDTO<T>
{
    public PagedInfoDTO PagedInfo { get; set; }
    public List<T> Data { get; set; }
}
