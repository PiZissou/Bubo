
global BuboSym
global TatMxlLibObjectNames
global TatMxsModifiersLib


struct St_SkinJob (
	/*
    <struct>
        <description> 
        	Extends Skin functions  
        	used in SkinEngine.cs, SkinMod.cs and BuboAPI
    	</description>
		<authors>PLA</authors>
    </struct>
    */
	_holdId = 463,
	_origColorId = 464 ,--2828177
	_displayId = 465 ,
	_holdColor	= color 0 0 0 ,
	_mainColor	= color 255 89 7,
	_handleColor = color 135 6 6,
	_twistColor  = color 203 42 5,
	_properties = #(),
	IsDisplayModified = false,

	fn WrappedSkin  sourceSk sourceNode destNode  = (
		try(
			/* 
			if the stack of modifiers contains other skin above sourceSk,
			then wee need clone sourceSk above otherwise the skin_wrap will not compute the right skin!
			we'll delete the cloned sourceSk at the end
			*/
			local sCloned = copy sourceNode
		    TatMxsModifiersLib.deleteAll  sCloned

			local mphs = TatMxsModifiersLib.FindClass  morpher sourceNode
			for mph in mphs where mph.enabled do (
				addModifierWithLocalData  sCloned (copy mph)  sourceNode mph 
			)
			addModifierWithLocalData  sCloned (copy sourceSk)  sourceNode sourceSk

			select destNode			
			max modify mode
			local skw = (for o in destNode.modifiers where classof o == Skin_Wrap collect o)[1]
			if skw == undefined then (
				skw = Skin_Wrap()
				addModifier destNode  skw  before:(destNode.modifiers.count)
			)
			modpanel.setCurrentobject skw 
			skw.meshList = #(sCloned)
			skw.engine = 1
			skw.falloff = 0.001
			skw.distance = 0.001
			skw.faceLimit = 0
			skw.weightAllVerts = true
			skw.ConvertToSkin true
			local skins = TatMxsModifiersLib.FindClass skin destNode
			local sk = skins[1]
			sk.enableDQ = true
			sk.filter_vertices = sourceSk.filter_vertices
			sk.showNoEnvelopes = sourceSk.showNoEnvelopes
			sk.name = sourceSk.name

			deleteModifier  destNode  skw
			delete sCloned
			return sk
		)
		catch (
			BuboLogger.LogError "SkinJob.WrappedSkin : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return undefined
		)
	),

	fn ReplaceVertexWeights sk vtx bonesId weigths = (
		skinOps.ReplaceVertexWeights sk  vtx  bonesId  weigths
	),

	fn RemoveUnusedBones sk threshold = (
		skinOps.RemoveUnusedBones sk  threshold:threshold  
	),

	fn InitSkin sk = ( 
		sk.showNoEnvelopes = true
        sk.filter_vertices = true
        sk.filter_envelopes = true
	),

	fn AddNewSkin n = (
        Max Modify mode
        sk = skin()
        local index = n.modifiers.count
        local skins = TatMxsModifiersLib.FindClass skin n 
        if skins.count>0 then (
        	index = modPanel.getModifierIndex  n skins[skins.count]
        )
        else (
        	local morphs = TatMxsModifiersLib.FindClass morpher n 
        	if morphs.count>0 then (
        		index = modPanel.getModifierIndex  n morphs[morphs.count]
        	)
        )
        addModifier  n  sk  before:index 
        this.InitSkin sk
        return sk
	),

	fn Shrink sk = (
		undo "Shrink" on (
			skinOps.ShrinkSelection sk
		)
	), 

	fn Grow sk = (
		undo "Grow" on (
			skinOps.GrowSelection sk
		)
	), 

	fn Ring sk = (
		undo "Ring" on (
			skinOps.RingSelection sk
		)
	), 

	fn Loop sk = (
		undo "Loop" on (
			skinOps.LoopSelection sk
		)
	), 

	fn SetScaleWeights sk val = (
		undo "SetScaleWeights" on (
			skinOps.scaleWeight sk val
		)
	), 

	fn ToogleEditEnvelopes  = (
		local sk =  modPanel.getCurrentObject() 
		if classof sk == Skin then ( 
			subobjectLevel = if subobjectLevel == 0 then 1 else 0
		)
	),

	fn SetEditEnvelopes sk skinNode onOff = (
		if modPanel.getCurrentObject() != sk then (
			modPanel.setCurrentObject sk
		)
		subobjectLevel = if onOff then 1 else 0
	),

	fn SetDisplayFaces sk onOff = (
		 sk.shadeweights = onOff
	),

	fn SetDisplayVertices sk onOff = (
		sk.draw_vertices = onOff
	),

	fn SuspendDisplay currentSk skList = (
		try (
			for sk in skList where sk != currentSk and ( getAppdata   sk  _displayId ) == undefined do (
				setAppData  sk  _displayId  (#(sk.draw_vertices, sk.shadeweights) as string)
				sk.draw_vertices = false
				sk.shadeweights = false
			)
		)
		catch (
			BuboLogger.LogError "SkinJob.SuspendDisplay : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	),

	fn ResumeDisplay sk = (
		try (
			local data = getAppdata   sk  _displayId
			if data != undefined then (
				onOffList = execute data
				sk.draw_vertices = onOffList[1]
				sk.shadeweights = onOffList[2]
				deleteAppdata sk _displayId
			)
		)
		catch (
			BuboLogger.LogError "SkinJob.ResumeDisplay : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	),

	fn EditEnvelopesDisplay sk skinNode  shading vertexColor = (
		try(
			if isValidNode  skinNode then (
				this.IsDisplayModified = true
				local mods = TatMxsModifiersLib.FindClass skin skinNode
				
				if shading then ( 
					displayColor.shaded = #object 
				) 
				else ( 
					displayColor.shaded = #material
				)		
				skinNode.showVertexColors = if sk.shadeweights then vertexColor else false

				this.SuspendDisplay sk mods 
				this.ResumeDisplay sk
			)
		)
		catch (
			BuboLogger.LogError "SkinJob.EditEnvelopesDisplay : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	),

	fn GetMutedPropertyNames  = (
		return	#( #draw_vertices , #shadeweights )
	),

	fn GetPropertyNames sk = (
		try (
			local propertyNames = getPropNames	sk
			local classes = #(BooleanClass, float, integer)
			local result = #()

			for propertyName in propertyNames where IsProperty sk propertyName do (
				local val = getProperty sk propertyName
				if findItem classes (classof val) then (
					append result propertyName
				)
			)
			return result
		)
		catch (
			BuboLogger.LogError "SkinJob.GetPropertyNames : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return #()
		)
	),

	fn SaveProperties sk = (
		try (
			if classof sk == skin then(
				local propertyNames = this.GetMutedPropertyNames()
				_properties = for propertyName in propertyNames collect (getProperty sk propertyName)
			)
		)
		catch (
			BuboLogger.LogError "SkinJob.SaveProperties : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	),

	fn GetPropetyChanged sk = (
		try (
			local propertyNames =  this.GetMutedPropertyNames()
			for i = 1 to propertyNames.Count do (
				if _properties[i] != getProperty sk propertyNames[i] then (

					return propertyNames[i]
				)
			)
			return ""
		)
		catch (
			BuboLogger.LogError "SkinJob.GetPropetyChanged : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return ""
		)
	),

	fn IsMutedPropetyChanged sk = (
		if classof sk == skin then(
			local muted = this.GetMutedPropertyNames()
			local propertyName = this.GetPropetyChanged sk
			return ( findItem muted propertyName ) != 0
		)
		else (
			return false
		)
	),

	fn AddVertexDQWeightSelected sk val = (
		undo "AddVertexDQWeightSelected" on (
			for  vtx = 1 to skinOps.GetNumberVertices  sk  do (
			    if  1 == ( skinOps.IsVertexSelected sk   vtx  )  then (
			    	local dqV = skinOps.getVertexDQWeight sk vtx  
			    	dqv += val
			    	dqv = if dqv < 0.0 then 0 else dqv
			    	dqv = if dqv > 1.0 then 1 else dqv
			    	skinOps.setVertexDQWeight sk vtx dqv
		    	)
	    	)
	    	skinOps.enableDQOverrideWeighting sk true
	    )
	),

	fn GetDisplayFaces sk  = (
		try (
			if modPanel.getCurrentObject() == sk and subobjectLevel == 1 then (
				return sk.shadeweights 
			)
			else (
				return false
			)
		)
		catch (
			BuboLogger.LogError "SkinJob.GetDisplayFaces : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	),

	fn GetDisplayVertices sk  = (
		try (
			if modPanel.getCurrentObject() == sk and subobjectLevel == 1 then (
				return if sk.shadeweights then false else sk.draw_vertices 
			)
			else (
				return false
			)
		)
		catch (
			BuboLogger.LogError "SkinJob.GetDisplayVertices : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	),
	/*
	skinJob.GetEnabledDQ (modPanel.getcurrentObject())
	*/
	fn GetEnabledDQ sk = ( 
		try (
			if modPanel.getCurrentObject() == sk and subobjectLevel == 1 then ( 
				return sk.enableDQ 
			)
			else(
				return false
			)
		)
		catch (
			BuboLogger.LogError "SkinJob.GetEnabledDQ : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	),

	fn GetEditEnvelopes sk = (
		try (
			if modPanel.getCurrentObject() == sk then ( 
				return if subobjectLevel == 1  then true else false
			)
			return false
		)	
		catch (
			BuboLogger.LogError "SkinJob.GetEditEnvelopes : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	), 

	fn GetEditingDQ sk = (
		try (
			if modPanel.getCurrentObject() == sk and subobjectLevel == 1 then ( 
				for vtx = 1 to  skinOps.GetNumberVertices  sk  where skinOps.GetVertexWeightCount  sk  vtx do ( 
					if skinOps.GetVertexWeightBoneID  sk  vtx  1 == 0 then (
						return true
					)
					else (
						return false
					)
				)
			)
			return false
		) 
		catch (
			BuboLogger.LogError "SkinJob.GetEditingDQ : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	), 

	fn GetPaintMode sk blendMode = (
		try (
			if modPanel.getCurrentObject() == sk and subobjectLevel == 1 then (
				if blendMode then (
					return thePainterInterface.inPaintMode() and sk.paintBlendMode 
				) 
				else (
					return thePainterInterface.inPaintMode() and not sk.paintBlendMode 
				)
			)
			else (
				return false
			)
		) 
		catch (
			BuboLogger.LogError "SkinJob.GetPaintMode : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
			return false
		)
	),

	fn GetVertexDQWeight sk id = (

		local val = skinOps.getVertexDQWeight sk id
		return val 
	),

	fn SetVertexDQWeight sk id val = (

		skinOps.SetVertexDQWeight sk id val
	),

	fn SetVertexDQWeightSelected sk val = (
		undo "SetVertexDQWeightSelected" on (
			for  vtx = 1 to skinOps.GetNumberVertices  sk  do (
			    if  1 == ( skinOps.IsVertexSelected sk   vtx  )  then (
			    	skinOps.setVertexDQWeight sk vtx val
		    	)
	    	)
	    	skinOps.enableDQOverrideWeighting sk true
	    )
	),

	fn SetEnabledDQ sk onOff  = ( 
		sk.enableDQ = onOff
		skinOps.enableDQOverrideWeighting sk (this.GetEditingDQ sk)
	),

	fn SetEditingDQ sk onOff = (
		if modPanel.getCurrentObject() == sk then ( 
			skinOps.enableDQOverrideWeighting sk onOff
		)
	),

	fn SetPaint sk OnOff = (
		local needOpen = not thePainterInterface.inPaintMode() and  OnOff
		local needClose = thePainterInterface.inPaintMode() and not OnOff
		if needOpen or needClose then (
			skinOps.paintWeightsButton sk
		) 
	),

	fn SetPaintWeights sk val blendMode onOff = (
		this.SetPaint sk onOff
		if onOff then (
			sk.paintBlendMode = blendMode
		)
		thepainterinterface.maxstr = val
	),

	fn RemoveZeroWeights sk  val = (
		undo "RemoveZeroWeights" on (
			sk.clearZeroLimit = val
			skinOps.RemoveZeroWeights  sk
		)
	),

	fn DisplayHoldBone n onOff = (
		if onOff then (
			if this.IsHold n then (
				n.wirecolor  =  _holdColor
			) else (
				this.UnDisplayHoldBone n
			)
		) else (
			this.UnDisplayHoldBone n
		)
	),

	fn UnDisplayHoldBone n = (
		local originColor = this.GetOriginColor n
		if originColor != undefined then (
			n.wirecolor  = originColor
		)
	),

	-- used by PocPoc
	fn HoldBones sk onOff = (
		if classof sk == Skin then (
			local nbBones = skinOps.GetNumberBones sk
			for i = 1 to nbBones do (
				local boneName = skinOps.GetBoneName sk i 0
				local boneNode = getNodeByName boneName
	            this.HoldBone boneNode onOff
			)
		)
	),

	fn HoldBone n onOff = (
		if not onOff then (
			setAppdata n _holdId "false"
			this.UnDisplayHoldBone n
		)
		else if onOff  and not this.IsHold n  then (
			setAppdata n _holdId "true"
			setAppdata n _origColorId ( n.wirecolor  as string )
			n.wirecolor  =  _holdColor
		)
	),

	fn IsHold n = (
		local onOff = false
		try(
			local data = getAppdata   n  _holdId
			if data != undefined then (
				onOff = execute data
			)
		)
		catch (
			BuboLogger.LogError "SkinJob.IsHold : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
		onOff
	),

	fn GetOriginColor  n = (
		local originColor 
		try(
			local data = getAppdata   n  _origColorId
			if data != undefined then (    
				originColor = execute data 

				if originColor == _holdColor then (
					local rgx = dotnetObject "System.Text.RegularExpressions.Regex" ""

					if  rgx.ismatch  n.name  "(Pelvis)|(Handle)|(Tail)|(Ear)|(Belly)" then (
						originColor = _handleColor
					) else if   rgx.ismatch  n.name  "(Twist)"  then (
						originColor = _twistColor
					) else (	
						originColor = _mainColor
					)
				)
			)
		)
		catch (
			BuboLogger.LogError "SkinJob.GetOriginColor : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
		originColor
	),

	fn ResetPos  sk  skinBones  = (

        local splines = for b in skinBones where classof b.baseobject == SplineShape collect b
        local points = #()

        for i = 1 to splines.count do (
            local p = point()
            append points p
            this.replaceBone sk #(splines[i])  p
        )
        --local reff =  sk.ref_frame
        sk.ref_frame = 0
        sk.always_deform = false
        sk.always_deform = true

        for i = 1 to splines.count do (

            this.replaceBone sk #(points[i])  splines[i]
            delete points[i]
        )
	),

	fn  ReplaceVtxWeights sk vtx sourceIds destId  = (
	    local boneIds = #()
	    local weights = #()
	    local finddestId = false
	    local sourcesK = #()
	    local destK 
	    local inflCount = skinOps.GetVertexWeightCount   sk   vtx

	    for k = 1 to inflCount do(
	        local id =  skinOps.GetVertexWeightBoneID    sk  vtx   k
	        local weight = skinOps.GetVertexWeight sk vtx k 
	        --print (  "id =  " + skinOps.GetBoneName sk id 0 )
	        if id == destId  then(
	            destK = k
	            append  boneIds  id
	            append  weights  weight
	           -- print (  "equal destId " + skinOps.GetBoneName sk id 0 )
	        ) else if findItem sourceIds id == 0 then (
               	append  boneIds  id
                append  weights  weight
               -- print (  "not in sourceIds " + skinOps.GetBoneName sk id 0 )
	        ) else (
		        append sourcesK  k
		        --print ( "In sourceIds " + skinOps.GetBoneName sk id 0 )
	       	)
	    )
	    if sourcesK.count > 0 do(

        	for  sourceK in  sourcesK do (
        		local sourceWeight = ( skinOps.GetVertexWeight sk vtx sourceK )

            	local j = findItem boneIds destId 
            	if j == 0  then (
               		append  boneIds  destId
                	append  weights  sourceWeight
                	--print (  "j == 0  " + skinOps.GetBoneName sk destId  0 )
                ) else (
                	weights[j] += sourceWeight
                	--print (  "weights[j]  " + skinOps.GetBoneName sk destId  0 )
                )
            )

/*             print boneIds
            print "--------"
            print weights
            print "--------" */

	        skinOps.ReplaceVertexWeights sk vtx boneIds weights
	    )
	),

	fn ReplaceBone  sk  sourcesBones  destBone  selectBone:false onSelectedVertex:false = (
		if modPanel.getCurrentObject() != sk then (
	        modPanel.setCurrentObject sk
	    )
        local destId = this.GetBoneId sk destBone
        if destId == undefined then (
            skinOps.addbone sk destBone  1
            destId = this.GetBoneId sk destBone
        )
        local sourceIds = #()

        for i = 1 to sourcesBones.Count do (
            local source =  sourcesBones[i]
            append sourceIds ( this.GetBoneId sk source )
        )

        if sourceIds.count > 0 and  destId != undefined then (
	        if	onSelectedVertex then (
				for  vtx = 1 to skinOps.GetNumberVertices  sk where skinOps.IsVertexSelected sk vtx do (
				    this.ReplaceVtxWeights sk vtx sourceIds  destId
				) 
	        ) else (
				for  vtx = 1 to skinOps.GetNumberVertices  sk do (
				    this.ReplaceVtxWeights sk vtx sourceIds  destId
				) 
	        )
        )
	    true
	),

	fn SelectBone sk index = (
		if modPanel.getCurrentObject() == sk then (
	        SkinOps.SelectBone sk index
	    )
	),

	fn AddBone sk AddBones = (

	    if modPanel.getCurrentObject() != sk then (
	        modPanel.setCurrentObject sk
	    )
	    local bonesName = #()
        for  i = 1   to  skinOps.GetNumberBones sk do (
            bname = skinOps.GetBoneName sk    i    0
            append  bonesName   bname
        )
        for skinNode in  AddBones where findItem bonesName skinNode.Name == 0 do (
            skinOps.addBone  sk  skinNode   1
        )
	    true
	),

	fn RemoveBone sk  id = (
		if sk != modPanel.getCurrentObject()  then (
			modPanel.setCurrentObject sk
		)
		skinOps.removebone sk id
	),

	fn GetBoneId sk skinBone = (

		local index = undefined
	    for  i = 1  to  skinOps.GetNumberBones  sk do (
	        if ( skinBone.name == skinOps.GetBoneName  sk i 0) then(
	            index = i
	        )
	    )
	    index
	),

	fn MirrorWeights sk skinNode side = (
		rollout  MirrorProgressBar  "" width:163 height:94
		(
			label 	lb 		"Mirror Process..."		pos:[23,7] width:117 height:13
			progressBar c_process 	"" pos:[13,34] width:139 height:25
		)
		try(
			local numverts = polyop.getNumVerts skinNode.baseobject
			if  BuboAPI.Sym.Count == numverts then (

			    if  sk != modPanel.getCurrentObject() then (
			    	modPanel.setCurrentObject sk
		    	)
				
		        local pos = BuboAPI.Sym.Positives 
				local neg = BuboAPI.Sym.Negatives 

				if this.GetEditingDQ sk then (
					if 	side == "D" then (
						for i = 1 to pos.count do (
							skinOps.setVertexDQWeight sk pos[i] ( skinOps.getVertexDQWeight sk neg[i] )
						)
					)
					if 	side == "G"  then (
						for i = 1 to pos.count do (
							skinOps.setVertexDQWeight sk neg[i] ( skinOps.getVertexDQWeight sk pos[i] )
						)
					)
					skinOps.enableDQOverrideWeighting sk (this.GetEditingDQ sk)
				) else (
					createdialog MirrorProgressBar
			        this.MirrorBonesDetect sk  skinNode &bonesPos  &bonesNeg &boneMid

					for i = 1 to pos.count do (

						this.GetSkinData sk pos[i] &bonesL &weightsL  
						this.GetSkinData sk neg[i] &bonesR &weightsR 

						local tmp = #()
						if 	side == "D" then (
							tmp = this.MirrorBones  bonesR bonesNeg bonesPos 
							if  tmp  !=  undefined  do(
								skinOps.replaceVertexWeights sk  pos[i]  tmp  weightsR    
							)
						)
						if 	side == "G"  then (
							tmp = this.MirrorBones bonesL bonesPos bonesNeg 
							if  tmp  !=  undefined  do(
								skinOps.replaceVertexWeights sk neg[i]  tmp  weightsL  
							)
						)
						MirrorProgressBar.c_process.value = (( i as float) / bonesPos.count * 100.0 )
					)
					destroydialog MirrorProgressBar
				)
			)
		) 
		catch (
			destroydialog MirrorProgressBar
			BuboLogger.LogError "SkinJob.MirrorWeights : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	),

	fn  MirrorBones bonesA mirrorA mirrorB  = (
	    local bonesB = #()
        for j = 1 to bonesA.count  do (
            for i = 1 to   mirrorA.count do (
                if bonesA[j] == mirrorA[i] then (
                    bonesB[j] = mirrorB[i]
            	)
            )
        )
        for j = 1 to  bonesA.count do (
            if bonesB[j] == undefined  do (
            	for i = 1 to   mirrorB.count do (
                    if bonesA[j] == mirrorB[i] then (
                        bonesB[j] = mirrorA[i]
                    )
            	)
            )
        )
        for j = 1 to  bonesA.count  do (
            if bonesB[j] == undefined  then (
            	bonesB[j] =  bonesA[j]
            )
        )
	    bonesB
	),

	fn GetSkinData sk vtx &bonesB &weights = (
		local inflCount = skinOps.GetVertexWeightCount sk vtx
		local bonesB = #()
		local weights = #()

		for i = 1 to inflCount do (
			append  bonesB (  skinOps.GetVertexWeightBoneID    sk  vtx   i )
			append  weights  (  skinOps.GetVertexWeight  sk  vtx   i  )
		)
	),

	fn TransfertWeightBones sk  transfertPercent = (
	    if sk != modPanel.getCurrentObject()  then (
	    	modPanel.SetCurrentObject sk
    	)
    	local unholdIDs = this.GetUnHoldedBonesId sk
        if unholdIDs.count == 2 then (
            local selectedBoneId = skinOps.GetSelectedBone  sk
            local selBoneInd = findItem unholdIDs selectedBoneId
            if selBoneInd > 0 then (
                if selBoneInd == 1 then (
                    targetTransfertBoneID = unholdIDs[2]
                    sourceTransfertBoneID = unholdIDs[1]
                ) else (
                    targetTransfertBoneID = unholdIDs[1]
                    sourceTransfertBoneID = unholdIDs[2]
                )

                local vertices = skinOps.GetNumberVertices  sk
                for vtx = 1 to vertices do (
                    if skinOps.IsVertexSelected sk vtx then (
                        local nbInfBones = skinOps.GetVertexWeightCount  sk  vtx
                        local sumOtherWeight = 0
                        for k = 1 to nbInfBones do (
                            local infBoneId = skinOps.GetVertexWeightBoneID sk vtx k
                            if infBoneId != targetTransfertBoneID and infBoneId != sourceTransfertBoneID then (
                                sumOtherWeight += skinOps.GetVertexWeight sk vtx k
                            )
                        )
                        amoutToShare = 1.0 - sumOtherWeight
                        sourceWeight = transfertPercent * amoutToShare
                        targetWeight =  (1.0 - transfertPercent) * amoutToShare
                        skinOps.ReplaceVertexWeights sk vtx #(sourceTransfertBoneID, targetTransfertBoneID) #(sourceWeight, targetWeight)
                    )
                )
            ) else (
                messageBox "Selected bone is not unhold !"
            )
        ) else (
            messageBox "Invalid number of unhold bones !"
        )
	),
	fn GetUnHoldedBonesId  sk  = (
	    local outputId =#()
	    if classof sk   ==  skin then (
			nb = skinOps.GetNumberBones   sk

			for i =1  to nb do (
				boneName =	skinOps.GetBoneName  sk   i   0
				local boneNode = getNodeByName boneName

	            if not this.IsHold boneNode  then (
	                append outputId i
	            )
			)
		)
	    outputId
	),
	fn MirrorBonesDetect sk skinNode  &bonesPos  &bonesNeg  &boneMid = (
		bonesPos = #()
		bonesNeg = #()
		bonesMid = #()
		local notFound = #()
		local boneNames = #()
		for  i = 1  to skinOps.GetNumberBones  sk  do(
			append  boneNames (  skinOps.GetBoneName   sk   i  0  )
		)

		for  i = 1 to boneNames.count do (
			local boneName = boneNames[i]
			local rgx = dotnetObject "System.Text.RegularExpressions.Regex" ""

			if  rgx.ismatch  boneName  "_G[0-9]?_"  then (
				local mirrorObj = TatMxlLibObjectNames.ToMirrorName  boneName
				local j = findItem boneNames mirrorObj

				if j != 0 then(
					append bonesPos i
					append bonesNeg j
				)else(
					append notFound boneName
				)
			) 
			else if rgx.ismatch  boneName  "_D[0-9]?_"  then (
				continue
			)
			else (
				append 	bonesMid  i
			)
		)

		if notFound.count > 0 then (
			local msg = "Missing Mirror Bone:\n"
			for m in notFound do (
				msg += m + "\n"
			)
			Print msg
		)
	),

	fn  AddWeights sk  val  = (

	    if  classof sk == skin and  sk == modPanel.getCurrentObject()  then (

	        local selId = skinOps.GetSelectedBone  sk
	        local selNode = skinOps.GetBoneNode sk selId

	        if this.IsHold selNode then (
	        	return false
	        )
	        for  vtx = 1 to  skinOps.GetNumberVertices  sk  do (
	            if  1 == ( skinOps.IsVertexSelected sk   vtx  )  then (
	                local inflCount = skinOps.GetVertexWeightCount   sk   vtx
	                local bufferBonesId = #()
	                local bufferWeights = #()
	                local sourceInclude = false

	                for k = 1 to inflCount do (
	                    if selId  ==  (  skinOps.GetVertexWeightBoneID    sk  vtx   k ) then (
	                        sourceInclude = true
	                    )
	                	append  bufferBonesId (  skinOps.GetVertexWeightBoneID    sk  vtx   k )
	                	append  bufferWeights  (  skinOps.GetVertexWeight  sk  vtx   k  )
	                )
	                if  sourceInclude do(

	                    append  bufferBonesId (  selId  )
	                    append  bufferWeights  (  0.0 )
	                )
	               	if bufferBonesId.count > sk.bone_Limit then (
	               		sk.bone_Limit += 1
	               	)
	                local holded = #{}
	                local current = 0

	                for i = 1 to bufferBonesId.count do (
	                    if this.IsHold ( skinOps.GetBoneNode sk bufferBonesId[i]) then (
	                        append holded i
	                    )
	                    if bufferBonesId[i] == selId do (
	                        current = i
	                    )
	                )
	                if not holded[current] then (
                        if bufferWeights[current] == 0.0 and val < 0.0 then(

                        ) else (
                            --newWeightList = AgloNormalizeWithNHolded bufferWeights holded val current
                            --skinOps.replaceVertexWeights sk vtx bufferBonesId newWeightList
                        )
                    )
                    else(
                        messagebox "Increase the Bone Affect Limit."
                        exit
                    )
	            )
	        )
	        return true
	    )
	    return false
	),
	/*
	GetSkinObjectsSSK is obsolete
	*/
	fn GetSkinObjectsSSK sk:undefined cleanWireColor:false = (
		local b_list = #()
		local b_name_list = #()
		local index_conflict = #()

		if classof  sk == skin then (
			modpanel.setcurrentobject sk
			nb = skinOps.GetNumberBones sk

			for  i = 1   to  nb do (
				b_name = skinOps.GetBoneName sk i 0
				append b_name_list b_name
			)

			for  i = 1 to  b_name_list.count do	(
				b1 = b_name_list[i]
				for j = 1 to b_name_list.count do (
					b2 = b_name_list[j]
					if b1 == b2 and i != j  do (
						format  "Warning same name % for != bones \n"b1
						append  index_conflict   ( getnodebyname  b1   )
					)
				)
			)
			if index_conflict.count == 0 then (
				for b_name in b_name_list do (
					b = getnodebyname  b_name
					if b == undefined  then	(
						format  "Warning bone name doesn't match node in scene % \n"b1
					)
					if  cleanWireColor == true do  deleteAppdata  b 2828177
					append  b_list    b
				)
			)
		)
		b_list
	)
)

SkinJob = St_SkinJob()