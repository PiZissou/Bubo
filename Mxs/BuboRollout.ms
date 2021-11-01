global Bubo
-- load the WPF Integration library needed to create the ElementHost control
dotNet.loadAssembly @"\\koro\pub\CommonDLLs\WindowsFormsIntegration.dll"
 
try ( cui.UnRegisterDialogBar BuboRollout ) catch ()
try ( DestroyDialog BuboRollout ) catch ()

rollout BuboSpinnerRollout ("Bubo Spinner Rollout")
(
	spinner c_value "" range:[0,1.0,1.0] type:#float

	on  BuboSpinnerRollout open do (
		c_value.value = 0
	)
)

rollout BuboRollout ("Bubo Rollout")
(
	dotNetControl host "Integration.ElementHost" align:#left offset:[-15,-5]
	
	local _isInitialized = false
	local _minSize = [10,10]
	local _defaultSize = [300, 480]
	
	fn SaveUI = (
		local p = (getDialogPos BuboRollout) as string
        setINISetting (getMAXIniFile()) "BuboAPI" "Pos" p
        local s =  (getDialogSize BuboRollout) as string
        setINISetting (getMAXIniFile()) "BuboAPI" "Size" s
	)

	fn LoadUI = (
		try ( cui.UnRegisterDialogBar BuboRollout ) catch ()
		local p = getINISetting (getMAXIniFile()) "BuboAPI" "Pos"
		if p != undefined do(
			try (
				p = execute p
				if classof p == Point2 then (
					setDialogPos BuboRollout p
				)		
			) catch (
				TRDLogger.LogWarning "Error LoadUI Pos"
			)
		)
		local size = _defaultSize
		local s = getINISetting (getMAXIniFile()) "BuboAPI" "Size"
		if s != undefined do (
			try (
				s = execute s
				if classof s == Point2 then (
					size = s					
				)				
			) catch (
				TRDLogger.LogWarning "Error LoadUI Size"
			)
		)
		setDialogSize BuboRollout size
		host.width = size.x
		host.height = size.y
		try (cui.RegisterDialogBar BuboRollout style:#(#cui_dock_left, #cui_dock_right, #style_resizing) minSize:_minSize) catch()
	)

	on BuboRollout open do (
		BuboAPI.UI.OnOpen()
		if not _isInitialized then (
			BuboRollout.host.child = BuboAPI.UI
			LoadUI()
			cui.RegisterDialogBar BuboRollout style:#(#cui_dock_left, #cui_dock_right, #style_resizing ) minSize:_minSize
			_isInitialized = true
		)
	)

	on BuboRollout close do (
		SaveUI()
		BuboAPI.UI.OnClose()
		if BuboAPI.UI.Parent != undefined then (
			BuboAPI.UI.Parent.Children.Remove(BuboAPI.UI)
		)
	)

	on BuboRollout resized pnt do (
		host.width = BuboRollout.width
		host.height = BuboRollout.height
	)
)

