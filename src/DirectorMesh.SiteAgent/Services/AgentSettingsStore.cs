using System.Text.Json;

namespace DirectorMesh.SiteAgent.Services;

// Holds the live Director-MESH connection settings. Values start from configuration
// (appsettings / env vars / the saved settings file) and can be replaced at runtime
// from the Settings screen, which persists them to FilePath so they survive restarts.
public class AgentSettingsStore(DirectorMeshOptions initial, string filePath)
{
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private volatile DirectorMeshOptions _current = initial;

    public DirectorMeshOptions Current => _current;
    public string FilePath => filePath;

    public async Task SaveAsync(DirectorMeshOptions options, CancellationToken ct = default)
    {
        await _saveLock.WaitAsync(ct);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            var tmp = filePath + ".tmp";
            await File.WriteAllTextAsync(tmp,
                JsonSerializer.Serialize(new { DirectorMesh = new { options.BaseUrl, options.SiteId, options.ApiKey } }, WriteOptions),
                ct);
            File.Move(tmp, filePath, overwrite: true);
            _current = options;
        }
        finally
        {
            _saveLock.Release();
        }
    }
}
