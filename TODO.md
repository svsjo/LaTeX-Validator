# LaTeX-Validator
von Jonas Weis und Jonathan Schwab

# Aktuelle Funktionen

## Vorarbeit
- Auslesen aller Glossar-Einträge
- Auslesen aller Akronym-Einträge
- Auslesen aller Linien aller .tex Dateien
- Hauptverzeichnis bereits konfigurierbar
- TODO:
  - Glossarname und Verzeichnis konfigurierbar machen

## Falsche Verwendung von \gls
- Prüfung aller Dateien im Verzeichnis 03_Preamble
- Erkennung wenn dort \gls statt \acrlong bzw. \acrshort verwendet wurde
- TODO: 
  - Verzeichnis auswählbar machen
  - Dateien durch Klick ignorierbar machen (togglebar)
  - Einträge durch Klick ignorierbar machen (togglebar)
  
## Verwendung von Worten ohne \gls
- Prüfung aller .tex-Dateien auf Verwendung von Einträgen (Glossar/Akronym)
- Erkennung aller Verwendungen ohne Nutzung von \gls bzw. \acrlong oder \acrshort
- TODO:
  - Einträge durch Klick ignorierbar machen (togglebar) 
  - Dateien durch Klick ignorierbar machen (togglebar)
  - Genauer analysieren (Regex), aktuell wird ein Wort in \cite{...} oft erkannt, wegen der Klammern
  
## Falsche Verwendung von \gls in Figuren
- Prüfung aller Tabellen, Bilder und Quellcodes auf Verwendung von Glossar/Akronym
- Erkennung aller Nutzungen von \gls (es sollte immer \acrlong bzw. \acrshort) verwendet werden

## Fehlende Referenzierung von Labels
- Auslesen aller Labels und aller Referenzen (\autoref)
- Überprüfung ob jedes Label mindestens einmal referenziert wurde
- TODO:
  - Selektierbar machen, dass verschiedene Label(Chapter, Section, ...) ignoriert werden (togglebar)
  - Anzeige der Verwendung von \ref statt \autoref
  
# Nacharbeit
- Alle Fehler angezeigt inklusive Datei und Zeile
- Alle Fehler anklickbar und automatisches Offnen von VS Code an der passenden Stelle
