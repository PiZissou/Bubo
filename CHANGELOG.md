## VERSIONS

### 0.2.2
- Add : Externalize config

### 0.2.1
- Add : 3dsmax 2022

### 0.1.9
- Fix : Extract ProgressiveMorph debug
- Add : DataProjectionMorph accept pattern in channelnames

### 0.1.8
- Fix : DataProjectionMorph

### 0.1.7
- Add : LoadSkin in API option onlySelected and input vertices list
- Add : Fix WrapSkin (select modifier) and get existing skinwrap if there is one

### 0.1.6
- Add : Api DataProjection

### 0.1.5
- Fix : loading Error 
- Fix : Bug on mixMorpher

### 0.1.4
- Add : SkinJobs.HoldBones
- Fix : Restore originColor where lost on unHold

### 0.1.3
- Add : deploy BuboDock.py for docking managment | window visibility and name, used in IBubo
- Fix : debug auto open UI on 3dsmax start 
- Fix : Rename IBubo.ms to __Bubo__.ms  
- Remove : unused BuboRollout.ms
- Add : Morpher, resetVertexSelection
- Fix : DisplyaHoldedBones only when editEnvelopes on

### 0.1.2.b
- Fix : MirroMorph do not delete existing mirror script controller 
- Add : BuboDock.py for docking managment (not deployed)
- Fix : [BuboRollout] change dll path @"\\koro\pub\CommonDLLs\WindowsFormsIntegration.dll"

### 0.1.2.a
- Fix : Sync selection envelopes
- Fix : When start on morpher modifier open

### 0.1.2
- Update : BuboMacros missing()

### 0.1.1.e
- Update : Rename versionning never deployed
- Update : Morpher, disable copyMirrorController option on MirrorMorph
- Add : Morpher, When Transform changed udpateUI

### 0.1.1.d (commit 0.1.5)
- Update : Skin, debug macros cash when bubo is closed
- Update : Skin, improve syn selection
- Update : Morpher, Clamp value under 0.0 

### 0.1.1.c (commit 0.1.4)
- Update : MorphJob disable mirrorController option
- Update : blendColorScript on SpinnerControl to green
- Add 	: Morpher, Current percent SetTo100 

### 0.1.1.b (commit 0.1.3)
- Add : blend color on SpinnerControl
- Update : SkinJob.ChangeBoneToBoneInfluence to Bubo.ReplaceBones
- Update : SkinJob.GetSkinObjectsSSK to Bubo.GetSkinBones

### 0.1.1.a (commit 0.1.2)
- Add : SkinJob.ChangeBoneToBoneInfluence and SkinJob.GetSkinObjectsSSK

### 0.1.1
- Update : macros tooltip

### 0.1.0
- Fix : display vertex or shade is no longer lost when user select bone or vertex
- Fix : ProjectionData does not disable morpher anymore during skin process
- Fix : Always turn viewport display to material on bubo closing

### 0.0.19c
- change colors to 3dsmax style
- button color change on mouseOver
- Enhance : ( Wip ) Increase width  of spinner in MorphEngine
- Add : RemoveZeroWeights 0.001  before RemoveUnusedBones

### 0.0.19b
- Add : BuboUI, add 2 RadioButtons in SkinTab for "draw_vertices" and "shadeweights" 
- Fix : Broken MirrorNode
- Enhance : Move RadioButtons "draw_vertices" and "shadeweights" to Display Expander
- Fix : LoadMorphChannels work with desabled morpher
- Add : XAttribute for  MaxfileName in Save Skin/Morph.Xml
- Enhance : ( Wip ) Increase width  of spinner in MorphEngine

### 0.0.19a
- Add : ResetPos operate on frame zero
- Add : geo.showVertexColors disabled when envelopes == false, change modifier or close Bubo
- Add : If current skin is "Skin (local)" then disable "draw_vertices" and "shadeweights" on other skin, else all enabled.

### 0.0.19
- Add : Undo/Redo SetSkinWeights
- Fix : Patch buboSkinWeight -
- Add : ResetPos operate on frame zero
- Add : geo.showVertexColors disabled when envelopes == false, change modifier or close Bubo
- Add : If current skin is "Skin (local)" then disable "draw_vertices" and "shadeweights" on other skin, else all enabled.

### 0.0.18f
- Add : SkinEngine, disable Layer Configuration when UnholdOnly is Checked + refresh List with hold/unhold SkinItems

### 0.0.18e
- Fix : FindChannelIndex in c# ExecutMxs
- Fix : Load from ChannelNames matchlist if is not empty
- Add : MixMorphChannels to mix severals files
- Add : MixMorphChannels, case of channel is not present in all Loaded files  
- Add : Display = #Shaded on bubo closing
- Remove: ShowRollout, CloseRollout
- Fix : QueryBox for MirrorMorph controller Broken 
- Fix : ExtractMorph keep value of selected channels
- Fix : MorphEngine.InMod debug unwanted deselection itmes in treeview

### 0.0.18d Skin Debug
- Fix : Replace Skin On Selection
- Fix : ProjectionData Skin && duatQ

### 0.0.18c Skin Debug
- Fix : Replace Skin On Selection
- Add : EditEnveloppe = display object or shaded

### 0.0.18b
- Fix : desynchro redrawUI
- Add : SaveMorphChannels in API
- Add : LoadMorphChannels in API
- Fix : refresh Deleted Nodes in skinUI and MorphUI (desable startMute in linkMax)

### 0.0.18a
- Fix : save / load Skin BoneListdualQ -> override BoneNList in SkinMod

### 0.0.18
- Fix : mixskin dualQ

### 0.0.17
- Add : Save / Load Skin in API

### 0.0.16
- Add : MorphJobs.GetChannelByTarget

### Wip-0.0.15
## Skin
- Fix : MixSkin with differents basenames
- Fix : Weights Apply 50 % 
## DataProjection
- Fix : Transfert Morph  
## Interface
- Add : RightClick Rename Item
- Add : ExcludePattern in independent list


### 0.0.14f
## Skin
- Add : Right Click select boneNodes
- Improve : Select Bone => Edit envelopes On
- Improve : Next Bone => Edit envelopes On
## DataProjection
- Add : Select && showVertexColor
## Interface
- Fix : Pair Item Background Color
- Fix : Left shift ignored for weightItemTab selection

### 0.0.14e
## Skin
- Add : Hold / Unhold on weigthItem
- Add : Quat Value visible in Ui when editDQModeEnabled
- Add : Hotkey for Edit Envelopes
- Fix : Mirror DualQuat
- Fix : Remove unused bones now refreshUI
- Fix : HoldToggle on hotkey works

### 0.0.14d
## Interface
- Add : Open Config Xml
- Add : Refresh
## Settings
- Add : Smart Mode
- Add : TimeRefresh


### 0.0.14c
## Morph
- Fix : Text edit on Morph Value
- Fix : StringFormat Morph value : N1
- Add : Delete Morpher ( Right Click)
- Add : AddRegisterTimeChanged
- Add : Open MorphTargetBuilder Button
## Skin
- Fix : Refresh weights
- Improve : AutoLoad skin on Edit envelopes 
- Improve : disable select envelopes
- Add : BuboMarcos, with macros for skinning shortcuts
	BuboSkinWDot25
	BuboSkinWDot50
	BuboSkinWDot75

### 0.0.14b
- Add : Vertices WeightsItems UI
        Synchro selection with treeview

### 0.0.14a
- Improve : all Bubo Macros are compatible with UI closed
- Add : BuboMarcos, with macros for skinning shortcuts :
	BuboNextBone

### 0.0.14
- Fix : Add Bone refreshUI
- Fix : debug Modifier Selection loop on skin modifier
- Fix : Rename Bone refreshUI
- Add : GetTextUI control, use to change weight and DQ weight 

### 0.0.13
- Fix : Copy du fichier BuboCui
- Add : BuboMarcos, with macros for skinning shortcuts
	BuboWeight = 1
	BuboWeight = 0
	BuboWeight +
	BuboWeight -
	BuboHoldToggle
	BuboSkinShrink
	BuboSkinGrow
	BuboSkinRing
	BuboSkinLoop
- Improve : BlendWeight MaxStr limit = 2

### 0.0.12
- Fix : AddWeight no effect
- Add : DQ fonctions Weight+, Weight-, SetWeight
- Add : Remove Zero Weights
- Add : Edit Envelopes ( ToggbleButton )
- Add : Init Skin 
- Add : Scale Weights ( no compatibility with Hold Unhold)
- Add : Shrink, Grow, Ring, loop Selection
- Imporve : Background & borded color coherence
- Improve: SpinnerControl min, max and moveMouse event with small values

### 0.0.11
- Fix : Debug Bubo open on lunch 3dsmax
