A Unity package with a collection of various miscellaneous features and tools.
This package is currently mainly developed for my own needs, but if you're interested in something in it feel free to use it.

Should be compatible at least down to Unity 2021.3, possibly older, but no promises.
Everything is constantly WIP, so things might occasionally break/change between versions. Changes are documented extensively in the [Changelog](CHANGELOG.md)

To install simply open the Unity Package Manager, and 'Add package from git URL...' with https://github.com/Hietakissa/HietakissaUtils.git

## Features

* Debug Console and Command system (found in the Samples, WIP)
* Simple prototyping scene for testing out character movement (found in the Samples, WIP)
* InputWrapper for generating a (imo) more complete Input class from an InputActions asset without any boilerplate (WIP)
* Simple input rebinding system for the legacy input system - this'll probably be deprecated and removed at some point
* Camera shake system with somewhat customizable shakes (will refactor in the distant future)
* LootTable, weight-based random distribution using the alias method
* SceneReference, serializable reference for Scene assets, can be used for scene loading, uses a string path under the hood
* SoundContainer for more easier variation and randomization for AudioClips, newer versions now have the Audio Random Container, so I'll either update this at some point or just keep it for the older versions
* HKTool system for creating simple editor windows in a centralized place, with a few built-in tools:
  * FavoritesTool, save assets and scene objects for quick access
  * AlignerTool, will be used for aligning objects in a multitude of ways, WIP
  * PhysicsSimulatorTool, allows you to simulate physics for a selection of objects and apply the simulation for physics-based placing of objects, WIP
  * UIDebuggerTool, show raycastable UI elements
* TransformTool, WIP but somewhat functional (UX is not the best and some of the scaling math is off when restricting axis), brings the familiar transform controls from Blender into Unity. GRS for Grab/Rotate/Scale, restrict operation to XYZ world/local axis, type out values
* Some field attributes
* A bunch of Math, Extension and QOL methods/functions
* Something other stuff that I missed or if I just forget to update this list later