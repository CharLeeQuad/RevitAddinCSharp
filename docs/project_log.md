# Projektlogbuch

## Überblick
- Repository: `c:\Users\ChristophQuatmann\OneDrive - DES GmbH\03_DEV\RevitAddinC#` / GitHub `https://github.com/CharLeeQuad/RevitAddinCSharp`
- Ziel: Revit-Add-in zur Beschleunigung des Projektstarts (Ebenen, Ansichten etc.)
- Unterstützte Revit-Versionen: 2022, 2024, 2026

## Entwicklungsumgebung
- IDE: Windsurf (VS Code Variante)
- .NET SDK lokal: `dotnet 9.0.304`
- Projektziel-Framework: `.NET Framework 4.8`
- Revit API Pfad (Standard): `C:\Program Files\Autodesk\Revit 2024`

## Bisher erledigt
1. Git eingerichtet (globale Konfiguration, Repo initialisiert, GitHub angebunden).
2. Solution `RevitAddinCSharp.sln` mit Projekt `src/RevitAddinCSharp` erstellt.
3. Projekt auf `net48` ausgerichtet, Revit API Referenzen eingebunden, Deployment-Targets (Copy in Add-in-Ordner) konfiguriert.
4. Add-in-Manifest `deploy/RevitAddinCSharp.addin` angelegt.
5. Erstes Kommando `HelloWorldCommand` (`IExternalCommand`) implementiert und erfolgreich in Revit 2024 getestet (TaskDialog erscheint).
6. Application-Entry `App` (`IExternalApplication`) ergänzt, Ribbon-Tab `DES Tools` mit Panel `Projektstart` und Button für `HelloWorldCommand` eingerichtet.
7. Dialog `LevelCreationForm` erstellt und Command `CreateLevelsCommand` implementiert, der Ober-/Untergeschosse gemäß Eingaben erzeugt; Ribbon-Button `Geschosse` verknüpft.
8. UI auf WPF umgestellt (`LevelCreationWindow`), Command angepasst, WinForms entfernt.
9. Änderungen committet und nach GitHub gepusht (`Add initial Revit add-in scaffold`).

## Wichtige Dateien
- `src/RevitAddinCSharp/RevitAddinCSharp.csproj`
- `src/RevitAddinCSharp/Commands/HelloWorldCommand.cs`
- `deploy/RevitAddinCSharp.addin`
- `docs/project_log.md` (dieses Logbuch)

## Nächste Schritte (Vorschlag)
- Konkrete Funktion(en) zur Projektinitialisierung definieren (z. B. Ebenenerstellung).
- Unit-/Integrationstestszenario bestimmen (Test-RVT vorbereiten).
- Optional: Build-Konfiguration anpassen (Release, Mehrfach-Targeting falls Revit 2026 .NET 8 benötigt).
- Dokumentation fortlaufend pflegen (diese Datei bei Änderungen aktualisieren).
- Formular `LevelCreationForm` langfristig auf WPF/XAML umstellen (bessere Skalierung & Styling).
