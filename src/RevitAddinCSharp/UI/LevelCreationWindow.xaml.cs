using System;
using System.Globalization;
using System.Windows;

namespace RevitAddinCSharp.UI
{
    public partial class LevelCreationWindow : Window
    {
        public int AboveCount { get; private set; }
        public double AboveHeightMm { get; private set; }
        public int BelowCount { get; private set; }
        public double BelowHeightMm { get; private set; }

        public LevelCreationWindow()
        {
            InitializeComponent();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseInt(AboveCountInput.Text, 0, 50, out int aboveCount))
            {
                ShowValidation("Bitte eine gültige Anzahl Obergeschosse (0-50) eingeben.");
                return;
            }

            if (!TryParseDouble(AboveHeightInput.Text, 500, 10000, out double aboveHeightMm))
            {
                ShowValidation("Bitte eine gültige Obergeschoss-Höhe in mm (500-10000) eingeben.");
                return;
            }

            if (!TryParseInt(BelowCountInput.Text, 0, 20, out int belowCount))
            {
                ShowValidation("Bitte eine gültige Anzahl Untergeschosse (0-20) eingeben.");
                return;
            }

            if (!TryParseDouble(BelowHeightInput.Text, 500, 10000, out double belowHeightMm))
            {
                ShowValidation("Bitte eine gültige Untergeschoss-Höhe in mm (500-10000) eingeben.");
                return;
            }

            AboveCount = aboveCount;
            AboveHeightMm = aboveHeightMm;
            BelowCount = belowCount;
            BelowHeightMm = belowHeightMm;

            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
