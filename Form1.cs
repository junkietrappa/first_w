using System.ComponentModel;
using System.Globalization;
using first_w.Models;
using first_w.Services;

namespace first_w
{
    public partial class Form1 : Form
    {
        private const double JoulesPerKilocalorie = 4184;
        private readonly TrainingJsonService _jsonService = new();
        private readonly TrainingAnalysisService _analysisService = new();
        private readonly Label _fileNameLabel = new();
        private readonly Label _informationLabel = new();
        private readonly Label _analysisLabel = new();
        private readonly Label _messageLabel = new();
        private readonly DataGridView _dataGridView = new();
        private readonly Button _openButton = new();

        public Form1()
        {
            InitializeComponent();
            Text = "Анализ тренировок Suunto";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 650);
            Size = new Size(1100, 750);
            CreateControls();
        }

        private void CreateControls()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(10)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 85));
            Controls.Add(layout);

            var title = new Label
            {
                Text = "Анализ спортивных тренировок Suunto",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(title, 0, 0);

            var filePanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            _openButton.Text = "Открыть JSON";
            _openButton.AutoSize = true;
            _openButton.Click += OpenButton_Click;
            _fileNameLabel.Text = "Файл не выбран";
            _fileNameLabel.AutoSize = true;
            _fileNameLabel.Padding = new Padding(10, 7, 0, 0);
            filePanel.Controls.Add(_openButton);
            filePanel.Controls.Add(_fileNameLabel);
            layout.Controls.Add(filePanel, 0, 1);

            var informationBox = new GroupBox
            {
                Text = "Общая информация",
                Dock = DockStyle.Fill
            };
            _informationLabel.Dock = DockStyle.Fill;
            _informationLabel.Padding = new Padding(8);
            _informationLabel.TextAlign = ContentAlignment.TopLeft;
            informationBox.Controls.Add(_informationLabel);
            layout.Controls.Add(informationBox, 0, 2);

            var tableBox = new GroupBox
            {
                Text = "Данные по интервалам",
                Dock = DockStyle.Fill
            };
            CreateGridColumns();
            _dataGridView.Dock = DockStyle.Fill;
            _dataGridView.ReadOnly = true;
            _dataGridView.AllowUserToAddRows = false;
            _dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dataGridView.RowHeadersVisible = false;
            tableBox.Controls.Add(_dataGridView);
            layout.Controls.Add(tableBox, 0, 3);

            var resultsBox = new GroupBox
            {
                Text = "Результаты анализа",
                Dock = DockStyle.Fill
            };
            var resultsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            resultsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            resultsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            _analysisLabel.Dock = DockStyle.Fill;
            _messageLabel.Dock = DockStyle.Fill;
            resultsLayout.Controls.Add(_analysisLabel, 0, 0);
            resultsLayout.Controls.Add(_messageLabel, 0, 1);
            resultsBox.Controls.Add(resultsLayout);
            layout.Controls.Add(resultsBox, 0, 4);

            ShowMessage("Выберите файл тренировки.");
        }

        private void CreateGridColumns()
        {
            AddColumn("№", nameof(IntervalAnalysisRow.Number));
            AddColumn("Тип", nameof(IntervalAnalysisRow.Type));
            AddColumn("Время", nameof(IntervalAnalysisRow.Time));
            AddColumn("Длительность (мин)", nameof(IntervalAnalysisRow.DurationMinutes));
            AddColumn("Дистанция (км)", nameof(IntervalAnalysisRow.DistanceKilometers));
            AddColumn("Ср. скорость (км/ч)", nameof(IntervalAnalysisRow.AverageSpeedKilometersPerHour));
            AddColumn("Макс. скорость (км/ч)", nameof(IntervalAnalysisRow.MaximumSpeedKilometersPerHour));
            AddColumn("Ср. пульс (уд/мин)", nameof(IntervalAnalysisRow.AverageHeartRate));
            AddColumn("Макс. пульс (уд/мин)", nameof(IntervalAnalysisRow.MaximumHeartRate));
            AddColumn("Каденс (шагов/мин)", nameof(IntervalAnalysisRow.AverageCadence));
            AddColumn("Набор высоты (м)", nameof(IntervalAnalysisRow.AscentMeters));
        }

        private void AddColumn(string heading, string propertyName)
        {
            _dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = heading,
                DataPropertyName = propertyName
            });
        }

        private async void OpenButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "JSON-файлы (*.json)|*.json",
                Title = "Открыть тренировку"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _fileNameLabel.Text = Path.GetFileName(dialog.FileName);
            _dataGridView.DataSource = null;
            _informationLabel.Text = "Нет данных";
            _analysisLabel.Text = "";
            _messageLabel.Text = "Загрузка файла...";
            _openButton.Enabled = false;

            try
            {
                var training = await _jsonService.LoadAsync(dialog.FileName);
                var analysis = _analysisService.Analyze(training);
                ShowTraining(training, analysis);
            }
            catch (System.Text.Json.JsonException)
            {
                ShowMessage("Ошибка: файл содержит некорректный JSON.");
            }
            catch (InvalidDataException exception)
            {
                ShowMessage($"Ошибка структуры файла: {exception.Message}");
            }
            catch (IOException exception)
            {
                ShowMessage($"Не удалось прочитать файл: {exception.Message}");
            }
            catch (UnauthorizedAccessException exception)
            {
                ShowMessage($"Нет доступа к файлу: {exception.Message}");
            }
            finally
            {
                _openButton.Enabled = true;
            }
        }

        private void ShowTraining(TrainingLog training, TrainingAnalysisResult analysis)
        {
            var header = training.Header!;
            var culture = CultureInfo.GetCultureInfo("ru-RU");
            var activity = header.ActivityType switch
            {
                3 => "Бег",
                12 => "Ходьба",
                _ when !string.IsNullOrWhiteSpace(header.Activity) => header.Activity,
                int type => $"Спортивный режим (тип {type})",
                _ => "Нет данных"
            };

            _informationLabel.Text =
                $"Вид активности: {activity}\n" +
                $"Дата: {header.DateTime?.ToString("dd.MM.yyyy HH:mm", culture) ?? "Нет данных"}\n" +
                $"Продолжительность: {FormatDuration(header.DurationSeconds)}\n" +
                $"Дистанция: {Format(header.DistanceMeters / 1000, "0.000", " км")}\n" +
                $"Расход энергии: {Format(header.EnergyJoules / JoulesPerKilocalorie, "0", " ккал")}\n" +
                $"Шаги: {header.StepCount?.ToString("N0", culture) ?? "Нет данных"}\n" +
                $"Набор высоты: {Format(header.AscentMeters, "0.0", " м")}";

            _dataGridView.DataSource = new BindingList<IntervalAnalysisRow>(analysis.Rows.ToList());
            _analysisLabel.Text = string.Join(Environment.NewLine, analysis.SummaryLines);
            _messageLabel.Text = analysis.Messages.Count > 0
                ? string.Join(" ", analysis.Messages)
                : "Анализ завершён.";
        }

        private void ShowMessage(string message)
        {
            _informationLabel.Text = "Нет данных";
            _analysisLabel.Text = "";
            _messageLabel.Text = message;
        }

        private static string FormatDuration(double? seconds)
        {
            if (!seconds.HasValue || !double.IsFinite(seconds.Value) || seconds.Value < 0)
            {
                return "Нет данных";
            }

            var duration = TimeSpan.FromSeconds(seconds.Value);
            return duration.TotalHours >= 1
                ? $"{(int)duration.TotalHours} ч {duration.Minutes} мин"
                : $"{duration.Minutes} мин {duration.Seconds} с";
        }

        private static string Format(double? value, string format, string suffix)
        {
            return value.HasValue
                ? value.Value.ToString(format, CultureInfo.GetCultureInfo("ru-RU")) + suffix
                : "Нет данных";
        }
    }
}
