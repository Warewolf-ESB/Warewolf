using System;
using System.Net;

public class HttpException : Exception
{
    readonly int httpStatusCode;
    new string Message;

    public HttpException(string message)
    {
        Message = message;
    }

    public HttpException(int httpStatusCode)
    {
        this.httpStatusCode = httpStatusCode;
    }

    public HttpException(HttpStatusCode httpStatusCode)
    {
        this.httpStatusCode = (int)httpStatusCode;
    }

    public HttpException(int httpStatusCode, string message) : base(message)
    {
        this.httpStatusCode = httpStatusCode;
    }

    public HttpException(HttpStatusCode httpStatusCode, string message) : base(message)
    {
        this.httpStatusCode = (int)httpStatusCode;
    }

    public HttpException(int httpStatusCode, string message, Exception inner) : base(message, inner)
    {
        this.httpStatusCode = httpStatusCode;
    }

    public HttpException(HttpStatusCode httpStatusCode, string message, Exception inner) : base(message, inner)
    {
        this.httpStatusCode = (int)httpStatusCode;
    }

    public int StatusCode { get { return this.httpStatusCode; } }
}