using HytaleModLister.Api.Models;

namespace HytaleModLister.Api.Services;

public interface ICurseForgeService
{
    Task<List<CfMod>> SearchModsAsync(string searchTerm);
    Task<List<CfMod>> GetModsBatchAsync(int offset, int pageSize = 50);
    Task<CfModData?> GetModBySlugAsync(string slug);
    Task<string?> GetFileDownloadUrlAsync(int modId, int fileId);
    Task<Stream?> DownloadFileAsync(string downloadUrl);
}
