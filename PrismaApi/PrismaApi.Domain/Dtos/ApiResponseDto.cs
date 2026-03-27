using System.Net;

namespace PrismaApi.Domain.Dtos;

public class ApiResponseDto
{
    public string? Content { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}
