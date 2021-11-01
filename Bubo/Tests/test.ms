	

sk = $Cylinder001.modifiers[#skin]
skinNode = $Cylinder001 
filenames = #(@"D:\Utilisateurs\Pierre\Documents\3ds Max 2020\import\Skin\skin1.xml" , @"D:\Utilisateurs\Pierre\Documents\3ds Max 2020\import\Skin\skin2.xml")
blendValues = #( 0.9 , 0.1)

bubo.mixSkin sk skinNode filenames blendValues
