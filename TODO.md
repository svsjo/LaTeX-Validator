# LaTeX-Validator
von Jonas Weis und Jonathan Schwab

# Aktuelle Funktionen

## Vorarbeit
- Auslesen aller Glossar-Einträge
- Auslesen aller Akronym-Einträge
- Auslesen aller Linien aller .tex Dateien
- Hauptverzeichnis, Glossar und Preamble konfigurierbar

## Falsche Verwendung von \gls
- Prüfung aller Dateien im Verzeichnis 03_Preamble
- Erkennung wenn dort \gls statt \acrlong bzw. \acrshort verwendet wurde
- Verzeichnis auswählbar
  
## Verwendung von Worten ohne \gls
- Prüfung aller .tex-Dateien auf Verwendung von Einträgen (Glossar/Akronym)
- Erkennung aller Verwendungen ohne Nutzung von \gls bzw. \acrlong oder \acrshort
- Dateien durch Klick ignorierbar (togglebar)
- TODO:
  - Auswahl ob case sensitiv
  
## Falsche Verwendung von \gls in Figuren
- Prüfung aller Tabellen, Bilder und Quellcodes auf Verwendung von Glossar/Akronym
- Erkennung aller Nutzungen von \gls (es sollte immer \acrlong bzw. \acrshort) verwendet werden

## Fehlende Referenzierung von Labels
- Auslesen aller Labels und aller Referenzen (\autoref)
- Überprüfung ob jedes Label mindestens einmal referenziert wurde
- Selektierbar, ob verschiedene Label(Chapter, Section, ...) ignoriert werden (togglebar)
- Anzeige der Verwendung von \ref statt \autoref
 
## Fehlende Zitation von Quellen
- Auslesen aller Labels der Quellen
- Überprüfung ob alle zitiert wurden
- TODO:
  - Labels ignorierbar machen
  
# Nacharbeit
- Alle Fehler angezeigt inklusive Datei, Zeile und Position
- Alle Fehler anklickbar und automatisches Offnen von VS Code an der passenden Stelle
- Alle Fehler persistent ignorierbar (togglebar)

# Allgemein offen
- Refactoring der Struktur, ggf. Nutzung MVVM
- Weitere Funktionalitäten: 
  - Prüfung ob alle Figuren eine Caption haben ?
  - Prüfung auf vorher definierte Füllworte ?
  - Prüfung auf Sätzlange > x ?
