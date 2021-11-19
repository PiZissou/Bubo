# Bubo

Copyright (c) 1998-2018 TATProductions.

DESCRIPTION: Tool for rigging department. skin, morph/blendshape, transfert mesh datas.
AUTHOR: Pierre Lasbignes

# API 

.\Bubo\API\API.cs
.\Bubo\API\APIMorph.cs
.\Bubo\API\APISkin.cs

This Api can be used in 3dsmax with maxscript scripting language.

## Dotnet Objects
	(PolySym)  	BuboApi.PolySym  -- access to PolySym object , detect symmetry points of a mesh to perform copy, paste and invers position points. 

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

