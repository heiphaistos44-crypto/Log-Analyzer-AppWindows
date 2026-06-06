# WinLog Analyzer

Application **desktop Windows** (WPF, .NET 8) qui analyse l'Observateur d'evenements,
extrait les erreurs Critical/Error, traduit les PID en noms de process, et affiche pour
chaque Event ID connu une **explication + remediation** issue d'une base de connaissance locale.

Interface native (pas de navigateur, pas de serveur web).

## Architecture

```
WinLogAnalyzer/
├── src/
│   ├── WinLogAnalyzer.Core/     # Lib metier (lecture EventLog, mapping, dictionnaire)
│   │   ├── Models/{EventEntry,Solution}.cs
│   │   ├── Process/ProcessResolver.cs    # PID -> nom ("[termine]" si process mort)
│   │   ├── Knowledge/SolutionProvider.cs # charge solutions.json
│   │   └── Reader/EventLogService.cs     # lecture streaming Level 1+2
│   └── WinLogAnalyzer.App/      # Interface WPF (MVVM)
│       ├── data/solutions.json           # base de connaissance Event ID -> solution
│       ├── Themes/Dark.xaml              # theme sombre
│       ├── ViewModels/                   # MainViewModel, EventItemViewModel
│       ├── Infrastructure/               # RelayCommand, converters
│       └── MainWindow.xaml               # fenetre principale
├── build.bat                    # Clean build -> dist/WinLogAnalyzer.exe
├── run.bat                      # Lance l'app en admin
└── .logs/                       # Logs build (jamais commit)
```

## Prerequis

- **.NET 8 SDK** (`dotnet --version` >= 8.0)
- Windows 10/11

## Build & Run

```bat
build.bat        :: KILL -> CLEAN -> publish single-file -> VERIFY
run.bat          :: lance dist\WinLogAnalyzer.exe (admin)
```

L'application s'ouvre et analyse automatiquement le journal System au demarrage.

## Utilisation

- **Journal** : System / Application / Security.
- **Nombre max** : 1 a 1000 erreurs.
- **Filtre** : recherche live (Event ID, source, message, titre solution).
- **Analyser** : relance la lecture.
- Cliquer une carte la **deplie** : message brut + explication + remediation + liens doc.

## Ajouter une solution

Editer `src/WinLogAnalyzer.App/data/solutions.json` — **aucune recompilation du code** :

```json
"1234": {
  "title": "Titre court",
  "explanation": "Cause du probleme.",
  "remediation": "Etapes de correction numerotees.",
  "severity": "critical",
  "links": ["https://learn.microsoft.com/..."]
}
```

Cle = Event ID. `severity` : `critical | error | warning | info` (pilote la couleur).
Rebuild (`build.bat`) pour copier le JSON mis a jour a cote du binaire.

## Notes techniques

- Lecture **streaming** (`EventLogReader`) : pas de charge complete en RAM.
- `EventRecord.ProcessId` = PID au moment de l'event. Process mort -> `[termine]`.
- Lecture hors thread UI (Task.Run) : interface jamais figee.
- Manifest `requireAdministrator` (lecture du log Security).
- Donnees 100% locales, jamais transmises.
