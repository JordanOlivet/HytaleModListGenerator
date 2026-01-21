using System.Net.Http.Json;
using System.Text.Json;
using HytaleModLister.Api.Models;

namespace HytaleModLister.Api.Services;

public class CurseForgeService : ICurseForgeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CurseForgeService> _logger;
    private readonly IConfiguration _configuration;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private const string ApiBaseUrl = "https://api.curseforge.com/v1";

    public CurseForgeService(HttpClient httpClient, ILogger<CurseForgeService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;

        var apiKey = _configuration["CurseForge:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _logger.LogInformation("CurseForge API key configured");
        }
    }

    private int GameId => _configuration.GetValue("CurseForge:GameId", 70216);

    public async Task<List<CfMod>> SearchModsAsync(string searchTerm)
    {
        try
        {
            var url = $"{ApiBaseUrl}/mods/search?gameId={GameId}&searchFilter={Uri.EscapeDataString(searchTerm)}&pageSize=50";
            var response = await _httpClient.GetFromJsonAsync<CfResponse>(url, JsonOpts);
            return ParseCfMods(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error searching mods for term: {SearchTerm}", searchTerm);
            return [];
        }
    }

    public async Task<List<CfMod>> GetModsBatchAsync(int offset, int pageSize = 50)
    {
        try
        {
            var url = $"{ApiBaseUrl}/mods/search?gameId={GameId}&pageSize={pageSize}&index={offset}";
            var response = await _httpClient.GetFromJsonAsync<CfResponse>(url, JsonOpts);
            return ParseCfMods(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching mods batch at offset {Offset}", offset);
            return [];
        }
    }

    private List<CfMod> ParseCfMods(CfResponse? response)
    {
        if (response?.Data == null) return [];

        return response.Data
            .Where(m => m.Links?.WebsiteUrl?.Contains("/mods/") == true)
            .Select(m => new CfMod
            {
                Name = m.Name ?? "",
                Slug = m.Slug ?? "",
                Url = m.Links!.WebsiteUrl!,
                Authors = m.Authors?.Select(a => a.Name ?? "").ToList() ?? [],
                LatestVersion = ExtractLatestVersion(m.LatestFiles)
            }).ToList();
    }

    private string? ExtractLatestVersion(List<CfFile>? files)
    {
        if (files == null || files.Count == 0) return null;

        // Get the most recent file by date, fallback to first file
        var latestFile = files
            .OrderByDescending(f => f.FileDate ?? DateTime.MinValue)
            .FirstOrDefault();

        if (latestFile == null) return null;

        // Try to extract version from DisplayName (e.g., "ModName v1.2.3" or "ModName 1.2.3")
        var displayName = latestFile.DisplayName;
        if (!string.IsNullOrEmpty(displayName))
        {
            // Common patterns: "v1.2.3", "1.2.3", "version 1.2.3"
            var versionMatch = System.Text.RegularExpressions.Regex.Match(
                displayName,
                @"v?(\d+(?:\.\d+)+(?:[-+][\w.]+)?)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (versionMatch.Success)
            {
                return versionMatch.Groups[1].Value;
            }
        }

        // Fallback: try to extract from FileName
        var fileName = latestFile.FileName;
        if (!string.IsNullOrEmpty(fileName))
        {
            var versionMatch = System.Text.RegularExpressions.Regex.Match(
                fileName,
                @"v?(\d+(?:\.\d+)+(?:[-+][\w.]+)?)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (versionMatch.Success)
            {
                return versionMatch.Groups[1].Value;
            }
        }

        return null;
    }

    public async Task<CfModData?> GetModBySlugAsync(string slug)
    {
        try
        {
            // Search for the mod by slug
            var url = $"{ApiBaseUrl}/mods/search?gameId={GameId}&slug={Uri.EscapeDataString(slug)}&pageSize=1";
            var response = await _httpClient.GetFromJsonAsync<CfResponse>(url, JsonOpts);

            if (response?.Data == null || response.Data.Count == 0)
            {
                _logger.LogWarning("No mod found with slug: {Slug}", slug);
                return null;
            }

            return response.Data[0];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting mod by slug: {Slug}", slug);
            return null;
        }
    }

    public async Task<string?> GetFileDownloadUrlAsync(int modId, int fileId)
    {
        try
        {
            var url = $"{ApiBaseUrl}/mods/{modId}/files/{fileId}/download-url";
            var response = await _httpClient.GetFromJsonAsync<CfDownloadUrlResponse>(url, JsonOpts);
            return response?.Data;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("Download forbidden for mod {ModId} file {FileId} - distribution disabled", modId, fileId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting download URL for mod {ModId} file {FileId}", modId, fileId);
            return null;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string downloadUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error downloading file from {Url}", downloadUrl);
            return null;
        }
    }
}
