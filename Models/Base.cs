

using FluentValidation.Results;
using Microsoft.Data.SqlClient;

namespace App.Models;


public class Response<T>
{
    public required string Message { get; set; }
    public required string Log { get; set; }
    public T? Data { get; set; }
}

public class ResponseMeta
{
    public required int TotalRecord { get; set; }
    public required int PageSize { get; set; }
    public required int Page { get; set; }
}

public class ResponseError
{
    public required ResponseErrorData Error { get; set; }
}

public class ResponseErrorData
{
    public required string Code { get; set; }
    public required string Message { get; set; }
    public required string Log { get; set; }
    public required dynamic Details { get; set; }

}

public class NotFoundException : Exception
{
    public string Code { get; }

    public NotFoundException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public class UnhandledException : Exception
{
    public string Code { get; }

    public UnhandledException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public class BadRequestException : Exception
{
    public BadRequestException(List<FluentValidation.Results.ValidationFailure> errors) : base("Bad request")
    {
        this.Errors = errors;
    }

    public List<ValidationFailure> Errors { get; }
}

public class ConflictException : Exception
{
    public ConflictException(string code, string message) : base(message)
    {
        Code = code;
    }
    public string Code { get; }

}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class ExceptionHelper
{
    public static bool IsDBInsertConflictException(Exception ex)
    {
        if (ex is SqlException sqlEx)
        {
            // Error code 2627 is for unique constraint violations in SQL Server
            // Error code 2601 is for duplicate key violations
            return sqlEx.Number == 2627 || sqlEx.Number == 2601;
        }

        return false;
    }
}




