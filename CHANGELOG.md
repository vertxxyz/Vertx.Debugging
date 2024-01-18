# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [3.0.0]
### Changed
- Minimum Unity version is now 2020.3.
- Added dependencies on `com.unity.burst` and `com.unity.mathematics`.
- Only the API surface is now public. If you want to extend the package use Assembly References.
- Shapes are now pre-allocated. Change the number of allocations via the Project Settings.
- Axis colors are now in Preferences.
- `DashedLine` is now backed by `Line` instead of two Vector3s.

### Improved
- Added support for drawing from Jobs and Burst-compiled functions.

### Added
- Added `[ExecuteAlways]` to Debug Collision/Trigger Events to support visualising during `Physics.Simulate`.
- Added `RaycastHit` and `RaycastHit2D` constructors to `Shape.Ray`.
- Added `DrawPhysicsUtility.GetFixedFrameJobDuration` for getting a corrected duration for jobs started from `FixedUpdate` or `FixedStepSimulationSystemGroup`.

### Fixed
- Fixed `D.raw(MeshCollider)` not respecting `color` and `duration`.

### Removed
- Removed the internal `CircleCache` class. If your extensions relied on it, sorry, it was removed for burst compatibility.

## [2.3.0]
### Added
- Added `Shape.FieldOfView`.
- Added a `Flip` function to `Shape.Cone` and `Pyramid`.

### Fixed
- Added missing axis support for CapsuleCollider drawing.
- Fixed incorrect calculations for Horizontal CapsuleCollider2D drawing.

## [2.2.0]
### Added
- Added PolygonCollider2D support to Shape drawing.
- Added `Shape.Cone`.
- Added `Shape.Frustum`.
- Added `Shape.Pyramid`.

### Improved
-  Box Matrix4x4 constructor is now public.

## [2.1.4]
### Added
- Added `Shape.Annulus`.

### Fixed
- Fixed rare introduction of NaN into Capsule2D code paths.

## [2.1.3]
### Improved
- Improved handling of NaN in Arc code paths. This will set reasonable defaults when invalid values are passed.

## [2.1.2]
### Fixed
- Removed NaNs from certain code paths when zero is used as a direction.

## [2.1.1]
### Added
- Scale parameter for Axis2D.

### Fixed
- Null reference exceptions present in Physics2D cast drawing.

## [2.1.0]
### Added
- Added `Shape.Plane`.
- Added `BoundsInt`, `RectInt`, and `Ray2D` overloads.
- Added `DrawPhysicsSettings.ResetDuration`.

### Fixed
- DrawPhysics2D being undefined for versions below 2022.1

## [2.0.6]
- Attempted fix for OpenUPM gitignore discrepancy that has caused a meta file ignore disconnect between git and the package repo.  
  This file is tracked in git, but is not present in the package repo for unknown reasons.

## [2.0.5]
### Note
- If you are seeing an error about a folder not having a meta file, right-click on the Debugging folder, open it in Explorer and delete the Assets folder.  
  The folder should be empty. (Project\Library\PackageCache\com.vertx.debugging@version\Assets) if Unity has also generated a meta file, delete that too.

### Added
- Added missing autoTiling, edgeRadius, and offset support to BoxCollider2D drawing.

### Fixed
- Fixed Z offset issues with nested rotations and scales and 2D Collider drawing.

## [2.0.1 to 2.0.4]
### Fixed
- Native collection disposal in certain scenarios where no shapes are being drawn.
- Removed stray meta file.
### Added
- Added `DrawPhysicsSettings.Duration` and `DrawPhysicsSettings.SetDuration` to override the length of time the casts draw for. You will need to reset this value manually.
  Calls to `Duration` cannot be stripped, I would recommend using `SetDuration` if this is important to you.

## [2.0.0]
### Fixed
- Fixed UnityException when using Shape colors from field initialisers, like DebugCollisionEvents does.
- Removed obsolete calls 2023+ projects that use Physics2D.

### Added
- Added Arc overload that takes a chord, arc length, and aiming vector.

### Improved
- DebugMeshNormals doesn't rely on ReadWrite meshes for 2020+

## [2.0.0-pre.6]
### Fixed
- Fixed issue where lines would fail to render if they were the only type of shape rendering.

## [2.0.0-pre.5]
### Fixed
- Fixed shape drawing from `Awake` or `Start` failing to register with duration.
- Added missing duration-only overload to `D.raw`.

## [2.0.0-pre.4]
### Fixed
- Fixed compatibility issues with 2022.1.

## [2.0.0-pre.3]
### Fixed
- Fixed compatibility issues with projects not using 2D Physics.

## [2.0.0-pre.2]
### Changed
- Order of arguments in Shape.SphereCast and SphereCastAll is now consistent with Physics.SphereCast.
- Order of arguments in Shape.CircleCast and CircleCastAll is now consistent with Physics2D.CircleCast.
- 2D casts now support minDepth and maxDepth instead of one optional depth.

### Added
- Added `DrawPhysics`, identical to `Physics` but implements the appropriate `D.raw` call for each operation.
- Added `DrawPhysics2D`, identical to `Physics2D` but implements the appropriate `D.raw` call for each operation.
- Added Linecast and Linecast2D.
- Added DashedLine.
- Added primitive Collider and Collider2D drawing.

### Fixed
- Capsule outlines no longer break when drawing around the origin.

### Known Issues
- See 2.0.0-pre.1

## [2.0.0-pre.1]
### Added, changed, improved
- **Complete API overhaul**, nothing is as it was previously! See README for more information.
  - The core drawing function is `D.raw(shape, color = Color.white, duration = 0)`
  - All drawable shapes are structs inside the `Shape` class.
- Greatly improved drawing performance.
  - Drawing no longer uses Debug.DrawLine or Gizmos.DrawLine internally.
  - Everything except text is now using instanced rendering.
- Improved drawing of basic shapes.
  - Spheres and capsules have a proper outline.
  - The interior of shapes is faded to better show 3D.
  - Casts have accurate outlines and better terminations.
  - Text now supports background and foreground colors.
  - Text now responds to Gizmos' "3D Icons" setting (only interpreted as distance fade).
  - 3D Text is now depth sorted.
- Added Arc and Arc2D.
- Added Hemisphere.
- Added ScreenText, draws text in the top left of the view, uses an Overlay in the scene view. It's optional, you can also dock it if it's distracting.
- Added more debug components. Find them in Add Component > Debugging.
- Removed complex shape parameters.
  - Specifying segment count is no longer available. If you need this, please let me know.
- Removed GizmosScope. This should be automatically detected.

### Known Issues
- Shape outlines become inaccurate at small sizes.

The two issues below can be remedied by playing with the Depth settings in *Project Settings>Vertx>Debugging*:
- 2022.2 URP versions may have a depth buffer flipped in Y in the Game view. This is a Unity bug that will also affect built-in gizmos.
- Some versions of Unity will not depth test in the Game view. This seems to be consistent with built-in Debug and Gizmo drawing.
  Fixes welcome if anyone has them!

## [1.9.2] - 2022-04-26
- Added IList/IEnumerable support for DrawLine2D and DrawArrowLine2D.
- Minor performance improvements to circle drawing.
- Fixed incomplete DebugCollisionEvents.

## [1.9.1] - 2022-04-11
- Removed Vertx from debug component menu.
- Added more debug components:
    - Debug Collision Events
    - Debug Trigger Events
- Fixed 2022-only API being present in earlier versions.

## [1.9.0] - 2022-04-07
- Added IList/IEnumerable support for DrawLine and DrawArrowLine.
- Added DrawMeshNormals.
- Added Debug components:
    - Debug Mesh Normals.
    - Debug Renderer Bounds.
    - Debug Transform.
- Changed DrawAxis to take an Axes enum that defines which axes are drawn.

## [1.8.2] - 2022-02-12
- Fix for gizmos scope failing to initialise properly.

## [1.8.1] - 2022-02-18
- Physics2D module is now optional.

## [1.8.0] - 2022-02-05
- Added duration parameter to most remaining methods.
- Added DrawHit variant of methods for casts with only one result.
- Renamed DrawSpiral to DrawSpiral2D.
- Reduced allocations.

## [1.7.1] - 2021-08-06
- Fixed alignment issue with DrawText on systems with scaling or retina displays.

## [1.7.0] - 2021-06-21
- DrawText uses a monospaced font.
- DrawText properly respects gizmo settings in all windows.
- Changed a constructor for Capsule to handle height consistent with a CapsuleCollider.

## [1.6.2] - 2021-06-01
- Added missing duration to DrawArrow.
- Added DrawArrowLine.

## [1.6.1] - 2021-04-28
- Added DrawArc.

## [1.6.0] - 2021-04-16
- Fixed exception when drawing Text while the game view is not open or hidden by a maximised scene view.
- Added duration to many 2D debugging methods.
- Added DrawSpiral (helpful for debugging fast rotating objects).

## [1.5.2] - 2021-01-25
- Added DrawCircle.
- Added DrawGizmosScope for use with OnDrawGizmos.

## [1.5.1] - 2021-01-22
- Added DrawBounds.
- Added DrawRect.
- Added Ray only overload for DrawRaycast

## [1.5.0] - 2020-10-25
- Added duration parameters to a few methods.
- Added a scale parameter to DrawAxis.

## [1.4.3] - 2020-10-18
- Added Game-View support for DrawText.

## [1.4.2] - 2020-10-17
- Added Editor-only conditional attribute to all debug functions.

## [1.4.1] - 2020-10-11
- Added DrawArrow.

## [1.4.0] - 2020-09-25
- Added DrawRaycast.
- Added raw shape drawing.
- Previous detailed shape drawing has been made internal in favour of the new functions.

## [1.3.0] - 2020-09-20
- Added combination DrawRaycast and DrawRaycast2D functions.
- Added DrawRaycastHits2D.
- Added more Color-based overloads.

## [1.2.0] - 2020-09-19
- Fixed naming issue with "Draw"BoxCast2D.
- Added code generation to add automatic Color-free method overloads.

## [1.1.3] - 2020-09-15
- Added cast lines for 2D Draw functions.
- Added simple variants for all Draw functions that combine drawing the cast and hits.

## [1.1.2] - 2020-09-13
- Added 2D variants of the DebugUtils.Draw functions.

## [1.1.1] - 2020-09-12
- Added DebugUtils.DrawCapsuleCast and DrawCapsuleCastHits.
- Added some variants for DrawSphereCast functions.

## [1.0.0]
- Initial release.