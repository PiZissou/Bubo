global BuboSym
global BuboAPI
global toleranceMirrorVtx = 0.005

struct  ST_OffsetVtxInfo
(
	/*
    <struct>
        <description> used in PolyJob </description>
    </struct>
    */
    numverts,
    movedVtx =#(),
    offsetPosVtx=#()
)


struct  St_PolyJob
(
	/*
    <struct>
        <description> used in MorphJob </description>
    </struct>
    */
	fn MirrorVtx n side  = (
		try (	
			local numverts = polyop.getNumVerts n.baseobject
 			if  numverts == BuboAPI.Sym.count then (
				local userCoordsys = #local
				local userAxes=[-1.0,1.0,1.0]
			    local pos =  BuboAPI.Sym.Positives 
				local neg =  BuboAPI.Sym.Negatives 
				local mid =  BuboAPI.Sym.Middles 
	            for  i = 1  to pos.count do (
					local invPos = ( polyop.getVert  n.baseobject pos[i] ) * userAxes
					local invNeg = ( polyop.getVert  n.baseobject neg[i] ) * userAxes

					if  side == "D"   then (
						polyop.setVert n.baseobject pos[i] invNeg
					)
					if  side ==  "G"   then (
						polyop.setVert n.baseobject neg[i] invPos
					)
					if   side == "Inverse"   then (
						polyop.setVert n.baseobject pos[i] invNeg
						polyop.setVert n.baseobject neg[i] invPos
					)
				)
				for  i = 1  to mid.count do (
					invMid = ( polyop.getVert n.baseobject  mid[i] ) * userAxes
	                polyop.setVert n.baseobject  mid[i] invMid
				)
			)
	    )
	    catch (
	    	BuboLogger.LogError "PolyJob.MirrorVtx : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
	    )
	),

	fn GetOffsetVertex  n  destN  tolerance = (

	    local offsetPosVtx =#()
	    local movedVtx =#()

	    if not classof n.baseobject  == editable_poly or not classof destN.baseobject  == editable_poly   or  polyop.getNumVerts n.baseobject  !=  polyop.getNumVerts  destN.baseobject  do
	        return undefined

	    local num = polyop.getNumVerts n.baseobject

	    for  vtx = 1  to   num  do (
	        vertPos =  polyop.GetVert    n.baseobject  vtx
	        destNvertPos =  polyop.GetVert    destN.baseobject  vtx

	        offset = destNvertPos - vertPos

	        if ( abs offset.x ) > tolerance or ( abs offset.y ) > tolerance or ( abs offset.z ) > tolerance do(
	            append  movedVtx  vtx
	            append  offsetPosVtx  offset
	        )
	    )
	    ST_offsetVtxInfo  numverts:num    movedVtx:movedVtx  offsetPosVtx:offsetPosVtx
	),

	fn CopyVertexPos   n  vtxList  =(
	    local posList=#()
	    for  vtx  in  vtxList   do (
	        vtxPos =  polyop.GetVert    n.baseobject  vtx

	        append  posList  vtxPos
	    )
	    posList
	),

	fn PasteVertexPos   n  vtxList  posList  = (
	    if vtxList.count != posList.count then (
	        return false
	    )
	    for  vtx = 1  to  posList.count  do (
	        polyop.setVert  n.baseobject    vtxList[vtx]    posList[vtx]
	    )
	    true
	),

	fn TransfertVertexPos  sourceN  destN = (

	    if not isValidNode destN or  not isValidNode sourceN  then   (
	        return False
	    )

	    if  classof destN.baseObject != Editable_Poly and destN.baseObject != modpanel.getCurrentObject() then (
	        return False
	    )
	    local vtxList = ( polyop.getVertSelection  destN)  as array
	    local posList = this.CopyVertexPos  sourceN  vtxList

	    local undolabel = "RevertMorphVertex"
	    undo undolabel on(
	        this.PasteVertexPos  destN  vtxList  posList
	    )
	    true
	)
)

PolyJob = St_PolyJob()