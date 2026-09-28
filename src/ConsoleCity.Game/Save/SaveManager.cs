using System.Text.Json;
using System.Text.Json.Serialization;
using ConsoleCity.Game;
using ConsoleCity.Core;
using System.IO;

namespace ConsoleCity.Game;

internal static class SaveManager
{
    private const int CurrentVersion = 2;

    private static JsonSerializerOptions Options => new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private sealed record SaveEnvelope(int Version, DateTime CreatedAt, ConsoleCity.Game.Save.Dto.SimulationSliceStateDto Payload);

        public static void Save(SimulationSliceState state, string name, string? baseDirectory = null)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Save name cannot be empty.", nameof(name));

            var dir = Path.Combine(baseDirectory ?? Environment.CurrentDirectory, "saves");
            Directory.CreateDirectory(dir);

            var payload = ConsoleCity.Game.Save.SaveMapper.ToDto(state);
            var envelope = new SaveEnvelope(CurrentVersion, DateTime.UtcNow, payload);
            var finalPath = Path.Combine(dir, name + ".json");
            var tempPath = finalPath + ".tmp";

            var json = JsonSerializer.Serialize(envelope, Options);
            File.WriteAllText(tempPath, json);
            // Ensure data flushed before replacing
            if (File.Exists(finalPath))
            {
                File.Replace(tempPath, finalPath, null);
            }
            else
            {
                File.Move(tempPath, finalPath);
            }
        }

        public static SimulationSliceState Load(string name, string? baseDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Save name cannot be empty.", nameof(name));

            var dir = Path.Combine(baseDirectory ?? Environment.CurrentDirectory, "saves");
            var finalPath = Path.Combine(dir, name + ".json");
            if (!File.Exists(finalPath)) throw new FileNotFoundException("Save not found.", finalPath);

            var json = File.ReadAllText(finalPath);
            var envelope = JsonSerializer.Deserialize<SaveEnvelope>(json, Options) ?? throw new InvalidOperationException("Invalid save file.");

            if (envelope.Version is not 1 and not 2)
            {
                throw new InvalidOperationException($"Unsupported save version: {envelope.Version}");
            }

            var state = ConsoleCity.Game.Save.SaveMapper.FromDto(envelope.Payload);
            return state;
        }

    public static IReadOnlyList<string> ListSaves(string? baseDirectory = null)
    {
        var dir = Path.Combine(baseDirectory ?? Environment.CurrentDirectory, "saves");
        if (!Directory.Exists(dir)) return Array.Empty<string>();

        var files = Directory.GetFiles(dir, "*.json");
        return files.Select(Path.GetFileNameWithoutExtension).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
