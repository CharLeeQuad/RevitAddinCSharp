using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Interop;
using RevitAddinCSharp.Settings;
using RevitAddinCSharp.UI;
using RevitAddinCSharp.Utils;

namespace RevitAddinCSharp.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateLevelsCommand : IExternalCommand
    {
        private const double LevelComparisonTolerance = 1e-4;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (commandData == null)
            {
                message = "CommandData ist null.";
                return Result.Failed;
            }

            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc != null ? uiDoc.Document : null;

            if (doc == null)
            {
                TaskDialog.Show("Ebenen anlegen", "Bitte öffnen Sie ein Revit-Modell.");
                return Result.Cancelled;
            }

            LevelCreationSettings settings = LevelCreationSettings.Current;

            LevelCreationWindow window = new LevelCreationWindow(settings);
            IntPtr revitHandle = uiApp.MainWindowHandle;

            WindowInteropHelper interopHelper = new WindowInteropHelper(window)
            {
                Owner = revitHandle
            };

            bool? dialogResult = window.ShowDialog();
            if (dialogResult != true)
            {
                return Result.Cancelled;
            }

            IList<PlannedLevelItem> plannedLevels = window.ResultLevels;
            if (plannedLevels == null || plannedLevels.Count == 0)
            {
                TaskDialog.Show("Ebenen planen", "Der Plan enthält keine Ebenen.");
                return Result.Cancelled;
            }

#if REVIT2021_OR_OLDER
            double computationHeightFt = UnitUtils.ConvertToInternalUnits(settings.DefaultComputationHeightMm, DisplayUnitType.DUT_MILLIMETERS);
#else
            ForgeTypeId millimeterId = UnitTypeId.Millimeters;
            double computationHeightFt = UnitUtils.ConvertToInternalUnits(settings.DefaultComputationHeightMm, millimeterId);
#endif

            ElementId levelTypeId = ResolveLevelTypeId(doc, settings);

            using (Transaction tx = new Transaction(doc, "Geschosse anlegen"))
            {
                tx.Start();

                CreatePlannedLevels(doc, plannedLevels, computationHeightFt, settings, levelTypeId
#if !REVIT2021_OR_OLDER
                    , millimeterId
#endif
                    );

                tx.Commit();
            }

            TaskDialog.Show("Ebenen anlegen", "Die Geschosse wurden erzeugt.");
            return Result.Succeeded;
        }

        private static void CreatePlannedLevels(
            Document doc,
            IList<PlannedLevelItem> plannedLevels,
            double computationHeightFt,
            LevelCreationSettings settings,
            ElementId levelTypeId
#if !REVIT2021_OR_OLDER
            , ForgeTypeId millimeterId
#endif
            )
        {
            if (doc == null || plannedLevels == null || plannedLevels.Count == 0)
            {
                return;
            }

            foreach (PlannedLevelItem item in plannedLevels.OrderBy(level => level.Order))
            {
                double elevationMm = item.ElevationMm;
#if REVIT2021_OR_OLDER
                double elevationFt = UnitUtils.ConvertToInternalUnits(elevationMm, DisplayUnitType.DUT_MILLIMETERS);
#else
                double elevationFt = UnitUtils.ConvertToInternalUnits(elevationMm, millimeterId);
#endif

                Level level = EnsureLevel(doc, elevationFt, out bool created);
                ApplyLevelType(level, levelTypeId, created);
                bool markAsBuildingStory = item.Category == LevelCategory.Building && item.IsBuildingStory;
                ApplyDefaultParameters(level, computationHeightFt, markAsBuildingStory);
                ApplyLevelName(level, settings, created, item.Name);
            }
        }

        private static Level EnsureLevel(Document doc, double elevation, out bool created)
        {
            Level existing = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault(level => Math.Abs(level.Elevation - elevation) < LevelComparisonTolerance);

            if (existing != null)
            {
                created = false;
                return existing;
            }

            created = true;
            return Level.Create(doc, elevation);
        }

        private static void ApplyDefaultParameters(Level level, double computationHeightFt, bool markAsBuildingStory)
        {
            if (level == null)
            {
                return;
            }

            Parameter storyParam = level.get_Parameter(BuiltInParameter.LEVEL_IS_BUILDING_STORY);
            if (storyParam != null && !storyParam.IsReadOnly)
            {
                int desiredValue = markAsBuildingStory ? 1 : 0;
                if (storyParam.AsInteger() != desiredValue)
                {
                    storyParam.Set(desiredValue);
                }
            }

            Parameter computationParam = level.get_Parameter(BuiltInParameter.LEVEL_ROOM_COMPUTATION_HEIGHT);
            if (computationParam != null && !computationParam.IsReadOnly)
            {
                double currentValue = computationParam.AsDouble();
                if (Math.Abs(currentValue - computationHeightFt) > LevelComparisonTolerance)
                {
                    computationParam.Set(computationHeightFt);
                }
            }
        }

        private static void ApplyLevelType(Level level, ElementId desiredTypeId, bool created)
        {
            if (level == null || desiredTypeId == null || desiredTypeId == ElementId.InvalidElementId)
            {
                return;
            }

            ElementId currentTypeId = level.GetTypeId();

            if (!created && currentTypeId == desiredTypeId)
            {
                return;
            }

            if (created || currentTypeId != desiredTypeId)
            {
                level.ChangeTypeId(desiredTypeId);
            }
        }

        private static void ApplyLevelName(Level level, LevelCreationSettings settings, bool created, string desiredName)
        {
            if (level == null || settings == null)
            {
                return;
            }

            bool shouldRename = created || settings.RenameExistingLevels;
            if (!shouldRename)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(desiredName))
            {
                return;
            }

            string trimmed = desiredName.Trim();
            if (!string.Equals(level.Name, trimmed, StringComparison.Ordinal))
            {
                level.Name = trimmed;
            }
        }

        private static ElementId ResolveLevelTypeId(Document doc, LevelCreationSettings settings)
        {
            if (doc == null)
            {
                return ElementId.InvalidElementId;
            }

            if (settings != null)
            {
                if (settings.PreferredLevelTypeId != 0)
                {
                    ElementId storedId = ElementIdHelper.Create(settings.PreferredLevelTypeId);
                    Element element = doc.GetElement(storedId);
                    if (element is LevelType)
                    {
                        return storedId;
                    }
                }

                if (!string.IsNullOrWhiteSpace(settings.PreferredLevelTypeName))
                {
                    LevelType byName = new FilteredElementCollector(doc)
                        .OfClass(typeof(LevelType))
                        .Cast<LevelType>()
                        .FirstOrDefault(type => string.Equals(type.Name, settings.PreferredLevelTypeName, StringComparison.OrdinalIgnoreCase));

                    if (byName != null)
                    {
                        return byName.Id;
                    }
                }
            }

            ElementId defaultId = doc.GetDefaultElementTypeId(ElementTypeGroup.LevelType);
            if (defaultId != ElementId.InvalidElementId)
            {
                return defaultId;
            }

            LevelType firstType = new FilteredElementCollector(doc)
                .OfClass(typeof(LevelType))
                .Cast<LevelType>()
                .FirstOrDefault();

            return firstType?.Id ?? ElementId.InvalidElementId;
     }
    }

}
