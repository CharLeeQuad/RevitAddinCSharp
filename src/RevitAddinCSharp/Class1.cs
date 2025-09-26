using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace RevitAddinCSharp
{
    public class App : IExternalApplication
    {
        private const string TabName = "DES Tools";
        private const string PanelName = "Projektstart";
        private const string ButtonName = "HelloWorldButton";
        private const string ButtonText = "Hallo Welt";
        private const string LevelsButtonName = "CreateLevelsButton";
        private const string LevelsButtonText = "Geschosse";
        private const string SettingsButtonName = "LevelSettingsButton";
        private const string SettingsButtonText = "Einstellungen";

        public Result OnStartup(UIControlledApplication application)
        {
            if (application == null)
            {
                return Result.Failed;
            }

            try
            {
                EnsureTab(application);
                RibbonPanel panel = EnsurePanel(application);
                CreateHelloWorldButton(panel);
                CreateLevelsButton(panel);
                CreateSettingsButton(panel);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("RevitAddinCSharp", "Fehler beim Starten des Add-ins:\n" + ex.Message);
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return application == null ? Result.Failed : Result.Succeeded;
        }

        private void EnsureTab(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (InvalidOperationException)
            {
            }
            catch (ArgumentException)
            {
            }
        }

        private RibbonPanel EnsurePanel(UIControlledApplication application)
        {
            RibbonPanel existingPanel = null;
            foreach (RibbonPanel panel in application.GetRibbonPanels(TabName))
            {
                if (panel?.Name == PanelName)
                {
                    existingPanel = panel;
                    break;
                }
            }

            return existingPanel ?? application.CreateRibbonPanel(TabName, PanelName);
        }

        private void CreateHelloWorldButton(RibbonPanel panel)
        {
            if (panel == null)
            {
                return;
            }

            AddButton(panel, ButtonName, ButtonText, typeof(Commands.HelloWorldCommand), "Zeigt einen einfachen TaskDialog an.");
        }

        private void CreateLevelsButton(RibbonPanel panel)
        {
            if (panel == null)
            {
                return;
            }

            AddButton(panel, LevelsButtonName, LevelsButtonText, typeof(Commands.CreateLevelsCommand), "Erstellt Ober- und Untergeschosse nach Vorgabe.");
        }

        private void CreateSettingsButton(RibbonPanel panel)
        {
            if (panel == null)
            {
                return;
            }

            AddButton(panel, SettingsButtonName, SettingsButtonText, typeof(Commands.OpenSettingsCommand), "Öffnet die Einstellungen für die Geschosserstellung.");
        }

        private void AddButton(RibbonPanel panel, string name, string text, Type commandType, string tooltip)
        {
            foreach (RibbonItem item in panel.GetItems())
            {
                if (item is PushButton existing && existing.Name == name)
                {
                    return;
                }
            }

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData data = new PushButtonData(name, text, assemblyPath, commandType.FullName);
            data.ToolTip = tooltip;

            panel.AddItem(data);
        }
    }
}
