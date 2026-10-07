using System.Collections.Generic;

namespace Munchify.DAL.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Optional HTTP status hint set by the Service layer (e.g. 404).
    /// Controllers can use this instead of parsing the Message string.
    /// Ignored during JSON serialization when null.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public int? StatusCode { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Thành công")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string> { message }
        };
    }

    public static ApiResponse<T> NotFoundResult(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 404,
            Errors = new List<string> { message }
        };
    }
}
