
global TatMxsModifiersLib
global Controls

struct St_MorphJob (
	/*
    <struct>
        <description> 
        	Extends Morpher functions  
        	used in MorphEngine.cs, MorphMod.cs and BuboAPI
    	</description>
		<authors>PLA</authors>
    </struct>
    */
	_modSatuts = #(),
	_channelStatus = #(),

	fn MirrorTargetsVtx targets side = (
		for n in targets do(
			format "Mirror TargetsVtx %\n" n.name
			PolyJob.MirrorVtx n  side 
		)
	),

	fn SetController mph c indices = (

		for i in indices do (
			mph[i].Controller = c
		)
	),

    fn ShowController mph indices = (
		try (	
	 		for i in   indices where  mph[i].controller  != undefined  do(
	            local  morphName = WM3_MC_GetName  mph  i
	            local c = controls.GetFloatController mph[i].controller
	            if classof  c == Float_Multi then (
	                 c.trackName = morphName
	            )
	            displayControlDialog   c   morphName
	        )	
	    )
	    catch (
	    	BuboLogger.LogError "MorphJob.ShowController : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    )
    ),

  	fn SelectTargets mph indices = (
  		try (	
	        local targets = #()
	        for i in indices do (
	            local info = this.GetMorphInfo  mph i
	            join targets info._targetTab
	        )
	        if targets.count  > 0 then (
	            select targets
	        )
	    )
	    catch (
	    	BuboLogger.LogError "MorphJob.SelectTargets : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    )
    ),

    fn HideTargets mph  indices  val = (
		try (	
	        local targets=#()
	        for i in  indices do (
	            local info = this.GetMorphInfo  mph  i
	            join targets info._targetTab
	        )
	        for target in   targets do (
	            target.ishidden = val
	        )
	    )
	    catch (
	    	BuboLogger.LogError "MorphJob.HideTargets : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    )
    ),

	fn ExtractBaseTarget  mphNode  layerName  = (

	    local orig = copy mphNode
	    TatMxsModifiersLib.deleteAll  orig
	    this.AddToLayer layerName #(orig)
	    orig
	),

	fn IsEditableChannel mph index = (
		with animate off (
			local testingRandomVal = 51.2739
			local val = mph[index].value
			mph[index].value = testingRandomVal
			local valueAsChanged = if mph[index].value == testingRandomVal  then true else false
			mph[index].value = val
			return valueAsChanged	
		)
	),

	fn SetChannelpos mph index = (
	
	 	local modulo = (mod  index  10 ) as integer
    	local pos = (index - modulo)
    	WM3_SetChannelpos mph   pos
	),

	fn SetChannelSel mph index = (
		local modulo = ( mod  index  10 ) as integer
        index = if index < 10 then 0 else 1
        WM3_SetChannelSel  mph  ( modulo +  index )
	),

	fn SetValue mph index val = 
	(
		WM3_MC_SetValue mph index val
	),
	fn SetActive mph index onOff = 
	(
		WM3_MC_SetActive mph index onOff
	),

	fn AddChannels  mph   nodeList = (
		try (	
	 		max modify mode
	 		SuspendEditing()

		    for n in nodeList  do (
	            local index = this.GetFreeChannel   mph
	            this.BuildFromNode mph index n  
		    )
		    resumeEditing()
		    modpanel.setCurrentobject mph 
	    )
	    catch (
	    	BuboLogger.LogError "MorphJob.AddChannels : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    	resumeEditing()
	    )
	),

	fn GetMorphPoint mph index pointId = (

		local pos = WM3_MC_GetMorphPoint mph index pointId
		return #(pos.x,pos.y,pos.z)
	),

	fn BuildFromNode mph index n  setDefault:true  = (
		try (
			if  index != undefined   do (
				with animate off (
		            WM3_MC_BuildFromNode   mph   index   n
		            if setDefault then (
			            WM3_MC_setvalue   mph   index  0.0
			            WM3_MC_setActive mph   index  true
			            WM3_MC_setUseLimits mph   index  true
			            WM3_MC_SetLimitMIN  mph   index  0.0
			            WM3_MC_SetLimitMAX  mph   index  100.0
		            )
	          	)
	          	return true
	        )
	        return false
       	)
       	catch(
	    	BuboLogger.LogError "MorphJob.BuildFromNode : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    	return false
       	)
	),

	fn DeleteChannel  mph  indices = (
		try (
	    	for index in  indices do (
	    		WM3_MC_Delete   mph   index   
			)
    	)
    	catch (
    		print ( "DeleteChannel" +  getCurrentException() )
    	)
	),

	fn GetChannelByName mph ListName =(

	    local listIndex = #()

	    for i = 1 to ListName.count  do (
	        local patternName = ListName[i]
	        local success = false

	        for j = 1 to mph.numSubs where  ( WM3_MC_GetName   mph   j ) == patternName do (
	            append  listIndex j
	            success = true
	        )
	        if not success then (
	            append  listIndex undefined
	        )
	    )
	    listIndex
	),

	fn GetChannelByTarget mph target = (
		for i=1 to mph.numSubs do (
			if WM3_MC_GetTarget mph i == target then (
				return i
			)
		)
		return undefined 
	),


	fn GetFreeChannel  mph  = (
	    local index
	    for i = 1  to mph.numSubs do (
	        if not  WM3_MC_HasData  mph  i then (
	            index = i
	            exit
	        )
	    )
	    if index == undefined then (
	        index =  mph.numSubs + 1
	    )
	    index
	),

	fn GetValidChannels mph = (
		try (
	    	return for i = 1 to mph.numSubs where  WM3_MC_HasData  mph i collect i
    	)
    	catch (
    		BuboLogger.LogError "MorphJob.GetValidChannels : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
    	)
	),

	fn GetChannelNames mph = (
		try (
	    	return for i = 1 to mph.numSubs  where (WM3_MC_HasData  mph  i) collect (WM3_MC_GetName  mph  i)
    	)
    	catch (
    		BuboLogger.LogError "MorphJob.GetChannelNames : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
    	)
	),

	fn FindChannelIndex mph channelName freeChannel = (
		try (
		    local tmp = for i = 1 to mph.numSubs  where  toLower (WM3_MC_GetName  mph  i) == toLower channelName Collect i
		    local index = tmp[1]
		    index = if index == undefined then -1 else index
		    if index == -1 and freeChannel then (
		    	return this.GetFreeChannel mph 
			)
			else (
				return index 
			)
		)
    	catch (
    		BuboLogger.LogError "MorphJob.FindChannelIndex : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
    		return -1 
    	)
	),

	fn GetMorphInfo  mph  index  = (

	    local info = ST_McInfo()
	    info._index = index

	    if  index  > 0 and index <= mph.numSubs and  (  WM3_MC_HasData  mph   index  ) then (

	        info._morpherMod = mph
	        info._controller = mph[index].controller
	        info._progCount =  WM3_NumberOfProgressiveMorphs  mph  index
	        info._tension  = WM3_GetProgressiveMorphTension   mph    index
	        info._name = WM3_MC_GetName   mph   index
	        info._UseLimits = WM3_MC_GetUseLimits mph  index
	        info._LimitMIN = WM3_MC_GetLimitMIN mph  index
	        info._LimitMAX = WM3_MC_GetLimitMAX mph  index
	        info._progCount = if info._progCount > 3 then 3 else info._progCount

	        local weightTab = #(100.0)

	        if info._progCount  == 2 then
	            weightTab = #(50.0, 100.0)
	        else if info._progCount  == 3 then
	            weightTab = #( 33.0 , 66.0 , 100.0)
	        else if info._progCount  == 4 then
	            weightTab = #( 25.0 , 50.0 , 75.0 ,100.0)
	        /*
	        Get the progressive weiht if prog_morph_node exist
	        else default weight
	        */

	        for k = 1  to  info._progCount   do (
	            target = WM3_GetProgressiveMorphNode   mph  index   k

	            if target != undefined   then (
	                info._IsTargetExist = true
	                local kstr = if k > 1 then ( k as string ) else ""
	                target.name =  info._name + kstr
	                append   info._weightTab   (  WM3_GetProgressiveMorphWeight   mph    index   target )
	                append   info._targetTab  target
	                append   info._targetTMTab   target.transform
	            )
	            else (
	                append   info._weightTab  weightTab[k]
	            )
	        )
	        if  info._progCount == 1 then(
	            info._weightTab = #(100.0)
	        )
	    )
	    else (
	        info = undefined
	    )
	    info
	),

	fn DisableMods mph mphNode  = (
	    _modSatuts = #()
	    for i = 1 to  mphNode.modifiers.count  do
	    (
	        append _modSatuts   mphNode.modifiers[i].enabled
	        mphNode.modifiers[i].enabled = false
	    )
	    mph.enabled = true
	),

	fn EnableMods  mphNode = (
	    for i = 1 to  mphNode.modifiers.count   do(
	        mphNode.modifiers[i].enabled = _modSatuts[i]
	    )
	    true
	),

	fn DisableChannels  mph  excludesID = (
	    _channelStatus = #()
	    for i = 1 to mph.numsubs do(

	        append  _channelStatus ( WM3_MC_IsActive   mph  i )
	        WM3_MC_SetActive   mph  i   false
	    )
	    if excludesID != undefined then (
		    for i in excludesID do (
		    	WM3_MC_SetActive   mph  i  _channelStatus[i]
		    )
	    )
	),

	fn EnableChannels  mph = (
	    for i = 1 to mph.numsubs do(
	        WM3_MC_SetActive   mph  i   _channelStatus[i]
	    )
	),

	fn BuildInfo  mph  info  setActive  isAssignController:true  = ( 
		try (
		    if classof info == ST_McInfo  then (
		        if WM3_MC_HasData   mph   info._index  then ( 
		        	for  i = 1 to info._progCount   do (
		            	WM3_DeleteProgressiveMorphNode  mph   info._index  1
		            )	
		        )
		        if  info._IsTargetExist  then  (
		            for  i = 1 to info._progCount  where isValidNode info._targetTab[i] do(
		                if i == 1  then (
		                    local success = WM3_MC_BuildFromNode   mph   info._index   info._targetTab[i]
		                )
		                else (
		                    WM3_AddProgressiveMorphNode   mph     info._index    info._targetTab[i]
		                )
		                WM3_SetProgressiveMorphTension   mph    info._index    info._tension
		            )
		            for i = 1  to info._progCount  do (
	                	WM3_SetProgressiveMorphWeight  mph  info._index   info._targetTab[i]   info._weightTab[i]
		            )
		            WM3_MC_setUseLimits mph     info._index   info._UseLimits
		            WM3_MC_SetLimitMIN  mph     info._index   info._LimitMIN
		            WM3_MC_SetLimitMAX  mph     info._index   info._LimitMAX
		            WM3_MC_SetActive  mph  info._index    setActive

		            if info._controller != undefined and isAssignController then (
		                mph [info._index ].controller = info._controller
		            )
		            if  mph [info._index ].controller  == undefined then (
	            		mph [info._index ].controller  = bezier_float()
		            )
		        )
		    )
	    ) 
	    catch (
	    	BuboLogger.LogError "MorphJob.BuildInfo : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    )
	),

	fn Compact mph = (

	    if classof mph != morpher do
	        return false

	    for i = 1 to mph.numsubs where  WM3_MC_HasData  mph   i   do(
	        for j = 1 to mph.numsubs where   not  WM3_MC_HasData  mph   j  and  j < i  do(

	            WM3_MoveMorph mph  i  j
	            exit
	        )
	    )
	    true
	),

	fn CopyLimits mph destMph = (
	    destMph.Use_Limits  	= mph.Use_Limits
	    destMph.Spinner_Minimum = mph.Spinner_Minimum
	    destMph.Spinner_Maximum = mph.Spinner_Maximum
	),

	fn Clone mph  = (
		local destMph = copy mph
		destMph.name = mph.name
		destMph.Autoload_of_targets = mph.Autoload_of_targets
		this.CopyLimits mph destMph
	    return destMph
	),

	fn SnapshotTargets  mph  snapShotNode  info  ishidden layerName  = (
		try(
			info._targetTab = #()
			local success = false
			local index 	= info._index
			local progCount = info._progCount
			local weightTab = info._weightTab
			local tmTab 	= info._targetTMTab
			local morphName = info._name

	        mph[index].controller = Bezier_float()
	        WM3_MC_SetActive   mph  index   true

			for i = 1  to  progCount  do (
	            WM3_MC_setvalue  mph   index   weightTab[ i ]
	            local target
	            if classof snapShotNode.baseobject  == splineShape then (
	                target = copy   snapShotNode
	                maxOps.CollapseNodeTo   target   1   off
	            ) 
	            else (
	                target = snapshot  snapShotNode
	                if classof snapShotNode.baseObject == editable_poly then (
	                    converttopoly  target
	                )
	            )
	            local incr = if i > 1 then ( i as string ) else ""
	            target.name =  morphName + incr

	            if tmTab[i] != undefined then (
	                target.transform = tmTab[i]
	            )
	            target.isHidden = ishidden
	            append   info._targetTab  target
	            success = true
	        )
	        try (
	            mph[index].controller = info._controller
	        ) catch ()

	        WM3_MC_SetActive  mph  index   false
	        if success then (

	        	info._IsTargetExist = true
	            info._TargetRebuilded = true
                this.AddToLayer layerName  info._targetTab 
	        )
        )
        catch (
        	BuboLogger.LogError "MorphJob.SnapshotTargets : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
        )
	),

	fn BeginSafeExtract mph mphNode excludesID = (
		try (
			this.DisableMods mph mphNode
			this.DisableChannels mph excludesID
		)
		catch (
			BuboLogger.LogError "MorphJob.BeginSafeExtract : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return #()
		)
	),
	fn EndSafeExtract mph mphNode = (
		try (
			this.EnableMods mphNode
			this.EnableChannels mph
		)
		catch (
			BuboLogger.LogError "MorphJob.EndSafeExtract : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return #()
		)
	),

	fn SelfExtract  mph  mphNode  indices  layerName  = (
		try (
			local infos = #()
			local excludesID = #()
			local st = timestamp()
			this.BeginSafeExtract mph  mphNode  excludesID
			for i = 1 to indices.Count do (
			    local info = this.GetMorphInfo  mph  indices[i]
			    if info  != undefined then (
			        if not info._IsTargetExist then  (
			            this.SnapshotTargets  mph   mphNode  info  false  layerName
			            this.BuildInfo  mph  info  false 
			        )
		            append infos info
			    )
			    else (
			    	BuboLogger.LogWarning "MorphJob.SelfExtract % can't find MorphInfo %" val:#(mphNode.name , indices[i] )
			    )
	        )
	        this.EndSafeExtract mph  mphNode 
-- 			format "% ms\n" (timestamp()-st)
		    return infos
	    ) 
	    catch (
	    	BuboLogger.LogError "MorphJob.SelfExtract : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    	return #()
	    )
	),

	fn DerivedExtract  mph  mphNode  destMph  snapShotNode  indices  excludesID  layerName  = (
		try (
			local infos = #()
-- 			local st = timestamp()
		   	this.BeginSafeExtract mph  mphNode   excludesID
			for i = 1 to indices.Count do (
			    local info =  this.GetMorphInfo  mph  indices[i]
	            this.SnapshotTargets  mph  snapShotNode  info  true  layerName
	            info._index = this.GetFreeChannel  destMph
	            this.BuildInfo  destMph  info  true 
	            append infos info
            )
            this.EndSafeExtract mph  mphNode 
-- 			format "% ms\n" (timestamp()-st)
          	return infos
	    ) 
	    catch (
	    	BuboLogger.LogError "MorphJob.DerivedExtract : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    	return #()
	    )
	),

	fn ExtractMorph  mph  mphNode  indices  layerName  = (
		try (
			suspendEditing()
			local targets = #()
			local infos = this.SelfExtract mph  mphNode  indices  layerName 
			for info in infos do (
				join targets info._targetTab 
			)
			select targets
			resumeEditing()
		)
		catch (
			BuboLogger.LogError "MorphJob.ExtractMorph : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			resumeEditing()
		)
	),

	fn OffsetMorph mph  mphNode  offsetNode  indices layerName = (
		try( 
			if mphNode.mesh.verts.count == offsetNode.mesh.verts.count then (
				suspendEditing()
				max modify mode 
				local tmpNode = copy offsetNode 
				local destMph = morpher()
				local id = this.GetFreeChannel mph
				local excludesID = #(id)

				this.BuildFromNode mph  id  tmpNode
				this.SetValue mph id 100.0

				TatMxsModifiersLib.DeleteAll offsetNode
				addModifier  offsetNode destMph

				this.CopyLimits mph destMph
			    this.DerivedExtract  mph  mphNode  destMph  mphNode  indices  excludesID layerName  
				this.DeleteChannel mph #(id)
				this.TransfertChannelScript   destMph

				delete tmpNode
				resumeEditing()
				modPanel.setCurrentobject destMph
			)
	    ) 
	    catch (
	    	BuboLogger.LogError "MorphJob.OffsetMorph : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    	resumeEditing()
	    )
	),

	fn WrappedMorph  mph  sourceNode  destNode indices layerName  createSkwIfNotExists:true  = (
		try( 
			max modify mode 
			local skw = (TatMxsModifiersLib.FindClass skin_wrap destNode)[1]
			if skw == undefined and createSkwIfNotExists then (
				skw  = Skin_Wrap()
				addModifier destNode  skw  before:(destNode.modifiers.count)
			)
			if skw != undefined then (
				select destNode			
				modpanel.setCurrentobject skw 

				local sCloned = copy sourceNode
			    TatMxsModifiersLib.deleteAll  sCloned

				skw.meshList = #(sCloned)
				skw.engine = 1
				skw.falloff = 0.001
				skw.distance = 0.001
				skw.faceLimit = 0
				skw.weightAllVerts = true

			    local mphCloned = copy mph
				addModifierWithLocalData  sCloned  mphCloned sourceNode mph

				local dCloned = copy destNode 
				TatMxsModifiersLib.DeleteAll  dCloned

				local destMph = morpher()
				local excludesID = #()
				addModifier  dCloned  destMph

			    this.CopyLimits mph destMph
			    destMph.name = mph.name
			    this.DerivedExtract  mphCloned  sCloned  destMph  destNode  indices  excludesID layerName 

				this.TransfertChannelScript destMph

				addModifier  destNode  destMph  before:destNode.modifiers.count

				deleteModifier  destNode  skw
				delete #( sCloned , dCloned )
			
				return destMph
			)
	    ) 
	    catch (
	    	BuboLogger.LogError "MorphJob.WrappedMorph : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    	resumeEditing()
	    	return undefined
	    )
	),

	fn CollapseMorph  mph  mphNode indices  = (
		try (
		    local modI = modPanel.getModifierIndex  mphNode  mph
	        local tmpMph = mphNode.modifiers[ modI + 1 ]
	        local excludesID = #()
	        if classof tmpMph == morpher then (
	        	SuspendEditing()
	        	local tmpNode = copy mphNode 
	        	local destMph = this.Clone tmpMph 
	        	TatMxsModifiersLib.DeleteAll  tmpNode
				addModifier  tmpNode  destMph
	        	local infos = this.DerivedExtract  mph  mphNode  destMph  mphNode  indices  excludesID  "0"  
	        	for info in infos do (
	        		info.DeleteRebuilded()
	        	)
	        	delete tmpNode
	        	resumeEditing()
	        	ModPanel.SetCurrentObject tmpMph
	        	ModPanel.AddModToSelection destMph
	        	SuspendEditing()
	        	this.TransfertChannelScript   destMph
	        	deleteModifier mphNode tmpMph
	        	resumeEditing()
	        	ModPanel.SetCurrentObject destMph
	        	return destMph
	        )
	    ) 
	    catch (
	    	BuboLogger.LogError "MorphJob.CollapseMorph : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    	resumeEditing()
	    	return undefined
	    )
    ),

	fn RemoveUnusedChannels  mph  mphNode tolerance  = (
		try (
			suspendEditing()
		    local baseObj = this.ExtractBaseTarget  mphNode   "0"
		    local indices = this.GetValidChannels mph 
		    local infos = this.SelfExtract  mph  mphNode  indices  "0" 

		    for info  in  infos  do (
		        if info != undefined then (
		            local unused = true
		            for  i = 1 to info._progCount  where isValidNode info._targetTab[i] do(
		                local targNode =  info._targetTab[i]
		                local offsetVtxInfo = PolyJob.GetOffsetVertex baseObj  targNode tolerance

		                if offsetVtxInfo.movedVtx.count > 0 then(
		                    unused = false
		                )
		            )
		            if unused then (
		                WM3_MC_Delete mph info._index
		            )
		            info.DeleteRebuilded()
		        )
		    )
		    delete baseObj
		    resumeEditing()
		    modPanel.setCurrentobject mph
	    )
		catch (
			BuboLogger.LogError "MorphJob.RemoveUnusedChannels : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			resumeEditing()
		)
	),

	fn ReplaceFromNodes mph nodeList =(
		try (
			suspendEditing()
		    local ListName = for n in nodeList collect n.name
		    local indexList = this.GetChannelByName mph ListName

		    for i = 1 to nodeList.count where  indexList[i] != undefined do (
		        local info = this.GetMorphInfo mph  indexList[i]
		        info.ReplaceTarget #(nodeList[i]) deleteOld:false
		        this.BuildInfo  mph  info  true
		    )
		    resumeEditing()
		    return indexList
	    )
	    catch(
	    	Print ( "ReplaceFromNodes" + getCurrentException())
	    	resumeEditing()
	    	return #()
	    )
	),
	
	fn MirrorMorph  mph  mphNode  indices  side  layerName = (
		try (
			suspendEditing()
			mph.Autoload_of_targets = 1
			local isCopyController = false 
		    local morphMirrored = #()
		    local sideMirrored = if side == "D" then "G" else "D"
		    local infos =  this.SelfExtract  mph  mphNode  indices  "0" 

		    for info in infos do (
		    	local mirrorInfo = info.CloneMirror() 
		        mirrorInfo._index = this.FindChannelIndex  mph  mirrorInfo._name  true

		    	this.BuildInfo  mph  mirrorInfo  true  isAssignController:isCopyController
		    	this.AddToLayer layerName  mirrorInfo._targetTab 
		    	this.MirrorTargetsVtx 	mirrorInfo._targetTab  "Inverse"  

		    	if isCopyController then (
	            	this.MirrorController   mirrorInfo._controller   sideMirrored
	            	this.InvMirrorController  mirrorInfo
            	)
	            info.DeleteRebuilded()
		    )
		    resumeEditing()
		    modpanel.setCurrentobject mph
	    ) 
	    catch (
	    	BuboLogger.LogError "MorphJob.MirrorMorph : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    	resumeEditing()
	    )
	),

	fn MirrorController ctrl side = (
	    --change FloatSript Connection mapping opposites nodes
	    local ctrl  = controls.GetFloatController ctrl
	    if classof  ctrl == float_script do(
	        local vartypeList =#( #target , #node , #object )

	        for i = 1 to vartypeList.Count do(

	            local varType = vartypeList[i]
	            local varScriptList = controls.GetScriptListVarValue  ctrl   type:varType

	            for varScript in varScriptList where classof varScript == St_ScriptVarValue do (

	                local sub = varScript._value
	                --format "sub : %\n" sub
	                local mirrorSub = controls.MapMirrorSubAnim  sub   side
	                --format "mirrorSub : %\n" mirrorSub
	                if mirrorSub != undefined then (

	                    controls.SetScriptVarValue   ctrl  varScript._name  mirrorSub   varType
	                )
	            )
	        )
	    )
	),

	fn InvController mph sourceId destId = (
	    local flMult
	    local sourceTrack = mph[sourceId]
	    local c = controls.GetFloatController sourceTrack.controller
	    local multiVal = if classof c == Float_Multi then  c.multi * -1 else  -1

	    local destTrack = mph[destId]
	    c = controls.GetFloatController destTrack.controller

	    if classof c == Float_Multi then (
	        c.multi = multiVal
	    ) else (
	        flMult = Float_Multi()
	        flMult.multi = multiVal
	        flMult.input.controller = destTrack.controller
	        destTrack.controller = flMult
	    )
	    flMult
	),

	fn InvMirrorController info =(

	    local DoInverse = false

	    local sides =#("D_", "G_")
	    for i = 1 to 2 do (

	        if matchpattern  info._name pattern:( sides[i] +"*_Up") then(
	            DoInverse = true
	        )else if  matchpattern  info._name pattern:( sides[i] +"*_Down") then(
	            DoInverse = true
	        )else if  matchpattern  info._name pattern:( sides[i] +"*_Twist*") then(
	            DoInverse = true
	        )else if  matchpattern  info._name pattern:( sides[i] +"*_In") then(
	            if  not matchpattern  info._name pattern:( "*_Finger*") then(
	                DoInverse = true
	            )
	        )else if  matchpattern  info._name pattern:( sides[i] +"*_Out") then(
	            if not  matchpattern  info._name pattern:( "*_Finger*") then(
	                DoInverse = true
	            )
	        )else()
	    )

	    if DoInverse do(

	        local sourceId = info._mirrorInfo._index
	        local destId = info._index

	        this.InvController   info._morpherMod sourceId  destId
	    )
	),

	fn TransfertChannelScript  destMph = (
		try(
		    for j = 1 to  destMph.numSubs do(

		        local flscript =  destMph[j].controller
		        local vartypeList =#( #target , #node , #object )

		        for i = 1 to vartypeList.Count do(

		            local varType = vartypeList[i]
		            local varScriptList = Controls.GetScriptListVarValue   flscript  type:varType
		            --
		            for varScript in varScriptList where classof varScript == St_ScriptVarValue do (

		                sub = varScript._value
		                indices = Controls.GetMorpherHierarchySubAnim  sub  &parentSubInfo

		                if indices.count > 0 do(
		                    local sourceMph = parentSubInfo._value
		                    chName = WM3_MC_GetName   sourceMph   indices[1]

		                    for k = 1 to  destMph.numSubs where  chName ==  WM3_MC_GetName  destMph  k do(

		                        Controls.SetScriptVarValue   flscript  varScript._name  destMph[k]   varType
		                    )
		                )
		            )
		        )
		    )
	    )
	    catch (
	    	BuboLogger.LogError "MorphJob.TransfertChannelScript : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    )
	),

	fn AddToLayer layerName targets = (

        local curLayer = LayerManager.getLayerFromName layerName
        if curLayer == undefined do(
            curLayer = LayerManager.newLayerFromName layerName
        )
        for  target in targets do(
            curLayer.addNode   target
        )
	)

)

MorphJob = St_MorphJob()

