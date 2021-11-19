# Bubo

Copyright (c) TATProductions.

DESCRIPTION: Tool for rigging department. skin, morph/blendshape, transfert mesh datas.<br />
AUTHOR: Pierre Lasbignes<br />
DATE: 2019<br />


# INSTALL & LOADING
	copy Bubo to [yourCustomPath]
	the commande "fileIn	[yourCustomPath]\Bubo\Mxs\Bubo.ms"  will execute the file. This is the loader of the project
	/!\ Warning, this project needs externals TAT libraries to work Properly 
	
# API

This Api can be used inside 3dsmax with maxscript scripting language.

## Files Path 
	.\Bubo\API\API.cs
	.\Bubo\API\APIMorph.cs
	.\Bubo\API\APISkin.cs

## Global
	(void)		Bubo.ShowUI() 
	(void)		Bubo.CloseUI()
	(void)		BuboApi.Refresh() 
	(void)		BuboApi.Dispose() -- disable all events
	(void)		BuboApi.UnDispose() -- enable all events
	(void)		BuboApi.ResetConfig() -- reset xml config of treeview groups
	(bool)  	BuboApi.ProjectMorph (int) sHandle (int) dHandle (string[]) channelNames (bool) isScript (bool) isUnusedTargets (bool) createSkwIfNotExists
	(bool)  	BuboApi.ProjectSkin (int) sHandle (int) dHandle (bool) isDualQuat (bool) isUnusedBones
	(int[]) 	BuboApi.MapIndices (int) sHandle (int) dHandle
	(string) 	BuboApi.GetBasename (string) s
	(string) 	BuboApi.ReplaceBasename (string) s (string) replaceS

## Skin methods

![alt text](Capture_Skin.PNG)

	(float) 	BuboApi.CurrentSkinWeight -- access property of the UI
	(SkinMod) 	BuboApi.GetSkinMod (int) modifierHandle  (int) nodeHandle
	(bool)		BuboApi.SaveSkin (int) modifierHandle (int) nodeHandle  (string) filename
	(int[])     	BuboApi.GetSkinBones (int) modifierHandle
	(bool)		BuboApi.MixSkin  (int) modifierHandle  (int)  nodeHandle  (string[])  filenames   (float[])  mix
	(bool)  	BuboApi.LoadSkin (int) modifierHandle  (int)  nodeHandle  (string)  fileName
	(bool)		BuboApi.LoadSkin (int) modifierHandle  (int)  nodeHandle  (string)  fileName  (bool) onlySelected
	(bool)		BuboApi.LoadSkin (int) modifierHandle  (int)  nodeHandle  (string)  fileName  (int[]) verticesToSkin
	(void) 		BuBoApi.SetSkinWeight (int) modifierHandle (int) nodeHandle  (float) val (bool) addValue 
	(void)		BuboApi.SkinWeightPlus()
	(void)		BuboApi.SkinWeightMinus()
	(void)      	BuboApi.HoldBoneToggle()
	(void)      	BuboApi.SkinGrow()
	(void)      	BuboApi.SkinShrink()
	(void)      	BuboApi.SkinLoop()
	(void)      	BuboApi.SkinRing()
	(void)      	BuboApi.SkinNextBone()
	

## Morph methods

![alt text](Capture_Morph.PNG)
	(PolySym)  	BuboApi.PolySym  -- access to PolySym object , detect symmetry points of a mesh to perform copy, paste and invers position points. 
	(bool) 		BuboApi.SaveMorphChannels (int) modifierHandle (int) nodeHandle (string) filename  (string[]) channelNames
	(bool) 		BuboApi.LoadMorphChannels (int) modifierHandle (int) nodeHandle (string) filename  (string[]) channelNames  (bool) clearChannels  (bool) keepTargetNodes
	(bool) 		BuboApi.MixMorphChannels (int) modifierHandle (int) nodeHandle (string[]) filenames  (float[]) mixValues  (string[])  channelNames  (bool)  clearChannels  (bool) keepTargetNodes
