using System;

namespace PrismaApi.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
