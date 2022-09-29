# LaTeX-Validator
von Jonas Weis und Jonathan Schwab

# Aktuelle Funktionen

## Vorarbeit
- Auslesen aller Glossar-Einträge (Definition)
- Auslesen aller Akronym-Einträge (Definition)
- Auslesen aller Zitat-Labels (Definition)
- Auslesen aller normalen Label (Definition)
- Auslesen aller Linien aller .tex Dateien
- Auslesen aller Sätze
- Auslesen aller Areas (begin{...} <> end{...})
- Auslesen aller Referenzen (Verwendung)
- Auslesen aller Zitate (Verwendung)
- Hauptverzeichnis, Glossar und Preamble konfigurierbar

## Falsche Verwendung von \gls
- Prüfung aller Dateien im Verzeichnis Preamble
- Verzeichnis Preamble mit FilePicker auswählbar
- Erkennung wenn dort \gls statt \acrlong bzw. \acrshort verwendet wurde
  
## Verwendung von Worten ohne \gls
- Prüfung aller .tex-Dateien auf Verwendung von Einträgen aus Glossar/Akronym
- Erkennung aller Verwendungen ohne Nutzung von \gls bzw. \acrlong oder \acrshort
- Dateien durch FilePicker ignorierbar (togglebar)
  
## Falsche Verwendung von \gls in Figuren
- Prüfung aller Tabellen, Bilder und Quellcodes, sowie aller Überschriften (Chapter und Co.), auf Verwendung von Glossar/Akronym
- Erkennung aller Nutzungen von \gls (es sollte immer \acrlong bzw. \acrshort verwendet werden)

## Fehlende Referenzierung von Labels
- Überprüfung und Anzeige ob jedes Label mindestens einmal referenziert wurde
- Definition von Labels zum ignorieren und Auswahlmöglichkeit ob diese ignoriert werden
- Selektierbar, ob verschiedene Sektionen-Labels (Chapter, Section, Subsection) ignoriert werden (togglebar)
- Anzeige wenn \ref statt \autoref verwendet wurde
- Anzeige aller Labels ohne Referenz
 
## Fehlende Zitation von Quellen
- Überprüfung ob alle Quellen zitiert wurden und Anzeige welche nicht zitiert wurden
- Definition von Labels zum ignorieren und Auswahlmöglichkeit ob diese ignoriert werden

## Füllworte finden
- (Füll-)Worte sind definierbar
- Definierte sind suchbar (aktuell nur jene mit Leerzeichen davor und danach)
  
## Falsche Verweise
- Verweise auf nicht vorhandene Labels bei Referenzierung oder Zitation werden gesucht
- Anzeige aller Fehlverweise

## Satzlänge
- Prüfungen auf Sätzlänge > x
- Anzeige von Satzlänge > 22 als Warnung und > 30 als Error

## Fehlende Labels oder Captions
- Prüfung ob alle Tabellen, Figuren und Code jeweils Label und Caption besitzen
- Anzeige aller wo Label oder Caption fehlt

## Nacharbeit
- Alle Fehler angezeigt inklusive Datei, Zeile, Position und einem Ausschnitt der Umgebung mit fett markiertem Fehler
- Alle Fehler anklickbar und automatisches Offnen von VS Code an der passenden Stelle, sowie Markierung des Wortes
- Alle Fehler persistent ignorierbar (togglebar)
- Hovern über der Spalte "Umgebung" gibt per Splashtext mehr Details
- Reset-Knopf für die einzelnen Pfade und generell alles
- Persistentes Speichern über JSON-Serialisierung in String

# Allgemein offen
Siehe unter Issues
