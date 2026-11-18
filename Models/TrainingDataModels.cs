using System.Text.Json;
using System.Text.Json.Serialization;

namespace first_w.Models;

public sealed class TrainingDocument
{
    [JsonPropertyName("DeviceLog")]
    public TrainingLog? DeviceLog { get; set; }
}

public sealed class TrainingLog
{
    [JsonPropertyName("Header")]
    public TrainingHeader? Header { get; set; }

    [JsonPropertyName("Samples")]
    public List<TrainingSample?>? Samples { get; set; }

    [JsonPropertyName("Windows")]
    public List<TrainingWindowEntry?>? Windows { get; set; }

    [JsonPropertyName("Device")]
    public TrainingDevice? Device { get; set; }
}

public sealed class TrainingHeader
{
    [JsonPropertyName("Activity")]
    public string? Activity { get; set; }

    [JsonPropertyName("ActivityType")]
    public int? ActivityType { get; set; }

    [JsonPropertyName("DateTime")]
    public DateTimeOffset? DateTime { get; set; }

    [JsonPropertyName("Duration")]
    public double? DurationSeconds { get; set; }

    [JsonPropertyName("Distance")]
    public double? DistanceMeters { get; set; }

    [JsonPropertyName("Energy")]
    public double? EnergyJoules { get; set; }

    [JsonPropertyName("StepCount")]
    public int? StepCount { get; set; }

    [JsonPropertyName("Ascent")]
    public double? AscentMeters { get; set; }

    [JsonPropertyName("Descent")]
    public double? DescentMeters { get; set; }

    [JsonPropertyName("EPOC")]
    public double? Epoc { get; set; }

    [JsonPropertyName("MAXVO2")]
    public double? MaxVo2 { get; set; }

    [JsonPropertyName("PeakTrainingEffect")]
    public double? PeakTrainingEffect { get; set; }

    [JsonPropertyName("RecoveryTime")]
    public double? RecoveryTimeSeconds { get; set; }

    [JsonPropertyName("FitnessAge")]
    public int? FitnessAge { get; set; }

    [JsonPropertyName("FitnessAgeClassification")]
    public string? FitnessAgeClassification { get; set; }

    [JsonPropertyName("TraingingLoadPeak")]
    public double? TrainingLoadPeak { get; set; }

    [JsonPropertyName("VerticalSpeed")]
    public double? VerticalSpeedMetersPerSecond { get; set; }

    [JsonPropertyName("Altitude")]
    public HeaderAltitude? Altitude { get; set; }
}

public sealed class HeaderAltitude
{
    [JsonPropertyName("Max")]
    public double? MaximumMeters { get; set; }

    [JsonPropertyName("Min")]
    public double? MinimumMeters { get; set; }
}

public sealed class TrainingSample
{
    [JsonPropertyName("TimeISO8601")]
    public DateTimeOffset? Time { get; set; }

    [JsonPropertyName("Events")]
    public List<JsonElement>? Events { get; set; }
}

public sealed class TrainingWindowEntry
{
    [JsonPropertyName("TimeISO8601")]
    public DateTimeOffset? Time { get; set; }

    [JsonPropertyName("Window")]
    public TrainingWindow? Window { get; set; }
}

public sealed class TrainingWindow
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Duration")]
    public double? DurationSeconds { get; set; }

    [JsonPropertyName("Distance")]
    public double? DistanceMeters { get; set; }

    [JsonPropertyName("Energy")]
    public double? EnergyJoules { get; set; }

    [JsonPropertyName("Ascent")]
    public double? AscentMeters { get; set; }

    [JsonPropertyName("Descent")]
    public double? DescentMeters { get; set; }

    [JsonPropertyName("Speed")]
    public List<MetricStatistics?>? Speed { get; set; }

    [JsonPropertyName("HR")]
    public List<MetricStatistics?>? HeartRate { get; set; }

    [JsonPropertyName("Cadence")]
    public List<MetricStatistics?>? Cadence { get; set; }

    [JsonPropertyName("Altitude")]
    public List<MetricStatistics?>? Altitude { get; set; }

    [JsonPropertyName("VerticalSpeed")]
    public List<MetricStatistics?>? VerticalSpeed { get; set; }

    [JsonPropertyName("Temperature")]
    public List<MetricStatistics?>? Temperature { get; set; }
}

public sealed class MetricStatistics
{
    [JsonPropertyName("Avg")]
    public double? Average { get; set; }

    [JsonPropertyName("Max")]
    public double? Maximum { get; set; }

    [JsonPropertyName("Min")]
    public double? Minimum { get; set; }
}

public sealed class TrainingDevice
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("SerialNumber")]
    public string? SerialNumber { get; set; }
}
