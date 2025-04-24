public class ApiResidenceResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Id { get; set; }

    public ApiResidenceResponse(bool success, string message, T? id = default)
    {
        Success = success;
        Message = message;
        Id = id;
    }
}
