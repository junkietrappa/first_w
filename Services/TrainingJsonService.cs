using System.Text.Json;
using first_w.Models;

namespace first_w.Services;

public sealed class TrainingJsonService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<TrainingLog> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var document = JsonSerializer.Deserialize<TrainingDocument>(json, SerializerOptions);
        var training = document?.DeviceLog;

        if (training?.Header is null)
        {
            throw new InvalidDataException("В файле отсутствует обязательный раздел DeviceLog.Header.");
        }

        return training;
    }
}
