/*
<grimoire>
	<description>	
		Koro Loading 
		Tools used for Skin, morph and mapChannel, accessible via C# UI and MaxScript API 
	</description>
	<authors>PLA</authors>
	<categories>Setup</categories>
	<tags>Skin, Morph, MapChannel</tags>
</grimoire>
*/

BuboLogger = TatLoggerModule()
BuboLogger.setModuleName "Bubo"

try(
	Bubo.CloseUI()
)catch()
try(
	BuboAPI.Dispose()
)catch()

global Bubo 
global BuboCUI
global BuboINI 
global BuboAPI 
global BuboLinkMax 
global BuboMain

version = substring (GetFileVersion (getDir #maxRoot + "\\3dsmax.exe")) 1 2
thisPath = getFilenamePath(getThisScriptFilename())

if thisPath == @"G:\Pierre\Bubo\Mxs\"  then (
	thisPath = @"\\koro\pub\Bubo\0.1.4\"
)

dllBuboFile = ""
dllCuiFile = ""
dllLocalCuiFile = ""

if version == "22" then (
	assembliesPath = @"C:\Program Files\Autodesk\3ds Max 2020\bin\assemblies\"
	dllBuboFile = thisPath + "Bubo_2020.dll"
	dllCuiFile = thisPath + "BuboCui.dll"
	dllLocalCuiFile = assembliesPath + "BuboCui.dll"
) else if version == "23" then (
	assembliesPath = @"C:\Program Files\Autodesk\3ds Max 2021\bin\assemblies\"
    dllBuboFile = thisPath + "Bubo_2021.dll"
	dllCuiFile = thisPath + "BuboCui.dll"
	dllLocalCuiFile = assembliesPath + "BuboCui.dll"
) else if version == "24" then (
	assembliesPath = @"C:\Program Files\Autodesk\3ds Max 2022\bin\assemblies\"
    dllBuboFile = thisPath + "Bubo_2022.dll"
	dllCuiFile = thisPath + "BuboCui.dll"
	dllLocalCuiFile = assembliesPath + "BuboCui.dll" 
)

ioFile = dotnetClass "System.IO.File"
reflectionAssembly = dotnetClass "System.Reflection.assembly"
activator = dotNetClass "System.Activator"
BuboAssembly = reflectionAssembly.Load (ioFile.ReadAllBytes (dllBuboFile))

if BuboAssembly != undefined then (
	BuboINI = BuboAssembly.CreateInstance  "Bubo.INI"
	BuboAPI = BuboAssembly.CreateInstance  "Bubo.API"
	BuboLinkMax = BuboAssembly.CreateInstance  "Bubo.LinkMax"
	BuboMain = BuboAssembly.CreateInstance  "Bubo.Main"
)

copyFile dllCuiFile  dllLocalCuiFile

fileIn "PolyJob.ms"
fileIn "McInfo.ms"
fileIn "NodeMods.ms"
fileIn "MorphJob.ms"
fileIn "SkinJob.ms"
fileIn "ProjectionJob.ms"
fileIn "BuboMacros.ms"
fileIn "__Bubo__.ms"

pyPath = thisPath + @"\BuboDock.py"
python.ExecuteFile pyPath

BuboINI.DisplayLog = false

Bubo = St_Bubo()
Bubo.Version = filterString thisPath @"\"
Bubo.Version = Bubo.Version[ Bubo.Version.count ]
Bubo.Title =  "Bubo " + Bubo.Version  

Bubo.RegisterSystem()




