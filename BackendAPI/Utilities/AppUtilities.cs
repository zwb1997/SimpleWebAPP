namespace BackendAPI.Controller;

using Ardalis.Result;
using BackendAPI.Models;
using BackendAPI.Models.Common;
using Microsoft.AspNetCore.Mvc;

public static class AppUtilities
{
    public static IActionResult BuildResponse<T>(this ControllerBase controller, Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Ok => controller.Ok(new ApiResponse<T> { Data = result.Value, IsSuccess = true }),
            ResultStatus.NotFound => controller.NotFound(new ApiResponse<T> { Errors = result.Errors, IsSuccess = false }),
            ResultStatus.Invalid => controller.BadRequest(new ApiResponse<T> { Errors = result.Errors, IsSuccess = false }),
            _ => controller.StatusCode(500, new ApiResponse<T> { Errors = result.Errors, IsSuccess = false }),
        };
    }

    public static string EnsureRemoveNewLineWhenLog(this ControllerBase controller, string processStr)
    {
        return processStr.Replace(Environment.NewLine, string.Empty);
    }

    public static string EnsureRemoveNewLineWhenLog(string processStr)
    {
        return processStr.Replace(Environment.NewLine, string.Empty);
    }

}