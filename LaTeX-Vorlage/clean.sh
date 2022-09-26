#!/bin/sh

# Variables
DOKUMENT_NAME='T2000_Spracherkennung.pdf'
PDF_DEST_PATH='C:\Users\entep04\Desktop\T2000\'

# Temp Files löschen, da diese nicht mehr gebraucht werden
rm *.aux
rm *.acn
rm *.acr
rm *.alg
rm *.bbl
rm *.bcf
rm *.blg
rm *.glg
rm *.glo
rm *.gls
rm *.ist
rm *.lof
rm *.log
rm *.lol
rm *.lot
rm *.lop
rm *.run.xml
rm "*.synctex(busy)"
rm *.synctex.gz
rm *.tdo
rm *.toc
find . -name \*.aux -type f -delete

# kopiere PDF und benenne sinnvoll
cp main.pdf $PDF_DEST_PATH
mv $PDF_DEST_PATH/main.pdf $PDF_DEST_PATH/$DOKUMENT_NAME

# Git add, nur sofern eingerichtet
# cd ../
# git add .
# git commit -m "tex auto commit"
# git pull origin
# git push