# Zusammenfassung nützlicher C#-Features
Im Folgenden findet sich eine kleine Zusammenfassung nützlicher LaTeX-Features mit Beispielen. Diese wurde nach bestem Wissen und Gewissen erstellt.

Autor: Jonas Weis

# Vorraussetzungen
Der Order ```Latex-Vorlage``` und die darin enthaltenen Dateien bilden den Startpunkt. Diese können angepasst und erweitert werden. Generell geben die Ordner darin bereits eine grobe Struktur vor, welche sich als sinnvoll erwiesen hat. Vor deren Nutzung muss aber erst etwas installiert werden:
1. TexStudio (https://www.texstudio.org/)
2. TexLive (https://www.tug.org/texlive/)

# Erläuterungen zur Vorlage

## Ordner
```01_img```: Hier werden alle Bilder als jpg oder png abgelegt.

```02_code```: Hier wird aller Code abgelegt.

```03_Preamble```: Enthält alles, war vor dem Inhaltsverzeichis kommt. Aktuell Abstrakt, Eidesstattliche Erklärung, Glossar und Abkürzungsverzeichnis, Sperrvermerk und Titelblatt.

```04_Content```: Enthält für jedes Kapitel eine eigene ```.tex```-Datei. Beispielhaft sind zwei Kapitel enthalten, in denen z.B. das Einbinden von Bildern und Quellcode abgeschaut werden kann.

```05_Post```: Enthält alles rund um den Anhang, was nach dem Literaturverzeichnis angehängt werden soll. Im Anhang ist beispielhaft eine Tabelle gegeben.

## Dateien
```main.tex``` Hauptelement in welchem alles andere eingebunden wird. Jedes Datei muss hier mit ```\include``` eingebunden werden (siehe Zeile 54-63 und Zeile 73). Hier werden außerdem die Verzeichnisse generiert (siehe Zeile 26-44 und Zeile 69). Ansonsten muss hier eigentlich nichts verändert werden.

```preamble.tex``` Definiert die Einstellungen und bindet Pakete ein. Ist letztendlich die wichtigste aller Dateien. Musst du erstmal nichts dran machen, außer es fehlt die eine Funktionalität.

```einstellungen.tex``` Hier werden Konstanten definiert, welche über die Arbeit mit einem einfachen Command wieder eingefügt werden können. Beispielsweise Titel der Arbeit, Firma, ... Hier müssen als Änderungen vorgenommen werden!

```literatur.bib``` Das Literaturverzeichnis. Hier werden Bücher und andere Quellen angebeben und können dann im Dokument verlinkt werden. In der Vorlage sind bereits einige Quellen vorhanden, die immer wieder verwendet werden können.

```clean.sh``` Beim Erzeugen der PDF werden viele temporäre Dateien erstellt, da danach nicht mehr gebraucht werden. Zur Übersichtlichkeit werden diese mit dem Skript gelöscht. Außerdem wird die erstellte PDF an einen festgelegten Ort kopiert und passend benannt.

## Einstellungen Tex-Studio
```Standardcompiler```: LuaLaTeX

```Standard Bibliographie```: Biber

```Ordner mit allen Anwendungen```: C:/texlive/2021/bin/win32/

## Kompilieren des Dokumentes
Um alles zu aktualisieren ist die Reihenfolge wichtig. Um sicherzugehen folgende Reihenfolge:

```Normal``` -> ```Bibliographie``` -> ```Glossar``` -> ```Normal``` -> ```Normal```

Für Änderungen im Text, ist dies nicht notwendig. nur wenn Änderungen am Glossar oder ähnlichem angezeigt werden sollen.


# Kapitel
Ein Kapitel wird immer durch ```\chapter{Name}``` gestartet. Aus Übersichtlichkeitsgründen hat jedes Kapitel eine eigene Datei. In der Regel wird in ein bis drei Sätzen erläutert, was im Kapitel gemacht wird. 

Ein Kapitel kann mehrere Sektionen enthalten. Die Kapitel werden dabei automatisch nummeriert.

# Sektionen
Eine Sektion wird immer durch ```\section{Name}``` gestartet. Sie ist dabei eine Ebene unter dem Kapitel. Es gibt außerdem noch die Möglichkeit für eine Subsektion durch ```\subsection{Name}```. Somit sind mit dem Kapitel die Möglichkeiten für drei Ebenen zur Unterteilung gegeben. Kapitel > Sektion > Subsektion.

Sie Sektionen werden daber immer abhängig vom Kapitel nummeriert, also beispielsweise ```3.1``` und ```3.2```. Gleiches gilt für Subsektionen (```3.1.1```).

# Label und Referenzieren
Damit an eine Stelle gesprungen, also diese referenziert werden kann, muss ein Label gesetzt werden. Dies wird durch ```\label{Bereich:Name}``` umgesetzt. Generell wird in jedem Kapitel, Sektion oder Untersektion direkt ganz oben ein Label gesetzt. Auch in der Definition von Tabellen und Bildern muss das Label gesetzt werden. Besonders wichtige Stellen im Text können außerdem ein Label enthalten um einen Sprung zu ermöglichen. Auch in Code-Snippets wird ein Label gesetzt, dort geschieht es aber innerhalb des ```[] mit label=...,```

Der Bereich wird dazu genutzt um zwischen eben disen den Arten der Label zu unterscheiden. Als Optionen stehen zur Verfügung: ```chap, sec, subsec, fig, table, lst, txt```. Dies ist optional, sollte aber zur Übersichtlichkeit verwendet werden wenn man referenzieren will.

Die Referenzierung erfolgt dann anhand des Nutzens von ```\autoref{Bereich:Name}```. Dabei wird nicht nur die Nummer geschrieben, sondern beispielsweise ```Abschnitt 2.3```, ```Kapitel 2```, ```Abbildung 15``` oder ```Quellcode 5```.

Wichtig: Alle Tabellen, Bilder und Code-Ausschnitte müssen aus dem Text referenziert werden!

# Bibliografie und Quellenverweis
Generell sind alle Einträge in der ```literatur.bib``` ähnlich aufgebaut. Und zwar 
```bibtex
@ArtDesEintrages{NameUmDaraufZuVerweisen,
    info1 = {},
    info2 = {},
    ...
}
```

## Webseiten
```bibtex
@misc{ARBURGGmbH+CoKG.2021,
	editor = {{ARBURG GmbH + Co KG}},
	year = {2021},
	title = {Daten {\&} Fakten},
	url = {https://www.arburg.com/de/unternehmen/daten-fakten/},
	urldate = {2022-01-12}
}
```

## Artikel
```bibtex
@article{Martin.2000,
	author = {Martin, Robert C.},
	year = {2000},
	title = {Design Principles and Design Patterns},
	urldate = {2022-02-25},
	journal = {objectmentor.com}
}
```

## Bücher
```bibtex
@book{LahresRayman,
	author    = {Lahres, Bernd and Rayman, Gregor}, 
	title     = {Objektorientierte Programmierung - Das umfassende Handbuch},
	publisher = {Rheinwerk Computing},
	year      = {2021},
	edition   = {2st},
	isbn      = {9783836214018}
}
```

## Sonderfälle
Gewisse Zeichen können nicht einfach verwendet werden und müssen daher folgendermaßen escaped werden. 
```
{\&}
f{\"u}r
Abh{\"a}ngigkeiten
W{\"o}rter
```

## Tatsächliches Zitieren
Um auf eine Quelle zu verweisen wird ```\cite{Eintrag}``` verwendet. Das Ergebnis wird im Text dann beispielsweise als ```[3]``` inklusive Hyperlink eingefügt. Der Autor kann über ```\citeauthor{Eintrag}``` zitiert werden.

# Code-Highlight
In der Datei ```preamble.tex``` ist definiert, wie genau der Code hervorgehoben werden soll, sofern Bedarf zur Anpassung besteht. 
Das Nutzen der Hervorhebung funktioniert folgendermaßen.
```latex
\begin{lstlisting}[caption=Name des Codeausschnittes, label=lst:someInterfce]
public class SomeClass
{
    public string SomeAttribute;
    ...
}
\end{lstlisting}
```
Der Code kann also einfach eingefügt werden.

# Tabellen
Es gibt viele verschiedene Arten von Tabellen. 

Im Folgenden Beispiel handelt es sich um eine zentrierte Tabelle mit einigen zusammengefügten Zellen.
```latex
\begin{table}[!htb]
	\centering
	\begin{tabular}[!htb]{ | P{2.4cm} | P{2.4cm} | P{2.4cm} | P{2.4cm} | P{2.4cm} | } 
		\hline
		\small
		\renewcommand{\arraystretch}{1.5}
		\setlength{\arrayrulewidth}{0.4mm}
		\cellcolor{Gray0}\textbf{Distanz}\vspace{.75\baselineskip} & \cellcolor{Gray0}\textbf{Lautstärke}\vspace{.75\baselineskip} & \cellcolor{Gray0}\textbf{Nicht}\vspace{.75\baselineskip} & \cellcolor{Gray0}\textbf{Falsch}\vspace{.75\baselineskip} & \cellcolor{Gray0}\textbf{Richtig}\vspace{.75\baselineskip} \\
		\hline
		\multirow{2}*{30cm} & 45 db & 3 & 7 & 25 \\ \cline{2-5}
		& 75 db & 33 & 2 & 0 \\ \hline
		\multirow{2}*{100cm} & 45 db & 29 & 3 & 3 \\ \cline{2-5}
		& 75 db & 35 & 0 & 0 \\ \hline
		\multirow{2}*{200cm} & 45 db & 35 & 0 & 0 \\ \cline{2-5}
		& 75 db & 35 & 0 & 0 \\ \hline
	\end{tabular}
	\caption{Messungen zur Erkennungsgenauigkeit ungefiltert}
	\label{table:messung_ohne}
\end{table}
\FloatBarrier
```

Nachfolgendes Beispiel behandelt besonders lange Tabellen (ggf. über mehrere Seiten).
```latex
\begin{longtable}[h]{ | p{5cm} | P{2cm} | }	\hline
	\label{table:ausschluss}
	\small
	\renewcommand{\arraystretch}{1.5}
	\setlength{\arrayrulewidth}{0.4mm}
	\vspace{.75\baselineskip}
	\cellcolor{Gray0}\textbf{Framework} & \cellcolor{Gray0}\textbf{Eignung} \\
	\hline		
	Dragon							& \cellcolor{Yellow2}Teils \\
	\hline
	Google Speech-to-Text			& \cellcolor{IndianRed1}Nein \\
	\hline
	Microsoft Speech				& \cellcolor{IndianRed1}Nein \\
	\hline
    ...
	\caption{Ausschluss ungeeigneter \gls{framework} anhand definierter Anforderungen}
\end{longtable}
\FloatBarrier
```

Die Definition der Anzahl der Spalten geschieht nach dem ```begin{}```. Auch ihre Breite wird dort bestimmt. Außerdem dem die Formatierung des Inhaltes. ```p{}``` ist standardmäßig linksbündig, ```P{}``` wiederum zentriert.

In der konkreten Definition des Inhaltes der Tabelle wird nach jeder Zeile ```\\``` und ```\hline``` für einen verwendet! Die Abtrennung innerhalb einer Zeile zwischen den Spalten findet über ein ```&``` statt. Die Farbe einer Zelle kann mit Hilfe von ```\cellcolor{}``` vor dem Text bestimmt werden.

Mehrere Zellen könne zeilenweise mit ```\multirow{Anzahl}*{Inhalt}``` kombiniert werden (spaltenweise mit ```\multicolumn```). Unvollständige Linien können mit ```\cline{von-bis}``` gezogen werden. 

Um die Höhe einer Zeile anzupassen kann ```\renewcommand{\arraystretch}{Wert}``` innerhalb der Tabelle ganz oben verwendet werden.

Wichtig ist über ```\caption{}``` die Tabelle zu benennen.

# Bilder
Die Bilder müssen im passenden Ordner ```01_img``` liegen.
Sie werden in Latex standardmäßig folgend eingebunden:
```latex
\begin{figure}[!htb] 
	\centering
	\includegraphics[width=10cm]{HiddenMarkov.png}
	\caption{Hidden-Markov-Modell mit sechs Zuständen \cite{Dylla}}
	\label{fig:HiddenMarkov}
\end{figure}
\FloatBarrier
```

Um zwei Bilder nebeneinander zu erreichen gibt es folgende Option:
```latex
\begin{figure}[!htb] 
	\centering
	\subfloat[\centering Spritzgießmaschine \gls{allrounder}]{{\includegraphics[width=7.8cm]{spritzgiessmaschine.jpg}}}%
	\qquad
	\subfloat[\centering \gls{freeformer}]{{\includegraphics[width=5.5cm]{freeformer.jpg}}}%
	\caption{Produkte der Firma \gls{arburg} \cite{ARBURGGmbH+CoKG_Mediendatenbank}}%
	\label{fig:maschinen}%
\end{figure}
\FloatBarrier
```

# Aufzählungen
Aufzählungen werden mit ```itemize``` ermöglicht.
```latex
\begin{itemize}
	\item Ist eine passende Software verfügbar?
	\subitem Eine Ebene tiefer
    \subitem Eine Ebene tiefer
	\item Gibt es genug Use Cases um eine vollständige Implementierung des Konzeptes zu rechtfertigen?
    \subitem Eine Ebene tiefer
    \subitem Eine Ebene tiefer
\end{itemize}
```

# Inhaltsverzeichnis
Das Inhaltverzeichnis enthält standardmäßig alle Kapitel, Sektionen und Subsektionen. In ```main.tex``` kann außerdem definiert werden, welche Verzeichnisse gelistet werden sollen.

Wird irgendwo (z.B. Anhang) ```\chapter*{}``` verwendet, wird das Kapitel nicht aufgeführt. Das liegt am Stern und gilt auch für Sektionen und Co. Dies wird verwendet, da die Nummerierung des Kapitels somit nicht in Kraft tritt.

Diese kann dann manuell gesetzt werden mit ```\renewcommand{\thesection}{A}``` beispielsweise. Soll es dann im Inhaltsverzeichnis auftauchen, muss folgendes gemacht werden (auch im Abstrakt):

```\addcontentsline{toc}{chapter}{Benamung}```

# Abkürzungen und Glossar
Abkürzungen und Erklärungen werden in ```glossaries.tex``` definiert.
```\newacronym{label}{Abkürzung}{Voller Name}``` für Abkürzungen. 

```\newglossaryentry{label}{name={Voller Name},description={Beschreibung}}``` für Erklärungen (Glossar).


Die Einträge können im Text verwendet werden:

```\gls{Eintrag}``` automatisch. Beim ersten mal das Einführen, danach die Abkürzung.

```\acrlong{Eintrag}``` den vollen Namen.

```\acrshort{Eintrag}``` die Abkürzung.

Alle drei führen durch den Klick zum passenden Verzeichnis (Glossar/Abkürzung).

Um sicherzugehen, dass erst ab der Einleitung auch Begriffe eingeführt werden, sollte bei allen Captions von Bilder, Tabellen und Co. ```\acrlong``` verwendet werden, da sonst teilweise im Verzeichnis eingeführt wird (das ist Latex zu dumm). Gleiches gilt für Abstrakt und Titel.

# Kursiv, Fett, Farbig und sonstiges
```\textit{}``` ist kursiv.

```\textbf{}``` ist fett.

```\underline{}``` ist unterstrichen.

```\\``` ist neue Zeile.

``` "" ``` kann nicht einfach genutzt werden. Stattdessen muss ``` "`"' ``` verwendet werden.

```\colorbox{Farbe}{Inhalt}``` erstellt eine farbige Box um den Inhalt.

```\textbackslash``` um einen Backslash zu erhalten. Dies ist wichtig, da der normale Backslash für Befehl und als Escape-Charakter verwendet wird.

Die Namen aller Farben (xColor) sind auf https://www.sciencetronics.com/greenphotons/wp-content/uploads/2016/10/xcolor_names.pdf zu finden.

# Positionierung
```\newpage``` zum Seitenumbruch.

```\FloatBarrier``` bei Bildern und Tabellen um eine richtige Positionerung zu erhalten.

Zentrierter Text geht folgendermaßen: 
```latex
\begin{nscenter}
Text
\end{nscenter}
```

```\centering``` kann innerhalb von Figuren verwendet werden.

# Fußnoten
Fußnoten sollen nur sehr sparsam eingesetzt werden. Die sind mit ```\footnote{Beschreibung}``` zu verwenden und werden automatisch nummeriert.

# Formeln
```latex
$$ f(x) = \ddfrac{Erkannte W\ddot{o}rter}{Erkennbare W\ddot{o}rter} * 100 $$
```

ü,ö,ä müssen mit ```\ddot{o}``` escaped werden.

```$$``` beginnt und endet eine Formel.

```\underset{Was drunter kommt}{Worunter es kommt}``` erlaubt z.B. etwas unter ein ```=``` zu schreiben.

```\cdot``` ist der Punkt für Multiplikation.

```a^2``` ist a hoch 2.

```\sqrt{2}``` ist Wurzel von 2.

```\ddfrac{Zähler}{Nenner}``` ist Bruch.

```\pm``` ist Plus-Minus.

```\vec{Worüber soll der Pfeil}``` ist ein Vektor.

```\pi, \alpha, \beta, \gamma, \delta, \sigma, ...```

```\frac{\partial \vec{B}}{\partial t}``` als Beispiel für partielle Ableitung.

```\quad``` für Leerzeichen

```\{Inhalt\}``` für geschweifte Klammern

# Abstrakt
Das Abstrakt wird in der Datei ```abstract.tex``` mit Hilfe von ```\begin{abstract}``` verfasst. Soll es nicht im Inhaltsverzeichnis aufgeführt werden, muss die Zeile ```\addcontentsline{toc}{chapter}{Zusammenfassung}``` entfernt werden.

# Eigene Befehle (nützlich)
In Latex können eigene Befehle definiert werden. Das macht Sinn, wenn etwas an mehreren Stellen genutzt wird. Beispielhaft ist dies in ```einstellungen.tex``` zu sehen. 
```tex
\newcommand{\betreuer}{B. Eng., Benedikt Link}
```
sorgt dafür, dass bei Schreiben von ```\betreuer``` immer ```B. Eng., Benedikt Link``` eingefügt wird.

Beispielsweise wird somit dafür gesorgt, dass mit 
```\shortlorem``` bzw. ```\longlorem``` einige Zeilen Lorem Ipsum eingefügt werden (Definition davon in ```preamble.tex```).