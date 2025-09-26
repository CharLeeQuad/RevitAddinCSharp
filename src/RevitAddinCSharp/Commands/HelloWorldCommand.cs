using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitAddinCSharp.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class HelloWorldCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (commandData == null)
            {
                message = "ExternalCommandData ist null.";
                return Result.Failed;
            }

            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;

            Document doc = uiDoc?.Document;
            if (doc == null)
            {
                TaskDialog.Show("RevitAddinCSharp", "Kein Dokument geöffnet.");
                return Result.Cancelled;
            }

            TaskDialog.Show("RevitAddinCSharp", "Hallo Revit! Das Add-in läuft.");
            return Result.Succeeded;
        }
    }
}
