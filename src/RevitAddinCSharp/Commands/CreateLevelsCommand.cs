using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAddinCSharp.UI;
using System.Windows.Interop;

namespace RevitAddinCSharp.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateLevelsCommand : IExternalCommand
    {
        private const double DefaultComputationHeightMm = 1000.0;
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

            LevelCreationWindow window = new LevelCreationWindow();
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

            int aboveCount = window.AboveCount;
            double aboveHeightMm = window.AboveHeightMm;
            int belowCount = window.BelowCount;
            double belowHeightMm = window.BelowHeightMm;

            if (aboveCount == 0 && belowCount == 0)
            {
                TaskDialog.Show("Ebenen anlegen", "Keine Geschosse ausgewählt.");
                return Result.Cancelled;
            }

            if (aboveHeightMm <= 0 || belowHeightMm <= 0)
            {
                message = "Die Geschosshöhe muss größer als 0 sein.";
                return Result.Failed;
            }

#if REVIT2021_OR_OLDER
            double aboveHeightFt = UnitUtils.ConvertToInternalUnits(aboveHeightMm, DisplayUnitType.DUT_MILLIMETERS);
            double belowHeightFt = UnitUtils.ConvertToInternalUnits(belowHeightMm, DisplayUnitType.DUT_MILLIMETERS);
            double computationHeightFt = UnitUtils.ConvertToInternalUnits(DefaultComputationHeightMm, DisplayUnitType.DUT_MILLIMETERS);
#else
            ForgeTypeId millimeterId = UnitTypeId.Millimeters;
            double aboveHeightFt = UnitUtils.ConvertToInternalUnits(aboveHeightMm, millimeterId);
            double belowHeightFt = UnitUtils.ConvertToInternalUnits(belowHeightMm, millimeterId);
            double computationHeightFt = UnitUtils.ConvertToInternalUnits(DefaultComputationHeightMm, millimeterId);
#endif

            using (Transaction tx = new Transaction(doc, "Geschosse anlegen"))
            {
                tx.Start();

                CreateLevels(doc, aboveCount, aboveHeightFt, true, computationHeightFt);
                CreateLevels(doc, belowCount, belowHeightFt, false, computationHeightFt);

                tx.Commit();
            }

            TaskDialog.Show("Ebenen anlegen", "Die Geschosse wurden erzeugt.");
            return Result.Succeeded;
        }

        private static void CreateLevels(Document doc, int count, double heightStep, bool positive, double computationHeightFt)
        {
            if (count <= 0)
            {
                return;
            }

            if (positive)
            {
                for (int index = 0; index < count; index++)
                {
                    double elevation = index * heightStep;
                    Level level = EnsureLevel(doc, elevation);
                    ApplyDefaultParameters(level, computationHeightFt);
                }

                return;
            }

            for (int index = 1; index <= count; index++)
            {
                double elevation = positive ? index * heightStep : -index * heightStep;
                Level level = EnsureLevel(doc, elevation);
                ApplyDefaultParameters(level, computationHeightFt);
            }
        }

        private static Level EnsureLevel(Document doc, double elevation)
        {
            Level existing = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault(level => Math.Abs(level.Elevation - elevation) < LevelComparisonTolerance);

            if (existing != null)
            {
                return existing;
            }

            return Level.Create(doc, elevation);
        }

        private static void ApplyDefaultParameters(Level level, double computationHeightFt)
        {
            if (level == null)
            {
                return;
            }

            Parameter storyParam = level.get_Parameter(BuiltInParameter.LEVEL_IS_BUILDING_STORY);
            if (storyParam != null && !storyParam.IsReadOnly && storyParam.AsInteger() != 1)
            {
                storyParam.Set(1);
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
    }

}
