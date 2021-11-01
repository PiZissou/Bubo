
global TatMxsModifiersLib

struct St_ProjectionJob (
	/*
    <struct>
        <description> 
        	Extends Projection MapChannel infos, 
        	Init and finalize Projection process 
        	used in ProjectDataEngine.cs
    	</description>
    </struct>
    */
	_nodeMods ,

	fn DisplayVertexColor n  onOff = (
		n.ShowVertexColors = false
		n.ShowVertexColors = onOff
	),

	fn ComputeProjectMapChannels refNode targetNode MapChannels:#(1,2,3) showResult:false = (
		try (
			if isValidNode refNode and isValidNode targetNode then (
				if classof refNode.baseObject == Editable_Poly and classof targetNode.baseObject == Editable_Poly then (
	
					local closestVertex = #()
					
					for v = 1 to targetNode.verts.count do (
						local Alldist = #()
						local targetV = polyop.getVert targetNode v  
						for i = 1 to refNode.verts.count do (
							currentdist = distance targetV (polyop.getVert refNode i )
							append Alldist currentdist
						)
						local MinDist = amin Alldist
						local closerIndex = findItem Alldist MinDist
						append closestVertex closerIndex
					)
					for currentChannel in MapChannels do (
						for v = 1 to targetNode.mesh.verts.count do (
							refNode.EditablePoly.SetSelection #Vertex #{closestVertex[v]}
							local theVertColor = refNode.getVertexColor  currentChannel
							polyop.setvertcolor targetNode  currentChannel  #{v} theVertColor
						) 
					)
					if showresult do (
						targetNode.showVertexColors = on
						targetNode.vertexColorType = 5
						targetNode.vertexColorMapChannel = amax MapChannels
					)
				) else (
					BuboLogger.LogWarning "ProjectionJob.BeginSetupNodes : Classof must be Editable_Poly"
				)
			) else (
				BuboLogger.LogWarning "ProjectionJob.BeginSetupNodes : InvalidNodes, refNode: % , targetNode: %" val:#(refNode, targetNode)
			)
		)
		catch (
			BuboLogger.LogError "ProjectionJob.ComputeProjectMapChannels : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	),

	fn ProjectMapChannels refNode targetNode  MapChannels  mappedIndices showresult = (
		try (
			for i = 1 to targetNode.mesh.verts.count do (
				refNode.EditablePoly.SetSelection #Vertex #{mappedIndices[i]}
				for map in MapChannels do (
					local col = refNode.getVertexColor map 
					polyop.setvertcolor targetNode map #{i} col
				)
			) 
			if showresult do (
				targetNode.showVertexColors = on
				targetNode.vertexColorType = 5
				targetNode.vertexColorMapChannel = amax MapChannels
			)
		)
		catch (
			BuboLogger.LogError "ProjectionJob.ProjectMapChannels : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
		)
	)
)

ProjectionJob = St_ProjectionJob()

/*
ProjectionJob.BeginSetupNodes $P_Isandre_HD (selection as array)

*/