using System;
using System.ComponentModel;
using System.Globalization;

namespace RevitAddinCSharp.UI
{
    public enum LevelCategory
    {
        Building,
        TopEdge,
        BottomEdge,
        Helper
    }

    public class PlannedLevelItem : INotifyPropertyChanged
    {
        private int _order;
        private string _name = string.Empty;
        private LevelCategory _category;
        private double _elevationMm;
        private string _shortLabel = string.Empty;
        private bool _isBuildingStory;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Order
        {
            get => _order;
            set
            {
                if (_order != value)
                {
                    _order = value;
                    OnPropertyChanged(nameof(Order));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                string newValue = value ?? string.Empty;
                if (!string.Equals(_name, newValue, StringComparison.Ordinal))
                {
                    _name = newValue;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public LevelCategory Category
        {
            get => _category;
            set
            {
                if (_category != value)
                {
                    _category = value;
                    OnPropertyChanged(nameof(Category));
                    OnPropertyChanged(nameof(CategoryLabel));
                }
            }
        }

        public string CategoryLabel
        {
            get
            {
                switch (Category)
                {
                    case LevelCategory.Building:
                        return "Gebäudegeschoss";
                    case LevelCategory.TopEdge:
                        return "Oberkante";
                    case LevelCategory.BottomEdge:
                        return "Unterkante";
                    case LevelCategory.Helper:
                        return "Hilfsebene";
                    default:
                        return Category.ToString();
                }
            }
        }

        public double ElevationMm
        {
            get => _elevationMm;
            set
            {
                if (Math.Abs(_elevationMm - value) > 1e-6)
                {
                    _elevationMm = value;
                    OnPropertyChanged(nameof(ElevationMm));
                    OnPropertyChanged(nameof(ElevationText));
                }
            }
        }

        public string ElevationText
        {
            get => ElevationMm.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                {
                    ElevationMm = parsed;
                }
            }
        }

        public string ShortLabel
        {
            get => _shortLabel;
            set
            {
                string newValue = value ?? string.Empty;
                if (!string.Equals(_shortLabel, newValue, StringComparison.Ordinal))
                {
                    _shortLabel = newValue;
                    OnPropertyChanged(nameof(ShortLabel));
                }
            }
        }

        public bool IsBuildingStory
        {
            get => _isBuildingStory;
            set
            {
                if (_isBuildingStory != value)
                {
                    _isBuildingStory = value;
                    OnPropertyChanged(nameof(IsBuildingStory));
                }
            }
        }

        public PlannedLevelItem Clone()
        {
            return new PlannedLevelItem
            {
                Order = Order,
                Name = Name,
                Category = Category,
                ElevationMm = ElevationMm,
                ShortLabel = ShortLabel,
                IsBuildingStory = IsBuildingStory
            };
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
