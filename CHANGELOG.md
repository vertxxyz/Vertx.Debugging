# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.0-pre.2]
### Changed
- Order of arguments in Shape.SphereCast and SphereCastAll is now consistent with Physics.SphereCast.

### Added
- Added `DrawPhysics`, identical to `Physics` but implements the appropriate `D.raw` call for each operation.
- Added `DrawPhysics2D`, identical to `Physics2D` but implements the appropriate `D.raw` call for each operation.
- Added Linecast.
- Added DashedLine.
- Added primitive Collider drawing.

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