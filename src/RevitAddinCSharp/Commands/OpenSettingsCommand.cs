using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Interop;
using RevitAddinCSharp.UI;

namespace RevitAddinCSharp.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class OpenSettingsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (commandData == null)
            {
                message = "CommandData ist null.";
                return Result.Failed;
            }

            UIApplication uiApp = commandData.Application;
            if (uiApp == null)
            {
                message = "UIApplication ist null.";
                return Result.Failed;
            }

            Document document = uiApp.ActiveUIDocument?.Document;
            LevelSettingsWindow window = new LevelSettingsWindow(document);
            IntPtr revitHandle = uiApp.MainWindowHandle;

            WindowInteropHelper interopHelper = new WindowInteropHelper(window)
            {
                Owner = revitHandle
            };

            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
