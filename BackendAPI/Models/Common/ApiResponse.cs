namespace BackendAPI.Models.Common;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    
    public T Data { get; set; }
    
    public IEnumerable<string> Errors { get; set; }

    public ApiResponse()
    {
        Errors = new List<string>();
    }
}