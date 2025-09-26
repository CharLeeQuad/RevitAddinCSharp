using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using RevitAddinCSharp.Settings;

namespace RevitAddinCSharp.UI
{
    public partial class LevelSettingsWindow : Window
    {
        private readonly LevelCreationSettings _draft;
        private readonly Document _document;
        private readonly List<LevelTypeOption> _levelTypes = new List<LevelTypeOption>();

        public LevelSettingsWindow(Document document = null)
        {
            InitializeComponent();
            _document = document;
            _draft = LevelCreationSettings.Current.Clone();
            LoadLevelTypes();
            LoadValues(_draft);
        }

        private void LoadValues(LevelCreationSettings settings)
        {
            BuildingStoryCheckBox.IsChecked = settings.AlwaysMarkAsBuildingStory;
            ComputationHeightInput.Text = settings.DefaultComputationHeightMm.ToString(CultureInfo.InvariantCulture);
            DefaultAboveCountInput.Text = settings.DefaultAboveCount.ToString(CultureInfo.InvariantCulture);
            DefaultAboveHeightInput.Text = settings.DefaultAboveHeightMm.ToString(CultureInfo.InvariantCulture);
            DefaultBelowCountInput.Text = settings.DefaultBelowCount.ToString(CultureInfo.InvariantCulture);
            DefaultBelowHeightInput.Text = settings.DefaultBelowHeightMm.ToString(CultureInfo.InvariantCulture);

            if (_levelTypes.Count > 0)
            {
                LevelTypeOption match = null;
                if (settings.PreferredLevelTypeId != 0)
                {
                    match = _levelTypes.Find(option => option.Id == settings.PreferredLevelTypeId);
                }

                if (match == null && !string.IsNullOrWhiteSpace(settings.PreferredLevelTypeName))
                {
                    match = _levelTypes.Find(option => string.Equals(option.Name, settings.PreferredLevelTypeName, StringComparison.OrdinalIgnoreCase));
                }

                LevelTypeCombo.SelectedItem = match;
                if (LevelTypeCombo.SelectedItem == null && _levelTypes.Count > 0)
                {
                    LevelTypeCombo.SelectedItem = _levelTypes[0];
                }
            }
            else
            {
                LevelTypeCombo.Text = settings.PreferredLevelTypeName ?? string.Empty;
            }
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseDouble(ComputationHeightInput.Text, 100, 10000, out double computationHeightMm))
            {
                ShowValidation("Bitte eine gültige Rechenhöhe in mm (100-10000) eingeben.");
                return;
            }

            if (!TryParseInt(DefaultAboveCountInput.Text, 0, 50, out int aboveCount))
            {
                ShowValidation("Bitte eine gültige Anzahl Obergeschosse (0-50) eingeben.");
                return;
            }

            if (!TryParseDouble(DefaultAboveHeightInput.Text, 500, 10000, out double aboveHeightMm))
            {
                ShowValidation("Bitte eine gültige Obergeschoss-Höhe in mm (500-10000) eingeben.");
                return;
            }

            if (!TryParseInt(DefaultBelowCountInput.Text, 0, 20, out int belowCount))
            {
                ShowValidation("Bitte eine gültige Anzahl Untergeschosse (0-20) eingeben.");
                return;
            }

            if (!TryParseDouble(DefaultBelowHeightInput.Text, 500, 10000, out double belowHeightMm))
            {
                ShowValidation("Bitte eine gültige Untergeschoss-Höhe in mm (500-10000) eingeben.");
                return;
            }

            _draft.AlwaysMarkAsBuildingStory = BuildingStoryCheckBox.IsChecked == true;
            _draft.DefaultComputationHeightMm = computationHeightMm;
            _draft.DefaultAboveCount = aboveCount;
            _draft.DefaultAboveHeightMm = aboveHeightMm;
            _draft.DefaultBelowCount = belowCount;
            _draft.DefaultBelowHeightMm = belowHeightMm;

            if (LevelTypeCombo.SelectedItem is LevelTypeOption selectedType)
            {
                _draft.PreferredLevelTypeName = selectedType.Name;
                _draft.PreferredLevelTypeId = selectedType.Id;
            }
            else
            {
                _draft.PreferredLevelTypeName = string.Empty;
                _draft.PreferredLevelTypeId = 0;
            }

            LevelCreationSettings.Current.Apply(_draft);

            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            _draft.ResetToDefaults();
            LoadValues(_draft);
        }

        private void LoadLevelTypes()
        {
            if (_document == null)
            {
                LevelTypeCombo.ItemsSource = null;
                LevelTypeCombo.IsEnabled = false;
                LevelTypeCombo.Text = _draft?.PreferredLevelTypeName ?? string.Empty;
                return;
            }

            foreach (LevelType type in new FilteredElementCollector(_document).OfClass(typeof(LevelType)))
            {
                _levelTypes.Add(new LevelTypeOption(type.Name, type.Id.IntegerValue));
            }

            _levelTypes.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

            LevelTypeCombo.ItemsSource = _levelTypes;
            LevelTypeCombo.IsEnabled = _levelTypes.Count > 0;
        }

        private static bool TryParseInt(string text, int min, int max, out int value)
        {
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                if (value >= min && value <= max)
                {
                    return true;
                }
            }

            value = 0;
            return false;
        }

        private static bool TryParseDouble(string text, double min, double max, out double value)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                if (value >= min && value <= max)
                {
                    return true;
                }
            }

            value = 0;
            return false;
        }

        private void ShowValidation(string message)
        {
            MessageBox.Show(this, message, "Eingabe prüfen", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
