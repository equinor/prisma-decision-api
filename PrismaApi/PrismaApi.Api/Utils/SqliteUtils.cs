using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PrismaApi.Domain.Constants;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Utils;

public static class SqliteUtils
{
    public static string ComputeModelHash(AppDbContext db)
    {
        var modelText = db.Model.ToDebugString(MetadataDebugStringOptions.ShortDefault);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(modelText));
        return Convert.ToHexString(bytes);
    }

    public static string? ReadStoredModelHash()
    {
        var path = GetSchemaHashPath();
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    public static void WriteStoredModelHash(string hash)
    {
        File.WriteAllText(GetSchemaHashPath(), hash);
    }

    private static string GetSchemaHashPath()
        => Path.Combine(AppContext.BaseDirectory, AppConstants.SqliteHashStorageName);
}
