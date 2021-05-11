run\makegraph RPB4 rap2 d:\projects\AppsWild\WorkingCopy\RAP\RAP_GUI\RAP\ dCompType > RUN\OUTPUT\rap.gv
dot -Tbmp RUN\OUTPUT\rap.gv -o RUN\OUTPUT\rap.bmp

run\makegraph RPB4 utilities d:\projects\AppsLibs\WorkingDirectory\Libs\Utilities\ No > RUN\OUTPUT\utilities.gv
dot -Tbmp RUN\OUTPUT\utilities.gv -o RUN\OUTPUT\utilities.bmp

Use of the bunch tool.
This creates and MDG file. Just parent child.
run\makegraph RPB4\SQLDEV longshort2 d:\projects\AppsWild\WorkingCopy\MRPTool\MRPTool_GUI\LS\ vComponent MDG > RUN\OUTPUT\ls.mdg
run\makegraph RPB4\SQLDEV longshort2 NO vComponent MDG > RUN\OUTPUT\ls.mdg
run\makegraph RPB4 rap2 NO vComponent MDG > RUN\OUTPUT\rap.mdg

run\makegraph RPB4\SQLDEV batchcalc d:\projects\AppsWild\WorkingCopy\BCNet\BCNet_GUI\BCNet\ vSumbac NO > RUN\OUTPUT\bcnet.gv
dot -Tbmp RUN\OUTPUT\bcnet.gv -o RUN\OUTPUT\bcnet.bmp

run\makegraph RPB4 TPIData d:\projects\AppsWild\WorkingCopy\TPI\TPI_GUI\TPINet\ NO NO > RUN\OUTPUT\tpinet.gv
dot -Tbmp RUN\OUTPUT\tpinet.gv -o RUN\OUTPUT\tpinet.bmp

run\makegraph RPB4 TPIData d:\projects\AppsWild\WorkingCopy\TPI\TPITrack\TPITrack\ NO NO > RUN\OUTPUT\tpitrack.gv
dot -Tbmp RUN\OUTPUT\tpitrack.gv -o RUN\OUTPUT\tpitrack.bmp

run\makegraph RPB4 ERPInterface d:\projects\AppsWild\WorkingCopy\LOP\LOP_GUI\LOPNet\ NO NO > RUN\OUTPUT\lopnet.gv
dot -Tbmp RUN\OUTPUT\lopnet.gv -o RUN\OUTPUT\lopnet.bmp

run\makegraph RPB5\SQLDEV2012 gis2 c:\projects\spits\GIS\gis-gui\GIS\ NO > RUN\OUTPUT\gis2.gv
c:\Projects\GraphViz\bin\dot -Tbmp RUN\OUTPUT\gis2.gv -o RUN\OUTPUT\gis2.bmp

c:\Projects\GraphViz\bin\dot -Tbmp RUN\OUTPUT\AISDataSet.txt -o RUN\OUTPUT\AISDataSet.bmp
c:\Projects\GraphViz\bin\dot -Tbmp RUN\OUTPUT\ActivityDataSet.txt -o RUN\OUTPUT\ActivityDataSet.bmp
c:\Projects\GraphViz\bin\dot -Tbmp RUN\OUTPUT\StockDataSet.txt -o RUN\OUTPUT\StockDataSet.bmp
c:\Projects\GraphViz\bin\dot -Tbmp RUN\OUTPUT\MedicationDataSet.txt -o RUN\OUTPUT\MedicationDataSet.bmp



//Then copy ls.mdg to d:\projects\bunch-3.4.
//Set the java classpath. 
set CLASSPATH=d:\\projects\\bunch-3.4\\bunch.jar;%CLASSPATH%
//Open the program 
java bunch.Bunch
//Load the ls.mdg file. It create ls.mdg.dot

//then copy this partitioned dot file to makegraph directory. 
//Remove 
size= "10,10";
rotate = 90;

And create bmp file.
dot -Tbmp ls.mdg.dot -o ls.bmp


Existing path is
.;C:\Program Files (x86)\Java\jre6\lib\ext\QTJava.zip;d:\Program Files (x86)\IBM\solidDB\solidDB6.5\jdbc\SolidDriver2.0.jar


run\makegraph RPB4 utilities d:\projects\AppsLibs\WorkingDirectory\Libs\Utilities\ nothing MDG > RUN\OUTPUT\utility.mdg
dot -Tbmp RUN\OUTPUT\utility.mdg.dot -o RUN\OUTPUT\utility.bmp


run\makegraph RPB5\SQLDEV2012 rap2 c:\Projects\AppsWild\WorkingCopy\RAP\RAP_GUI\RAP\ NO > RUN\OUTPUT\rap2.gv
c:\Projects\GraphViz\bin\dot -Tbmp RUN\OUTPUT\rap2.gv -o RUN\OUTPUT\rap2.bmp


c:\Projects\GraphViz\bin\sfdp.exe -o c:\Projects\GenUI\RUN\OUTPUT\rap2out.gv c:\Projects\GenUI\RUN\OUTPUT\rap2.gv

c:\Projects\GraphViz\bin\smyrna.exe c:\Projects\GenUI\RUN\OUTPUT\rap2out.gv 
c:\Projects\GraphViz\bin\smyrna.exe c:\Projects\GenUI\RUN\OUTPUT\gis2out.gv 

run\makegraph RPB5\SQLDEV2012 rap2 c:\Projects\AppsWild\WorkingCopy\RAP\RAP_GUI\RAP\ NO JS > RUN\OUTPUT\rap2.js
run\makegraph RPB5\SQLDEV2012 gis2 c:\projects\spits\GIS\gis-gui\GIS\ NO JS > WebA1\GraphData.js
run\makegraph RPB5\SQLDEV2012 rap2 c:\Projects\AppsLibs\libbaines\ NO JS > RUN\OUTPUT\utilities.js
run\makegraph BAINESLENOVO SVP2 c:\projects\SVP\GUI\SVP\ NO JS > WebA1\GraphData.js
run\makegraph BAINESLENOVO CAT c:\projects\cocoa\GUI\CAT\ NO JS > WebA1\GraphData.js
