using System;
using System.IO;
using System.Xml.Serialization;

namespace RevitAddinCSharp.Settings
{
    public class LevelCreationSettings
    {
        private const string SettingsFolderName = "RevitAddinCSharp";
        private const string SettingsFileName = "LevelCreationSettings.xml";

        private static readonly LevelCreationSettings _current;

        static LevelCreationSettings()
        {
            LevelCreationSettings loaded = LoadFromDisk();
            _current = loaded ?? CreateDefault();
        }

        public static LevelCreationSettings Current => _current;

        public bool AlwaysMarkAsBuildingStory { get; set; }
        public double DefaultComputationHeightMm { get; set; }
        public int DefaultAboveCount { get; set; }
        public double DefaultAboveHeightMm { get; set; }
        public int DefaultBelowCount { get; set; }
        public double DefaultBelowHeightMm { get; set; }
        public string PreferredLevelTypeName { get; set; }
        public long PreferredLevelTypeId { get; set; }
        public bool RenameExistingLevels { get; set; }
        public int LevelNumberDigits { get; set; }
        public string LevelNameSuffix { get; set; }
        public string GroundFloorToken { get; set; }
        public string UpperFloorToken { get; set; }
        public string TopFloorToken { get; set; }
        public string BasementFloorToken { get; set; }

        public static LevelCreationSettings CreateDefault()
        {
            return new LevelCreationSettings
            {
                AlwaysMarkAsBuildingStory = true,
                DefaultComputationHeightMm = 1000.0,
                DefaultAboveCount = 5,
                DefaultAboveHeightMm = 3000.0,
                DefaultBelowCount = 0,
                DefaultBelowHeightMm = 3000.0,
                PreferredLevelTypeName = string.Empty,
                PreferredLevelTypeId = 0,
                RenameExistingLevels = true,
                LevelNumberDigits = 2,
                LevelNameSuffix = "_OKRF",
                GroundFloorToken = "EG",
                UpperFloorToken = "OG",
                TopFloorToken = "DG",
                BasementFloorToken = "UG"
            };
        }

        public LevelCreationSettings Clone()
        {
            return new LevelCreationSettings
            {
                AlwaysMarkAsBuildingStory = AlwaysMarkAsBuildingStory,
                DefaultComputationHeightMm = DefaultComputationHeightMm,
                DefaultAboveCount = DefaultAboveCount,
                DefaultAboveHeightMm = DefaultAboveHeightMm,
                DefaultBelowCount = DefaultBelowCount,
                DefaultBelowHeightMm = DefaultBelowHeightMm,
                PreferredLevelTypeName = PreferredLevelTypeName,
                PreferredLevelTypeId = PreferredLevelTypeId,
                RenameExistingLevels = RenameExistingLevels,
                LevelNumberDigits = LevelNumberDigits,
                LevelNameSuffix = LevelNameSuffix,
                GroundFloorToken = GroundFloorToken,
                UpperFloorToken = UpperFloorToken,
                TopFloorToken = TopFloorToken,
                BasementFloorToken = BasementFloorToken
            };
        }

        public void Apply(LevelCreationSettings source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            AlwaysMarkAsBuildingStory = source.AlwaysMarkAsBuildingStory;
            DefaultComputationHeightMm = source.DefaultComputationHeightMm;
            DefaultAboveCount = source.DefaultAboveCount;
            DefaultAboveHeightMm = source.DefaultAboveHeightMm;
            DefaultBelowCount = source.DefaultBelowCount;
            DefaultBelowHeightMm = source.DefaultBelowHeightMm;
            PreferredLevelTypeName = source.PreferredLevelTypeName;
            PreferredLevelTypeId = source.PreferredLevelTypeId;
            RenameExistingLevels = source.RenameExistingLevels;
            LevelNumberDigits = source.LevelNumberDigits;
            LevelNameSuffix = source.LevelNameSuffix;
            GroundFloorToken = source.GroundFloorToken;
            UpperFloorToken = source.UpperFloorToken;
            TopFloorToken = source.TopFloorToken;
            BasementFloorToken = source.BasementFloorToken;

            SaveCurrent();
        }

        public void ResetToDefaults()
        {
            Apply(CreateDefault());
        }

        private static LevelCreationSettings LoadFromDisk()
        {
            try
            {
                string path = GetSettingsFilePath();
                if (!File.Exists(path))
                {
                    return null;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(LevelCreationSettings));
                using (FileStream stream = File.OpenRead(path))
                {
                    object result = serializer.Deserialize(stream);
                    return result as LevelCreationSettings;
                }
            }
            catch
            {
                return null;
            }
        }

        private static void SaveCurrent()
        {
            try
            {
                string directory = GetSettingsDirectory();
                Directory.CreateDirectory(directory);

                XmlSerializer serializer = new XmlSerializer(typeof(LevelCreationSettings));
                string path = GetSettingsFilePath();
                using (FileStream stream = File.Create(path))
                {
                    serializer.Serialize(stream, _current);
                }
            }
            catch
            {
            }
        }

        private static string GetSettingsDirectory()
        {
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(roaming, SettingsFolderName);
        }

        private static string GetSettingsFilePath()
        {
            return Path.Combine(GetSettingsDirectory(), SettingsFileName);
        }
    }
}
