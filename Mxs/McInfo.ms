
global TatMxlLibObjectNames

struct  ST_McInfo
(
    _index =1 ,
    _name = "",
    _tension = 0.0 ,
    _controller = undefined,
    _progCount = 0,
    _weightTab = #(),
    _targetTab = #(),
    _targetTMTab = #(),
    _IsTargetExist = false,
    _TargetRebuilded = false,
    _UseLimits = true,
    _LimitMIN = 0.0,
    _LimitMAX = 100.0,
    _mirrorInfo = undefined,
    _morpherMod = undefined,

    fn CloneMirror  =(
        try(
            dest = ST_McInfo()
            dest._index = _index
            dest._name = TatMxlLibObjectNames.ToMirrorName _name
            dest._tension = _tension
            dest._progCount = _progCount
            dest._weightTab = _weightTab
            dest._targetTab = #()
            dest._targetTMTab = _targetTMTab
            dest._IsTargetExist = _IsTargetExist
            dest._TargetRebuilded = _TargetRebuilded
            dest._UseLimits = _UseLimits
            dest._LimitMIN = _LimitMIN
            dest._LimitMAX = _LimitMAX
            dest._mirrorInfo = this
            dest._morpherMod = _morpherMod

            if  _controller != undefined  then (
                dest._controller = copy _controller
            ) 
            local inverseX = scaleMatrix  [-1,1,1]

            for i = 1 to _targetTab.Count do(
                local target = _targetTab[i]
                copyTarget = copy  target
                copyTarget.name =  TatMxlLibObjectNames.ToMirrorName target.name
                local tm = target.transform * inverseX
                copyTarget.pos = tm.translation
                copyTarget.isHidden =  target.isHidden
                append  dest._targetTab copyTarget
            )
            return dest
        ) 
        catch (
            BuboLogger.LogError "McInfo.CloneMirror : % %" val:#(getCurrentException() , getCurrentExceptionStackTrace() )
            return ST_McInfo()
        )
    ),

    fn Clone  =(
        dest = ST_McInfo()
        dest._index = _index
        dest._name = _name
        dest._tension = _tension
        dest._progCount = _progCount
        dest._weightTab = _weightTab
        dest._targetTab = _targetTab
        dest._targetTMTab = _targetTMTab
        dest._IsTargetExist = _IsTargetExist
        dest._TargetRebuilded = _TargetRebuilded
        dest._controller =  _controller
        dest._UseLimits = _UseLimits
        dest._LimitMIN = _LimitMIN
        dest._LimitMAX = _LimitMAX
        dest._mirrorInfo = _mirrorInfo
        dest._morpherMod = _morpherMod
        dest
    ),

    fn ReplaceTarget  nodeList  deleteOld:false = (
        if nodeList.count > 0 then(
            if deleteOld then(
                delete  _targetTab
            )
            _targetTab = nodeList
            _IsTargetExist = true
        )
    ),

    fn GetOldTargets = (

        local olds = #()
        for target in _targetTab do(

            local sames =  getNodeByName target.name  all:true
            for  obj in sames where obj != target do(
                append olds  obj
            )
        )
        olds
    ),
    
    fn DeleteRebuilded = (
        if _TargetRebuilded then (
            for  i = 1 to _progCount  where isValidNode _targetTab[i] do (
                delete _targetTab[i]
            )
        )
    )
)
