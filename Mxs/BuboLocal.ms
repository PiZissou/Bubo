/*
<grimoire>
	<description>	
		Local Loading
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
assembliesPath = @"C:\Program Files\Autodesk\3ds Max 2021\bin\assemblies\"
dllBuboFile = ""
dllCuiFile = ""
dllLocalCuiFile = ""
configFile = ""
skinDir    = ""

buboFolder = case toLower sysinfo.userName of (
	"pla": @"\\dev\WORKPLACE\Pierre\Bubo\" 
	"vgu": @"\\dev\WORKPLACE\Vivian\Bubo\"
	"Pierre": @"D:\Utilisateurs\Pierre\_Workplace\dev_TAT\Bubo\" 
)

if version == "22" then (
	dllBuboFile = buboFolder + @"Bubo\bin\Bubo_2020.dll"
	dllCuiFile = buboFolder + @"Bubo\bin\BuboCui.dll"
	dllLocalCuiFile = assembliesPath + "BuboCui.dll"
	configFile = @"P:\XX_LIB\_Bubo\" + "config.xml"
	skinDir = (GetDir #import) + @"\Skin\"
) else if version == "23" then (
    dllBuboFile = buboFolder + @"Bubo\bin\Bubo_2021.dll"
    dllCuiFile = buboFolder + @"Bubo\bin\BuboCui.dll"
    dllLocalCuiFile = assembliesPath + "BuboCui.dll"
    configFile = @"R:\XX_LIB\_Bubo\" + "config.xml" 
	skinDir = (GetDir #import) + @"\Skin\" 
) else if version == "24" then (
    dllBuboFile = buboFolder + @"Bubo\bin\Bubo_2022.dll"
    dllCuiFile = buboFolder + @"Bubo\bin\BuboCui.dll"
    dllLocalCuiFile = assembliesPath + "BuboCui.dll"
    configFile = @"N:\XX_LIB\_Bubo\" + "config.xml" 
	skinDir = (GetDir #import) + @"\Skin\" 
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

pyPath = buboFolder + @"Mxs\BuboDock.py"
python.ExecuteFile pyPath
python.Execute("BuboDock = PyDocking()")

--configFile =  @"D:\Test_3dsmax\config.xml"
BuboINI.DisplayLog = true

Bubo = St_Bubo()
Bubo.LoadConfig configFile
Bubo.LoadSkinDir skinDir 
Bubo.RegisterSystem()

Bubo.InitDockableUI()
Bubo.showUI() 

Bubo.SetTitleWindow ( "Bubo " + "__dev__" ) 

/*
filename1 = @"G:\Pierre\xx_tests\saveChannels.xml" 
bubo.SaveMorphChannels  $P_LambdaHumain_HD.modifiers[#morpher1]  $P_LambdaHumain_HD filename1 
bubo.LoadMorphChannels $P_LambdaHumain_HD.modifiers[#morpherMix] $P_LambdaHumain_HD   filename1

filename2 = @"G:\Pierre\xx_tests\saveChannels2.xml" 
bubo.SaveMorphChannels  $P_LambdaHumain_HD.modifiers[#morpher2]  $P_LambdaHumain_HD filename2 
bubo.LoadMorphChannels $P_LambdaHumain_HD.modifiers[#morpherMix] $P_LambdaHumain_HD   filename2

bubo.MixMorphChannels $P_LambdaHumain_HD.modifiers[#morpherMix] $P_LambdaHumain_HD   #( filename1, filename2 )  #(0.5 , 0.5)  

--------- bug mix Morph ----------
loadMaxFile "R:\XX_TESTS\Dev\Pierre\Bubo\Morphers\P_Apollon.setup.[CheckShell].v120.aen_source.max"

filenames = #(@"\\lm4\ARGO\XX_LIB\_TatPresets\Argonautes\Humain\Aphrodite\Morphers\_Corps_HD[Morpher]_others.xml")
--filenames = #(@"R:\XX_TESTS\Dev\Pierre\Bubo\Morphers\_Corps_HD[Morpher]_others.xml")
presetPercents = #(1)
channelNames = #("D_CheekBone_Up", "G_CheekBone_Up", "D_Cheek_FB", "G_Cheek_FB", "D_EyeBrows_In_D", "G_EyeBrows_In_D", "D_EyeBrows_In_LR_Inner", "G_EyeBrows_In_LR_Inner", "D_EyeBrows_In_U", "G_EyeBrows_In_U", "D_EyeBrows_Mid_D", "G_EyeBrows_Mid_D", "D_EyeBrows_Mid_U", "G_EyeBrows_Mid_U", "D_EyeBrows_Out_D", "G_EyeBrows_Out_D", "D_EyeBrows_Out_U", "G_EyeBrows_Out_U", "D_Eyebrow_In_OpCB", "G_Eyebrow_In_OpCB","D_Eyebrow_In_Strech", "G_Eyebrow_In_Strech", "D_Eyebrow_Out_LR_Inner", "G_Eyebrow_Out_LR_Inner", "D_MouthCorner_DO", "G_MouthCorner_DO", "D_MouthCorner_Down", "G_MouthCorner_Down", "D_MouthCorner_In", "G_MouthCorner_In", "D_MouthCorner_Out", "G_MouthCorner_Out", "D_MouthCorner_UO", "G_MouthCorner_UO", "D_MouthCorner_Up", "G_MouthCorner_Up", "Mouth_Down", "Swelling_Lower", "Swelling_Upper", "Mouth_Jaw_Open", "D_Upper_Eyelid_Open", "D_Lower_Eyelid_Open", "G_Upper_Eyelid_Open", "G_Lower_Eyelid_Open")
n = $P_Apollon_Corps_HD
mph = n.modifiers[#Morpher]
BuboAPI.MixMorphChannels  ( gethandlebyanim mph ) ( gethandlebyanim n ) fileNames  presetPercents channelNames  false false 

----Test DataProjection API----
loadMaxFile @"R:\XX_TESTS\Dev\Pierre\PathieTestDataProjection.v004.mch.max"

bubo.DataProjection $P_Pathie_Corps_HD  #($P_Pathie_Corps_MD ) IsSkin:true DQ:false RemoveUB:true IsMorph:true Script:false RemoveUT:false 
bubo.DataProjection $P_Pathie_Corps_HD  #($P_Pathie_Corps_MD ) IsSkin:false DQ:false RemoveUB:true IsMorph:true Script:false RemoveUT:false 

sk = $.modifiers[5]
addModifierWithLocalData  $ sk $ sk before:3

sk = $.modifiers[5]
addModifierWithLocalData  $ sk $ sk before:3

skinJob.WrappedSkin  $P_Pathie_Corps_HD.modifiers[5]  $P_Pathie_Corps_HD  $P_Pathie_Corps_MD 
morphJob.WrappedMorph   $P_Pathie_Corps_HD.modifiers[6]   $P_Pathie_Corps_HD   $P_Pathie_Corps_MD  (#{1..100} as array) "ok google" 

----Test DataProjection API----
R:\XX_TESTS_CreateMD\ms\createMD_v05.ms

loadMaxFile @"R:\05_MOD\P_RatA\03_setup\P_RatA.setup.v001.jml.max"

Bubo.DataProjectionMorph  $P_Pathie_Corps_HD  #($P_Pathie_Corps_MD ) Script:true removeUT:false channelNames:#("Head_Back", "P_Pathie_Corps_HD001") createSkwIfNotExists:true




/*

facialSkin
headSkin = ($*_Head_SKIN)[1]
if IsValidNode headSkin then (
    m = $P_Aphrodite_ANIM.modifiers[#selset_setup]
    rigDeformFacial = m.getSelectionSetNodes "RigDeformFacial"
    facialSkin = for o in rigDeformFacial where  o != headSkin and  matchPattern o.name pattern:"*_SKIN" collect o
    if facialSkin.count > 0 then (
        nodeList = $*_HD as array
        for n in nodeList do (
        	n = $P_Aphrodite_Corps_HD 
            local sk = n.modifiers[#Skin]
            if sk != undefined then (
            	SkinJob.ReplaceBone  sk  facialSkin  headSkin  selectBone:false   onSelectedVertex:false               
            )
        )  
    )                               
)
else (
    throw ("Can't find Head_SKIN")
)

*/