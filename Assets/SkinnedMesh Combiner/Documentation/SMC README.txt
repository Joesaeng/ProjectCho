  ______   __        _                                 __         ____    ____               __       
.' ____ \ [  |  _   (_)                               |  ]       |_   \  /   _|             [  |      
| (___ \_| | | / ]  __   _ .--.   _ .--.  .---.   .--.| |          |   \/   |  .---.  .--.   | |--.   
 _.____`.  | '' <  [  | [ `.-. | [ `.-. |/ /__\\/ /'`\' |          | |\  /| | / /__\\( (`\]  | .-. |  
| \____) | | |`\ \  | |  | | | |  | | | || \__.,| \__/  |         _| |_\/_| |_| \__., `'.'.  | | | |  
 \______.'[__|  \_][___][___||__][___||__]'.__.' '.__.;__]       |_____||_____|'.__.'[\__) )[___]|__] 
                    ______                       __         _                                         
                  .' ___  |                     [  |       (_)                                        
                 / .'   \_|  .--.   _ .--..--.   | |.--.   __   _ .--.  .---.  _ .--.                 
                 | |       / .'`\ \[ `.-. .-. |  | '/'`\ \[  | [ `.-. |/ /__\\[ `/'`\]                
                 \ `.___.'\| \__. | | | | | | |  |  \__/ | | |  | | | || \__., | |                    
                  `.____ .' '.__.' [___||__||__][__;.__.' [___][___||__]'.__.'[___]                   
                                                                                                      

This documentation is available online at https://sites.google.com/view/awtdev/skinned-mesh-combiner

If you have any questons, comments or concerns you can reach me at awtdevcontact@gmail.com

Feel free to use any of the included demo models for any purpose. Also feel free to modify and change any part of the
SkinnedMeshCombiner source code to suit your needs, that's what I intended it to be for :)

---- HOW TO USE ----
    - Attach a SkinnedMeshCombiner component to a parent of the SkinnedMeshRenderers / MeshRenders you wish to combine
    - Generally the auto-detect feature can find and assign the appropriate meshes and settings, so press that
    - Otherwise, you can manually assign / change any settings and reorder the collection of meshes as needed
    - Click combine!

---- NOTES ----
    - ALL meshes and textures involved in combining MUST have Read/Write enabled. For the model, it's under the first
    'Model' tab. For textures it's under the 'Advanced' foldout. If it isn't enabled, the SMC will yell at you for it
    and ignore the mesh / texture.

    - The first Renderer in the renderers to combine collection will define the following things about the combined result:
        * Texture Resolution
        * Armature (discounting any virtual bones)
        * Material instance (will be assigned a combined texture if applicable)

    - All textures for each mesh, if you wish them to be combined, must share the same dimensions. As mentioned, this
    dimension is set by the first processed renderer. For example, if the first texture processed has a dimension of 256x256,
    the texture combiner will expect all the others to be as well, so they can fit onto a uniform texture atlas.

    - As it's set up, there's a limit on how many textures can be combined into an atlas. I set it to 16, so a 4x4 grid.
    With a texture resolution of 256x256, this creates a texture of 1024x1024. If you need more textures, you can change
    this texture resolution variable in the TextureCombiner script.

    - The texture atlas will create an atlas from the textures it finds in each renderer's material, from left to right,
    bottom-up. Though it usually has no effect, if you need to control the order of textures it is based on the order of
    renderers in the component renderers to combine collection. So, the first renderer's texture will be at the bottom
    leftmost square on the atlas (0, 0) then the next will be (1, 0) and so forth.

    - Textures are expected to be uniform in size, ex. 256x256, 512x512, 1024x1024...

    - When combining meshes onto an armature, the "create virtual bones" option will create new bone transforms as needed
    to map meshes without any bone weights onto. For example, a sword being combined onto a character mesh will use its
    transform as a bone for skinning. This allows you to move the bone around as you wish and even disconnect it from the
    armature. HOWEVER, mesh skinning is a fairly costly operation so this isn't practical for lots of virtual bones. If
    the setting is off, the mesh will do its best to bind to any parent bone you have it under.

    - If you need to create a virtual bone for a specific object but not others:
        * Turn off "Create Virtual Bones"
        * Keep the meshes that you don't need to move dynamically as children of the armature
        * Take the thing you want to operate as a virtual bone out from under the armature entirely
        * Combine the meshes
        * Move that gameobject back under the armature parent you want it to follow
    This essentially creates a virtual bone, but only for that object.

    - Only a single blendshape (shape key) frame is supported. In blender this is in the range (0-1), and (0-100) in the unity
    editor. What this means is just that if you have a shape key going from (-1 to 1) or (0-200) I don't know what will
    happen.

    - This tool is ideally intended to be used to combine SMRs with eachother or SMRs with MRs. Unity provides a built-in
    method for combining regular meshes (no skinning, boneweights or blendshapes) which is Mesh.CombineMeshes(), so if
    that's what you need I recommend using that.


This is a pretty advanced application of SkinnedMeshRenderers, and I can't predict every use case, so if you have any
issues feel free to get in touch with me and I'll see if I can help figure it out.