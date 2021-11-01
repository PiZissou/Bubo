struct St_NodeMods (

	_node,
	_excludeMod,
	_excludeClass =#(),
	_modSatuts = #(),
	_mods = #(),
	fn Suspend   = (
	    _modSatuts = #()
	    _mods =  #()
	    for i = 1 to  _node.modifiers.count where findItem _excludeClass (classof _node.modifiers[i]) == 0 do (
	    	append _mods  _node.modifiers[i]
	        append _modSatuts   _node.modifiers[i].enabled
	        _node.modifiers[i].enabled = false
	    )
	    if _excludeMod != undefined then (
	   		_excludeMod.enabled = true
	    )
	),
	fn Resume = (
	    for i = 1 to  _mods.count  where _mods[i] != undefined do (
	        _mods[i].enabled = _modSatuts[i]
	    )
	    _mods = #()
	)
)