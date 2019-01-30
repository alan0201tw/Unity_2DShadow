## Introduction

Several simple 2D shadow caster implemented with different algorithms.

1. CPU Ray-casting approach : 
   * base on the algorithm provided in Sebastian Lague's Field of view visualisation tutorial, which can be found [here](https://www.youtube.com/watch?v=rQG9aUWarwE).
   * The shadow computations are done by CPU. I think this is easier to make interactions between other objects and shadow casters.

2. GPU Ray-marching approach (in fragment) :
   * (This approach is too expensive in respect of computation time, so I abandoned this and try to do the ray-marching in vertex.)

3. GPU Ray-marching approach (in vertex) :
   * The algorithm is basically the same as the CPU one, just use ray-marching instead of ray-casting, and do this in vertex level. Otherwise it'll be too expensice.
  
4. GPU Ray-marching approach (in geometry) :
   * <To Be Implemented>

## How to use?

Please refer to the demo scenes of each approach.

For CPU, basically you can just put ShadowCaster2D on an empty GameObject and put the LightCaster2DCamera on the MainCamera.

For GPU Ray-marching, I'll describe acstractly, viewing the actual scene explains better in detail.

    1. Set up a camera for rendering obstacles into a texture (In this case I treat non-black colors as obstacle. If you have black obstacles in your scene, you should probably use "SetDisplacementShader" on the camera for rendering obstacles.)
    2. Add LightCaster2DCameraGPU to MainCamera, add ShadowCaster2DGPU to empty GameObjects.
    3. Please notice that the draw call on the MeshRenderers will be done manually in "LightCaster2DCameraGPU", so it's better to disable the instantiated MeshRenderers. (You can also edit the script and do it there.)
    4. Add the blending material and the obstacle texture to LightCaster2DCameraGPU.
    5. P.S. Remember to set the layer of obstacle objects to "Obstacle", or the layer that the obstacle camera will render.

That should be it.
