using System.Globalization;
using first_w.Models;

namespace first_w.Services;

public sealed class TrainingAnalysisService
{
    private const double MetersPerSecondToKilometersPerHour = 3.6;
    private const double BeatsPerSecondToBeatsPerMinute = 60;
    private const double CadenceCyclesPerSecondToStepsPerMinute = 120;
    private static readonly CultureInfo RussianCulture = CultureInfo.GetCultureInfo("ru-RU");

    public TrainingAnalysisResult Analyze(TrainingLog training)
    {
        ArgumentNullException.ThrowIfNull(training);

        var allEntries = training.Windows?
            .Where(entry => entry?.Window is not null)
            .ToList() ?? [];
        var entries = allEntries
            .Where(entry => !IsSummaryWindow(entry!.Window!))
            .ToList();
        var showingActivitySummary = entries.Count == 0 && allEntries.Count > 0;
        if (showingActivitySummary)
        {
            var activitySummary = allEntries.FirstOrDefault(entry =>
                string.Equals(entry!.Window!.Type, "Activity", StringComparison.OrdinalIgnoreCase));
            entries.Add(activitySummary ?? allEntries[0]);
        }

        var rows = entries.Select((entry, index) => CreateRow(entry!, index + 1)).ToList();
        var speeds = entries.Select(entry => GetStatistic(entry!.Window!.Speed, statistic => statistic?.Average))
            .Where(value => value.HasValue)
            .Select(value => value!.Value * MetersPerSecondToKilometersPerHour)
            .ToList();
        var maximumSpeeds = entries.Select(entry => GetStatistic(entry!.Window!.Speed, statistic => statistic?.Maximum))
            .Where(value => value.HasValue)
            .Select(value => value!.Value * MetersPerSecondToKilometersPerHour)
            .ToList();
        var averageHeartRates = entries.Select(entry => GetStatistic(entry!.Window!.HeartRate, statistic => statistic?.Average))
            .Where(value => value.HasValue)
            .Select(value => value!.Value * BeatsPerSecondToBeatsPerMinute)
            .ToList();
        var maximumHeartRates = entries.Select(entry => GetStatistic(entry!.Window!.HeartRate, statistic => statistic?.Maximum))
            .Where(value => value.HasValue)
            .Select(value => value!.Value * BeatsPerSecondToBeatsPerMinute)
            .ToList();
        var averageCadences = entries.Select(entry => GetStatistic(entry!.Window!.Cadence, statistic => statistic?.Average))
            .Where(value => value.HasValue)
            .Select(value => value!.Value * CadenceCyclesPerSecondToStepsPerMinute)
            .ToList();
        var maximumCadences = entries.Select(entry => GetStatistic(entry!.Window!.Cadence, statistic => statistic?.Maximum))
            .Where(value => value.HasValue)
            .Select(value => value!.Value * CadenceCyclesPerSecondToStepsPerMinute)
            .ToList();

        var summaryLines = new List<string>
        {
            $"Записей в таблице: {rows.Count}."
        };
        var messages = new List<string>();

        if (showingActivitySummary)
        {
            messages.Add("Интервалы в файле отсутствуют; показано сводное окно тренировки.");
        }

        AddStatistics(summaryLines, "Скорость", speeds, maximumSpeeds, "км/ч");
        AddStatistics(summaryLines, "Пульс", averageHeartRates, maximumHeartRates, "уд/мин");
        AddStatistics(summaryLines, "Каденс", averageCadences, maximumCadences, "шагов/мин");

        if (averageCadences.Count == 0)
        {
            messages.Add("Для данной тренировки отсутствуют данные каденса.");
        }

        if (averageHeartRates.Count == 0)
        {
            messages.Add("Для данной тренировки отсутствуют данные пульса.");
        }

        if (rows.Count == 0)
        {
            messages.Add("В файле отсутствуют интервальные данные в разделе Windows.");
        }

        var maximumHeartRate = maximumHeartRates.Count > 0 ? maximumHeartRates.Max() : (double?)null;
        return new TrainingAnalysisResult(rows, summaryLines, messages, maximumHeartRate);
    }

    private static bool IsSummaryWindow(TrainingWindow window)
    {
        return string.Equals(window.Type, "Activity", StringComparison.OrdinalIgnoreCase)
            || string.Equals(window.Type, "Move", StringComparison.OrdinalIgnoreCase);
    }

    private static IntervalAnalysisRow CreateRow(TrainingWindowEntry entry, int number)
    {
        var window = entry.Window!;
        var speedAverage = GetStatistic(window.Speed, statistic => statistic?.Average);
        var speedMaximum = GetStatistic(window.Speed, statistic => statistic?.Maximum);
        var heartRateAverage = GetStatistic(window.HeartRate, statistic => statistic?.Average);
        var heartRateMaximum = GetStatistic(window.HeartRate, statistic => statistic?.Maximum);
        var cadenceAverage = GetStatistic(window.Cadence, statistic => statistic?.Average);

        return new IntervalAnalysisRow
        {
            Number = number.ToString(RussianCulture),
            Type = string.IsNullOrWhiteSpace(window.Type) ? "Нет данных" : window.Type,
            Time = entry.Time?.ToString("HH:mm:ss", RussianCulture) ?? "Нет данных",
            DurationMinutes = Format(window.DurationSeconds / 60, "0.00"),
            DistanceKilometers = Format(window.DistanceMeters / 1000, "0.000"),
            AverageSpeedKilometersPerHour = Format(speedAverage * MetersPerSecondToKilometersPerHour, "0.00"),
            MaximumSpeedKilometersPerHour = Format(speedMaximum * MetersPerSecondToKilometersPerHour, "0.00"),
            AverageHeartRate = Format(heartRateAverage * BeatsPerSecondToBeatsPerMinute, "0"),
            MaximumHeartRate = Format(heartRateMaximum * BeatsPerSecondToBeatsPerMinute, "0"),
            AverageCadence = Format(cadenceAverage * CadenceCyclesPerSecondToStepsPerMinute, "0"),
            AscentMeters = Format(window.AscentMeters, "0.0")
        };
    }

    private static double? GetStatistic(
        IEnumerable<MetricStatistics?>? statistics,
        Func<MetricStatistics?, double?> selector)
    {
        if (statistics is null)
        {
            return null;
        }

        foreach (var statistic in statistics)
        {
            var value = selector(statistic);
            if (value.HasValue && double.IsFinite(value.Value))
            {
                return value;
            }
        }

        return null;
    }

    private static void AddStatistics(
        ICollection<string> summary,
        string name,
        IReadOnlyCollection<double> averages,
        IReadOnlyCollection<double> maxima,
        string unit)
    {
        if (averages.Count == 0)
        {
            summary.Add($"{name}: нет данных.");
            return;
        }

        var max = maxima.Count > 0 ? maxima.Max() : averages.Max();
        summary.Add($"{name}: среднее по интервалам — {Number(averages.Average())} {unit}, максимум — {Number(max)} {unit}.");
    }

    private static string Format(double? value, string format)
    {
        return value.HasValue && double.IsFinite(value.Value)
            ? Number(value.Value, format)
            : "Нет данных";
    }

    private static string Number(double value, string format = "0.0")
    {
        return value.ToString(format, RussianCulture);
    }
}

public sealed class IntervalAnalysisRow
{
    public string Number { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Time { get; init; } = string.Empty;
    public string DurationMinutes { get; init; } = string.Empty;
    public string DistanceKilometers { get; init; } = string.Empty;
    public string AverageSpeedKilometersPerHour { get; init; } = string.Empty;
    public string MaximumSpeedKilometersPerHour { get; init; } = string.Empty;
    public string AverageHeartRate { get; init; } = string.Empty;
    public string MaximumHeartRate { get; init; } = string.Empty;
    public string AverageCadence { get; init; } = string.Empty;
    public string AscentMeters { get; init; } = string.Empty;
}

public sealed record TrainingAnalysisResult(
    IReadOnlyList<IntervalAnalysisRow> Rows,
    IReadOnlyList<string> SummaryLines,
    IReadOnlyList<string> Messages,
    double? MaximumHeartRate);
