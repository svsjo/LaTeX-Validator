# LaTeX-Validator
von Jonas Weis und Jonathan Schwab

# Aktuelle Funktionen

## Vorarbeit
- Auslesen aller Glossar-Einträge
- Auslesen aller Akronym-Einträge
- Auslesen aller Linien aller .tex Dateien
- Hauptverzeichnis, Glossar und Preamble konfigurierbar

## Falsche Verwendung von \gls
- Prüfung aller Dateien im Verzeichnis Preamble
- Verzeichnis Preamble mit FilePicker auswählbar
- Erkennung wenn dort \gls statt \acrlong bzw. \acrshort verwendet wurde
  
## Verwendung von Worten ohne \gls
- Prüfung aller .tex-Dateien auf Verwendung von Einträgen (Glossar/Akronym)
- Erkennung aller Verwendungen ohne Nutzung von \gls bzw. \acrlong oder \acrshort
- Dateien durch FilePicker ignorierbar (togglebar)
- **TODO**: Ggf. Auswahl ob case sensitiv oder nicht ? (aktuell ist case sensitiv)
  
## Falsche Verwendung von \gls in Figuren
- Prüfung aller Tabellen, Bilder und Quellcodes auf Verwendung von Glossar/Akronym
- Erkennung aller Nutzungen von \gls (es sollte immer \acrlong bzw. \acrshort verwendet werden)
- **TODO**: Selbes für \chapter \section \subsection und Co. (damit nicht im Inhaltsverzeichnis eingeführt wird)

## Fehlende Referenzierung von Labels
- Auslesen aller Labels und aller Referenzen (\autoref und \ref)
- Überprüfung und Anzeige ob jedes Label mindestens einmal referenziert wurde
- Definition von Labels zum ignorieren und Auswahlmöglichkeit ob diese ignoriert werden
- Selektierbar, ob verschiedene Sektionen-Labels (Chapter, Section, Subsection) ignoriert werden (togglebar)
- Anzeige wenn \ref statt \autoref verwendet wurde
 
## Fehlende Zitation von Quellen
- Auslesen aller Labels der Quellen
- Überprüfung ob alle zitiert wurden und Anzeige welche nicht zitiert wurden
- Definition von Labels zum ignorieren und Auswahlmöglichkeit ob diese ignoriert werden
- **TODO**: Anzeige von falschen Zitationen (Label nicht vorhanden)

## Füllworte finden
- (Füll-)Worte sind definierbar
- Definierte sind suchbar (aktuell nur jene mit Leerzeichen davor und danach)
- **TODO**: Suche besser machen (Satzanfang mit einbeziehen, ...)
  
## Nacharbeit
- Alle Fehler angezeigt inklusive Datei, Zeile, Position und einem Ausschnitt der Umgebung mit fett markiertem Fehler
- Alle Fehler anklickbar und automatisches Offnen von VS Code an der passenden Stelle
- Alle Fehler persistent ignorierbar (togglebar)

# Allgemein offen
- Prüfung ob alle Figuren/Tabellen/Codes eine Caption haben
- Methode ```FindTablesErrors``` erweitern, damit neben den Tabellen, Quellcode und Bildern auch Überschriften, Sektionen und Untersektionen beachtet werden
- Anzeige von falschen Zitationen (Label nicht vorhanden) => Grüße gehen an @jdev
- Füllwortsuche verbessern
- Prüfung auf Sätzlange > x ?
- Auswahl ob casesensitiv oder nicht (aktuell ist immer casesensitiv) ?
- Architektur und Aussehen der Anwendung verbessern ?
