## Introduction

A very simple 2D shadow caster implemented base on the algorithm provided in Sebastian Lague's Field of view visualisation tutorial, which can be found [here](https://www.youtube.com/watch?v=rQG9aUWarwE).

The shadow computations are done by CPU. I think this is easier to make interactions between other objects and shadow casters.

## How to use?

1. Add "Editor" and "Scripts" directior to your project.
2. Add "ShadowCaster2D" to any gameObject.
3. Set up parameters and most importantly, the "Obstable Mask" field.
4. Profit!

## Current Performance

1. 
   * 64 ShadowCaster2D
   * sampleCount = 120
   * angle range from 70 to 180
   * Less than 10 Obstacles
   * around 40 to 50 FPS

2. 
   * Doubling the number of ShadowCaster2D in testcase 1 
   * 128 ShadowCaster2D
   * Less than 30 FPS

3. 
   * 64 ShadowCaster2D
   * sampleCount = 120
   * angle range from 70 to 180
   * 28 Obstacles
   * around 30 to 40 FPS

## Furute work

1. Maybe add an optional GPU implementation?
   * Algorithm unknown, need to find a valid solution.

2. Add optional static option?
   * Options to manually update shadowMesh, rather than updating it every frame.
   * For example in games like Deadbolt, shadowMesh(s) only need to be updated when a door is opened...etc.
   * Maybe use an Event-based update method?
     * Update the mesh only when other MonoBehaviors are changed.