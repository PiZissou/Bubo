
global BuboAPI
global BuboCUI
global BuboDockUI
global BuboLinkMax
global PyDocking_is_visible

struct St_Bubo 
(
	/*
	<struct>
		<description> used to call BuboAPI.cs, pyDocking.py from maxscript </description>
		<authors>PLA</authors>
	</struct>
	*/
	_SuspendedMod,
	NetCuiFile,
	LocCuiFile,
	Version,
	Title = "Bubo...",

	fn InitDockableUI = (
		try (
			BuboCUI = dotNetObject  "Bubo.CUI" 
			BuboCUI.Dockable.Content = BuboAPI.UI
			BuboCUI.Dockable.ExecuteOnOpen =  ""
			python.Execute("BuboDock = PyDocking()")
			this.SetTitleWindow  this.title 
			if this.IsVisibleUI() then ( 
				this.CloseUI()
			)
		) catch (
			BuboLogger.LogWarning "_Bubo_.InitDockableUI : % %" val:#(getCurrentException() )
		)
	),

	fn SuspendSelection = (
		local m = modpanel.GetCurrentObject()
		if Selection.Count == 1 and  classof m == skin or classof m == morpher then (
			_SuspendedMod = m
			clearSelection()
		)
	),

	fn ResumeSelection = (
		if   _SuspendedMod != undefined  then (
			modpanel.SetCurrentObject _SuspendedMod
			_SuspendedMod = undefined
		)
	),

	fn LoadConfig configFile = (
		BuboINI.ConfigFile = configFile
		BuboAPI.Refresh()
	),

	fn EditConfig = (
		Edit BuboINI.ConfigFile 
	),

	fn LoadSkinDir skinDir = (
		BuboINI.SkinDir = skinDir
	),

	fn IsVisibleUI = (
		try (
			python.Execute("BuboDock.is_visible()")
			return PyDocking_is_visible 
		)catch(
			BuboLogger.LogWarning "_Bubo_.IsVisibleUI : % %" val:#(getCurrentException() )
			return false
		)
	),

	fn ShowUI = (
		try (
			python.Execute("BuboDock.show()")
			python.Execute("BuboDock.debug_ui()")
		)catch(
			BuboLogger.LogWarning "_Bubo_.ShowUI : % %" val:#(getCurrentException() )
		)
	),

	fn CloseUI = (
		try (
			python.Execute("BuboDock.close()")
		)catch(
			BuboLogger.LogWarning "_Bubo_.CloseUI : % %" val:#(getCurrentException() )
		)
	),

	fn SetTitleWindow  title = (
		try (
			python.Execute("BuboDock.set_title(\"" + title + "\")")
		)catch(
			BuboLogger.LogWarning "_Bubo_.SetTitleWindow : % %" val:#(getCurrentException() )
		)
	),

	fn ToggleUI = (
		if this.IsVisibleUI() then ( 
			this.CloseUI()
		)
		else (
			this.ShowUI()
		)
	),

	fn RegisterSystem = (
 		callbacks.removeScripts  id:#BuboSystem 
		callbacks.addScript #postSystemStartup "Bubo.InitDockableUI()"  id:#BuboSystem 
		callbacks.addScript #preSavingCuiToolbars "Bubo.CloseUI()"  id:#BuboSystem
	),

	fn DataProjection sourceNode  destNodes  IsSkin:true DQ:true RemoveUB:true IsMorph:false Script:false RemoveUT:false  = (
		try(
			local sHandle = gethandlebyanim sourceNode 

			for n in destNodes do (
				local dHandle = gethandlebyanim n 

				if IsMorph then  (
					BuboAPI.ProjectMorph  sHandle  dHandle  Script  RemoveUT
				)
				if IsSkin then (
					BuboAPI.ProjectSkin  sHandle  dHandle  DQ  RemoveUB 
				) 
			)
			select destNodes
			return true 
		) catch(
			BuboLogger.LogError "_Bubo_.DataProjection : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	),

	fn DataProjectionMorph sourceNode destNodes Script:false removeUT:false channelNames:#() createSkwIfNotExists:true = (
		try(
			local sHandle = gethandlebyanim sourceNode 
			for n in destNodes do (
				local dHandle = gethandlebyanim n 
				BuboAPI.ProjectMorph  sHandle  dHandle channelNames Script  removeUT createSkwIfNotExists
			)
			select destNodes
			return true 
		) catch (
			BuboLogger.LogError "_Bubo_.DataProjectionMorph : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	),

	fn SaveMorphChannels  mph morphNode fileName  channelNames:#()  = (
		try(
			return BuboAPI.SaveMorphChannels  ( gethandlebyanim mph ) ( gethandlebyanim morphNode ) fileName  channelNames   
		)catch(
			BuboLogger.LogError "_Bubo_.SaveMorphChannels : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	),
	/*
	channelNames.count == 0 => loadAll, 
	clearChannels == true => delete all channels in mph before loading,  
	loadGeo == true => keep buildedTargetNodes in scene
	*/
	fn LoadMorphChannels  mph morphNode fileName  channelNames:#()  clearChannels:true  loadGeo:false  = (
		try(
			return BuboAPI.LoadMorphChannels  ( gethandlebyanim mph ) ( gethandlebyanim morphNode ) fileName  channelNames   clearChannels  loadGeo 
		)catch(
			BuboLogger.LogError "_Bubo_.LoadMorphChannels : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	),
	/*
	channelNames.count == 0 => loadAll, 
	clearChannels == true => delete all channels in mph before loading,  
	loadGeo == true => keep buildedTargetNodes in scene
	*/
	fn MixMorphChannels  mph morphNode fileNames blendValues channelNames:#()  clearChannels:true  loadGeo:false  = (
		try(
			return BuboAPI.MixMorphChannels  ( gethandlebyanim mph ) ( gethandlebyanim morphNode ) fileNames  blendValues channelNames  clearChannels  loadGeo  
		)catch(
			BuboLogger.LogError "_Bubo_.MixMorphChannels : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	),

	fn MapIndices sNode  dNode = (
		try(
			local indices = BuboAPI.MapIndices  ( gethandlebyanim sNode )  ( gethandlebyanim dNode )   
			return indices
		)catch(
			BuboLogger.LogError "_Bubo_.MapIndices : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return #()
		)
	),

	fn MixSkin sk skinNode filenames blendValues  = (	 
		try(
			BuboAPI.MixSkin ( gethandlebyanim sk )  ( gethandlebyanim skinNode ) 	filenames 	blendValues 
		)catch(
			BuboLogger.LogError "_Bubo_.MixSkin : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	),

	fn GetSkinBones sk = (
		try(
			local nodeHandles = BuboAPI.GetSkinBones ( gethandlebyanim sk ) 
			return for handle in  nodeHandles collect getanimbyhandle handle
		)catch(
			BuboLogger.LogError "_Bubo_.GetSkinBones : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return #()
		)
	),

	fn ReplaceBones sk nodelist:(selection as array) = (
	   	local sourceInfluences = #()
	   	local dest =  nodelist[nodelist.Count]

	   	if modPanel.getCurrentObject() != sk then (
	    	modPanel.setCurrentObject sk
	   	)

		if nodelist.count > 1 then (
	        dest =  nodelist[nodelist.Count]
	        for i = 1 to nodelist.count - 1 do (
	            append sourceInfluences nodelist[i]
	        )
	        skinJob.ReplaceBone sk sourceInfluences dest
	        select  dest
	    )
	),

	fn InMod ev nd = (
		try (
			BuboLinkMax.OnInMod()
		) 
		catch (
			BuboLogger.LogError "_Bubo_.InMod : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	),

	fn NodeDeleted ev nd = (
		try (
			BuboLinkMax.OnNodeTabChanged()
		) 
		catch (
			BuboLogger.LogError "_Bubo_.NodeDeleted : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	),

	fn NodeRenamed ev nd = (
		try (
			BuboLinkMax.OnNodeRenameChanged()
		) 
		catch (
			BuboLogger.LogError "_Bubo_.NodeRenamed : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	),

	fn WhenTransformChanged n  = (
		try (
			--   print ("Event Detected: Event "+(ev as string) + ", Nodes " + (nd as string))
			 deleteAllChangeHandlers id:#BuboNodeTransformChanged 
			
			 when transform n changes id:#BuboNodeTransformChanged do (
				  BuboLinkMax.OnNodeTransformChanged()
			 )
		) 
		catch (
			BuboLogger.LogError "_Bubo_.NodeTransformed : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	)
)
Bubo = St_Bubo()


