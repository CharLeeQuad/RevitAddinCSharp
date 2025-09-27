using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using RevitAddinCSharp.Settings;
using RevitAddinCSharp.Utils;

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
            UpdateCategoryView("Levels");
        }

        private void LoadValues(LevelCreationSettings settings)
        {
            BuildingStoryCheckBox.IsChecked = settings.AlwaysMarkAsBuildingStory;
            ComputationHeightInput.Text = settings.DefaultComputationHeightMm.ToString(CultureInfo.InvariantCulture);
            DefaultAboveCountInput.Text = settings.DefaultAboveCount.ToString(CultureInfo.InvariantCulture);
            DefaultAboveHeightInput.Text = settings.DefaultAboveHeightMm.ToString(CultureInfo.InvariantCulture);
            DefaultBelowCountInput.Text = settings.DefaultBelowCount.ToString(CultureInfo.InvariantCulture);
            DefaultBelowHeightInput.Text = settings.DefaultBelowHeightMm.ToString(CultureInfo.InvariantCulture);
            RenameExistingCheckBox.IsChecked = settings.RenameExistingLevels;
            LevelDigitsInput.Text = settings.LevelNumberDigits.ToString(CultureInfo.InvariantCulture);
            LevelSuffixInput.Text = settings.LevelNameSuffix ?? string.Empty;
            GroundTokenInput.Text = settings.GroundFloorToken ?? string.Empty;
            UpperTokenInput.Text = settings.UpperFloorToken ?? string.Empty;
            TopTokenInput.Text = settings.TopFloorToken ?? string.Empty;
            BasementTokenInput.Text = settings.BasementFloorToken ?? string.Empty;

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

            if (!TryParseInt(LevelDigitsInput.Text, 1, 4, out int numberDigits))
            {
                ShowValidation("Bitte eine gültige Stellenanzahl für Ebenennummern (1-4) eingeben.");
                return;
            }

            _draft.AlwaysMarkAsBuildingStory = BuildingStoryCheckBox.IsChecked == true;
            _draft.DefaultComputationHeightMm = computationHeightMm;
            _draft.DefaultAboveCount = aboveCount;
            _draft.DefaultAboveHeightMm = aboveHeightMm;
            _draft.DefaultBelowCount = belowCount;
            _draft.DefaultBelowHeightMm = belowHeightMm;
            _draft.RenameExistingLevels = RenameExistingCheckBox.IsChecked == true;
            _draft.LevelNumberDigits = numberDigits;
            _draft.LevelNameSuffix = (LevelSuffixInput.Text ?? string.Empty).Trim();
            _draft.GroundFloorToken = (GroundTokenInput.Text ?? string.Empty).Trim();
            _draft.UpperFloorToken = (UpperTokenInput.Text ?? string.Empty).Trim();
            _draft.TopFloorToken = (TopTokenInput.Text ?? string.Empty).Trim();
            _draft.BasementFloorToken = (BasementTokenInput.Text ?? string.Empty).Trim();

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

        private void OnCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryList?.SelectedItem is ListBoxItem selected && selected.Tag is string tag)
            {
                UpdateCategoryView(tag);
            }
        }

        private void UpdateCategoryView(string categoryTag)
        {
            bool showLevels = string.Equals(categoryTag, "Levels", StringComparison.OrdinalIgnoreCase);
            bool showMenu = string.Equals(categoryTag, "Menu", StringComparison.OrdinalIgnoreCase);
            if (LevelsContent != null)
            {
                LevelsContent.Visibility = showLevels ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }

            if (MenuContent != null)
            {
                MenuContent.Visibility = showMenu ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }

            bool showPlaceholder = !showLevels && !showMenu;
            if (PlaceholderContent != null)
            {
                PlaceholderContent.Visibility = showPlaceholder ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }

            if (PlaceholderText != null && showPlaceholder)
            {
                string message;
                switch (categoryTag)
                {
                    case "Views":
                        message = "Einstellungen für Ansichten folgen in einem späteren Schritt.";
                        break;
                    case "Menu":
                        message = "Menü-Konfiguration wird vorbereitet.";
                        break;
                    case "Future":
                        message = "Weitere Funktionen sind in Planung.";
                        break;
                    default:
                        message = "Auswahl wird vorbereitet.";
                        break;
                }

                PlaceholderText.Text = message;
            }
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
                long typeIdValue = ElementIdHelper.GetIdValue(type.Id);
                _levelTypes.Add(new LevelTypeOption(type.Name, typeIdValue));
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
