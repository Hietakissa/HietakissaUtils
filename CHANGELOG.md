# Changelog

## WIP

- Console/Command System Rework



## [1.3.4] - 26.05.2024

### New Features
- ObjectPool class found in HietakissaUtils.Pooling



## [1.3.3] - 23.05.2024

### New Features

- CameraShakeSource, used for playing looping/positional camera shakes. Affects all CameraShakers in radius

### Changed

- SoundContainer Clips are now automatically named according to the clip name
- SoundContainer mode is now Shuffle by default
- ConditionalField attribute can now display based on if the given UnityEngine.Object is null, displays if reference is not null. WIP



## [1.3.2] - 20.05.2024

### New Features

- HKTicker class to create tickers that tick at a given rate. Automatically cached per-delay and each delay has multiple batches to even out the load
- SoundContainer ScriptableObject to easily add sounds with variation and randomization
- Maf.Easing class with a bunch of easing functions
- HKTransformTool for Blender-like editing. WIP
- CameraShaker shakes a given Transform based on the playing shakes. Shakes defined as configurable ScriptableObjects

### Reworked

- HKPhysicsSimTool now has proper playback options, configurable ticks per keyframe and full support for ProBuilder and MeshColliders. WIP
- LootTable, now found in HietakissaUtils.LootTable, way better performance with better runtime usability


### Added

- Vector2/2.Mult/Div/Add/Sub X/Y/Z extension methods
- Vector3.SetXYZ extension method
- Generic Component.CopyTo(Component) extension method
- Shuffle extension method for Lists and Arrays
- AudioSource.GetMaxClipLength to calculate the maximum running time of the AudioSource based on the clip's length and pitch for use with the SoundContainer
- Maf.GetRandomRotation and Maf.GetRandomDirection functions
- QOL.Quit method, closes the game in a build, stops playing in the editor
- QOL.Destroy(GameObject) method to destroy a given GameObject with the correct destroy method
- QOL.Log(object) method as an editor-only wrapper to Debug.Log(object), gets stripped from builds

### Removed

- Vector2/3.Average functions

### Changed

- ConditionalField attribute now accepts Enums
- DestroyChildren extension methods also work with GameObjects, also added DestroyChildrenAuto to automatically use the correct destroy method