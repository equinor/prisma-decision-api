using System;

namespace PrismaApi.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
