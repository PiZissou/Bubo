global BuboAPI
global Bubo

macroScript Bubo
category:"tat_Setup"
buttontext:"Bubo"
tooltip:"Bubo"
(
    try (
        Bubo.ToggleUI()
    ) catch()
)

macroScript  BuboSkinW1
	category:"tat_Setup"
	buttontext:"BWeight 1"
    tooltip:"Bubo - Weight = 1"
(
	BuboAPI.SetSkinWeight  1.0  false
)

macroScript  BuboSkinWDot25
	category:"tat_Setup"
	buttontext:"BWeight .25"
    tooltip:"Bubo - Weight = .25"
(
	BuboAPI.SetSkinWeight  .25  
)

macroScript  BuboSkinWDot50
	category:"tat_Setup"
	buttontext:"BWeight .5"
    tooltip:"Bubo - Weight = .5"
(
	BuboAPI.SetSkinWeight  .5  
)

macroScript  BuboSkinWDot75
	category:"tat_Setup"
	buttontext:"BWeight .75"
    tooltip:"Bubo - Weight = .75"
(
	BuboAPI.SetSkinWeight  .75  
)

macroScript  BuboSkinW0
	category:"tat_Setup"
	buttontext:"BWeight 0"
    tooltip:"Bubo - Weight = 0"
(
	BuboAPI.SetSkinWeight  0  
)

macroScript  BuboSkinWPlus
	category:"tat_Setup"
	buttontext:"BWeight +"
    tooltip:"Bubo - Weight +"
(
	BuboAPI.SkinWeightPlus()  
)

macroScript  BuboSkinWMinus
	category:"tat_Setup"
	buttontext:"BWeight -"
    tooltip:"Bubo - Weight -"
(
	BuboAPI.SkinWeightMinus()
)

macroScript  BuboSkinHoldToggle
	category:"tat_Setup"
	buttontext:"BHoldTogg"
    tooltip:"Bubo - HoldToggle"
(
	BuboAPI.HoldBoneToggle()
)

macroScript  BuboSkinEditToggle
	category:"tat_Setup"
	buttontext:"BEditTogg"
    tooltip:"Bubo - SkinEditToggle"
(
	SkinJob.ToogleEditEnvelopes()
)

macroScript  BuboSkinShrink
	category:"tat_Setup"
	buttontext:"BShrink"
    tooltip:"Bubo - SkinShrink"
(
	BuboAPI.SkinShrink()
)

macroScript  BuboSkinGrow
	category:"tat_Setup"
	buttontext:"BGrow"
    tooltip:"Bubo - SkinGrow"
(
	BuboAPI.SkinGrow()
)

macroScript  BuboSkinRing
	category:"tat_Setup"
	buttontext:"BRing"
    tooltip:"Bubo - SkinRing"
(
	BuboAPI.SkinRing()
)

macroScript  BuboSkinLoop
	category:"tat_Setup"
	buttontext:"BLoop"
    tooltip:"Bubo - SkinLoop"
(
	BuboAPI.SkinLoop()
)

macroScript  BuboSkinNextBone
	category:"tat_Setup"
	buttontext:"BNextBone"
    tooltip:"Bubo - NextBone"
(
	BuboAPI.SkinNextBone()
)
/*
test Crash max

BuboAPI.SetSkinWeight  1.0
BuboAPI.SkinWeightPlus()
BuboAPI.SkinWeightMinus()
BuboAPI.HoldBoneToggle()
SkinJob.ToogleEditEnvelopes()
BuboAPI.SkinShrink()
BuboAPI.SkinGrow()
BuboAPI.SkinRing()
BuboAPI.SkinLoop()
BuboAPI.SkinNextBone()

*/