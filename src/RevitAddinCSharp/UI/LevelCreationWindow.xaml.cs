using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RevitAddinCSharp.Settings;

namespace RevitAddinCSharp.UI
{
    public partial class LevelCreationWindow : Window
    {
        private readonly LevelCreationSettings _settings;
        private readonly ObservableCollection<PlannedLevelItem> _plannedLevels = new ObservableCollection<PlannedLevelItem>();

        public LevelCreationWindow(LevelCreationSettings settings = null)
        {
            InitializeComponent();
            _settings = settings ?? LevelCreationSettings.Current;
            DataContext = this;
            InitializePlan();
        }

        public ObservableCollection<PlannedLevelItem> PlannedLevels => _plannedLevels;

        public IList<PlannedLevelItem> ResultLevels { get; private set; }

        private void InitializePlan()
        {
            _plannedLevels.Clear();

            if (_settings == null)
            {
                return;
            }

            int digits = NormalizeDigits(_settings.LevelNumberDigits);
            string suffix = _settings.LevelNameSuffix ?? string.Empty;

            string groundToken = NonEmpty(_settings.GroundFloorToken, "EG");
            string upperToken = NonEmpty(_settings.UpperFloorToken, "OG");
            string topToken = NonEmpty(_settings.TopFloorToken, "DG");
            string basementToken = NonEmpty(_settings.BasementFloorToken, "UG");

            AddPlannedLevel(LevelCategory.Building, FormatPositiveName(0, digits, groundToken, suffix), 0.0, groundToken, _settings.AlwaysMarkAsBuildingStory);

            int aboveCount = Math.Max(0, _settings.DefaultAboveCount);
            double aboveHeight = _settings.DefaultAboveHeightMm > 0 ? _settings.DefaultAboveHeightMm : 3000.0;

            for (int index = 1; index <= aboveCount; index++)
            {
                bool isTop = aboveCount > 0 && index == aboveCount;
                string token = isTop ? topToken : upperToken;
                double elevation = index * aboveHeight;
                string name = FormatPositiveName(index, digits, token, suffix);
                AddPlannedLevel(LevelCategory.Building, name, elevation, token, _settings.AlwaysMarkAsBuildingStory);
            }

            int belowCount = Math.Max(0, _settings.DefaultBelowCount);
            double belowHeight = _settings.DefaultBelowHeightMm > 0 ? _settings.DefaultBelowHeightMm : 3000.0;

            for (int index = 1; index <= belowCount; index++)
            {
                double elevation = -index * belowHeight;
                string name = FormatNegativeName(index, digits, basementToken, suffix);
                AddPlannedLevel(LevelCategory.Building, name, elevation, basementToken, _settings.AlwaysMarkAsBuildingStory);
            }

            RenumberOrders();
        }

        private void AddPlannedLevel(LevelCategory category, string name, double elevationMm, string shortLabel, bool isBuildingStory)
        {
            PlannedLevelItem item = new PlannedLevelItem
            {
                Category = category,
                Name = name ?? string.Empty,
                ElevationMm = elevationMm,
                ShortLabel = shortLabel ?? string.Empty,
                IsBuildingStory = isBuildingStory
            };

            _plannedLevels.Add(item);
            RenumberOrders();
        }

        private void RenumberOrders()
        {
            for (int index = 0; index < _plannedLevels.Count; index++)
            {
                _plannedLevels[index].Order = index + 1;
            }
        }

        private void OnAddBuildingLevelClick(object sender, RoutedEventArgs e)
        {
            int digits = NormalizeDigits(_settings != null ? _settings.LevelNumberDigits : 2);
            string suffix = _settings != null ? _settings.LevelNameSuffix ?? string.Empty : string.Empty;
            string upperToken = NonEmpty(_settings != null ? _settings.UpperFloorToken : null, "OG");

            double step = _settings != null && _settings.DefaultAboveHeightMm > 0 ? _settings.DefaultAboveHeightMm : 3000.0;
            IEnumerable<PlannedLevelItem> positiveLevels = _plannedLevels.Where(level => level.Category == LevelCategory.Building && level.ElevationMm >= 0.0);
            double elevation = 0.0;
            int index = 0;

            if (positiveLevels.Any())
            {
                PlannedLevelItem topLevel = positiveLevels.OrderBy(level => level.ElevationMm).Last();
                elevation = topLevel.ElevationMm + step;
                index = positiveLevels.Count();
            }

            string name = FormatPositiveName(index, digits, upperToken, suffix);
            AddPlannedLevel(LevelCategory.Building, name, elevation, upperToken, _settings != null && _settings.AlwaysMarkAsBuildingStory);
        }

        private void OnAddTopEdgeClick(object sender, RoutedEventArgs e)
        {
            string token = NonEmpty(_settings != null ? _settings.TopFloorToken : null, "DG");
            double elevation = GetSuggestedElevation(LevelCategory.TopEdge);
            AddPlannedLevel(LevelCategory.TopEdge, token, elevation, token, false);
        }

        private void OnAddBottomEdgeClick(object sender, RoutedEventArgs e)
        {
            string token = NonEmpty(_settings != null ? _settings.BasementFloorToken : null, "UG");
            double elevation = GetSuggestedElevation(LevelCategory.BottomEdge);
            AddPlannedLevel(LevelCategory.BottomEdge, token, elevation, token, false);
        }

        private void OnAddHelperLevelClick(object sender, RoutedEventArgs e)
        {
            AddPlannedLevel(LevelCategory.Helper, "Hilfsebene", 0.0, string.Empty, false);
        }

        private void OnResetPlanClick(object sender, RoutedEventArgs e)
        {
            InitializePlan();
        }

        private void OnMoveUpClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PlannedLevelItem item)
            {
                int currentIndex = _plannedLevels.IndexOf(item);
                if (currentIndex > 0)
                {
                    _plannedLevels.Move(currentIndex, currentIndex - 1);
                    RenumberOrders();
                }
            }
        }

        private void OnMoveDownClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PlannedLevelItem item)
            {
                int currentIndex = _plannedLevels.IndexOf(item);
                if (currentIndex >= 0 && currentIndex < _plannedLevels.Count - 1)
                {
                    _plannedLevels.Move(currentIndex, currentIndex + 1);
                    RenumberOrders();
                }
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PlannedLevelItem item)
            {
                _plannedLevels.Remove(item);
                RenumberOrders();
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            if (_plannedLevels.Count == 0)
            {
                ShowValidation("Bitte mindestens eine Ebene planen.");
                return;
            }

            foreach (PlannedLevelItem item in _plannedLevels)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    ShowValidation("Bitte für jede Ebene eine Bezeichnung angeben.");
                    return;
                }

                if (!double.TryParse(item.ElevationText, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    ShowValidation("Bitte nur numerische Höhenwerte (mm) verwenden.");
                    return;
                }
            }

            ResultLevels = _plannedLevels.Select(level => level.Clone()).ToList();

            DialogResult = true;
            Close();
        }

        private double GetSuggestedElevation(LevelCategory category)
        {
            if (category == LevelCategory.TopEdge)
            {
                PlannedLevelItem highest = _plannedLevels.OrderBy(level => level.ElevationMm).LastOrDefault();
                if (highest != null)
                {
                    return highest.ElevationMm;
                }
            }

            if (category == LevelCategory.BottomEdge)
            {
                PlannedLevelItem lowest = _plannedLevels.OrderBy(level => level.ElevationMm).FirstOrDefault();
                if (lowest != null)
                {
                    return lowest.ElevationMm;
                }
            }

            return 0.0;
        }

        private static int NormalizeDigits(int digits)
        {
            if (digits < 1)
            {
                return 1;
            }

            if (digits > 4)
            {
                return 4;
            }

            return digits;
        }

        private static string NonEmpty(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string FormatPositiveName(int index, int digits, string token, string suffix)
        {
            string number = index.ToString("D" + digits, CultureInfo.InvariantCulture);
            return string.Concat(number, "_", token, suffix);
        }

        private static string FormatNegativeName(int index, int digits, string token, string suffix)
        {
            string number = index.ToString("D" + digits, CultureInfo.InvariantCulture);
            return string.Concat("-", number, "_", token, suffix);
        }

        private void ShowValidation(string message)
        {
            MessageBox.Show(this, message, "Eingabe prüfen", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
