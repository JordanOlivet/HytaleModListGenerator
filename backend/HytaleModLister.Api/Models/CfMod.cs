namespace HytaleModLister.Api.Models;

public class CfMod
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Url { get; set; } = "";
    public List<string> Authors { get; set; } = [];
    public string? LatestVersion { get; set; }
}

public record MatchResult(string Url, string MatchType, string? LatestVersion);

// CurseForge API response models
public class CfResponse
{
    public List<CfModData>? Data { get; set; }
}

public class CfModData
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public CfLinks? Links { get; set; }
    public List<CfAuthor>? Authors { get; set; }
    public List<CfFile>? LatestFiles { get; set; }
}

public class CfFile
{
    public int Id { get; set; }
    public string? DisplayName { get; set; }
    public string? FileName { get; set; }
    public DateTime? FileDate { get; set; }
    public string? DownloadUrl { get; set; }
    public long FileLength { get; set; }
    public List<CfHash>? Hashes { get; set; }
}

public class CfHash
{
    public string Value { get; set; } = "";
    public int Algo { get; set; } // 1 = SHA1, 2 = MD5
}

public class CfLinks
{
    public string? WebsiteUrl { get; set; }
}

public class CfAuthor
{
    public string? Name { get; set; }
}

public record UpdateModResponse(bool Success, string Message, string? NewFileName = null, string? OldFileName = null);

// Single mod response wrapper
public class CfSingleModResponse
{
    public CfModData? Data { get; set; }
}

// Download URL response wrapper
public class CfDownloadUrlResponse
{
    public string? Data { get; set; }
}
