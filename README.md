# Debugging
Editor debugging utilities for Unity.  
All of these methods are accessed through `DebugUtils.`.

https://user-images.githubusercontent.com/21963717/153703387-cc55e3c6-26b6-4474-815a-0e65e27a73f0.mov


## General
- **DrawText**  
Draws a label in the scene view at the provided position

## 3D
### Shapes
- **DrawSphere**
- **DrawBox**
- **DrawCapsule**
- **DrawBounds**
- **DrawSurfacePoint**  
Draws a ray with a circle to indicate the surface
- **DrawPoint**  
Draws a point without specifying direction
- **DrawAxis**  
Draws an XYZ direction gizmo
- **DrawArrow**  
Draws an arrow gizmo for a vector
### Casts
- **DrawRaycast**, (**DrawRaycastHits**)
- **DrawSphereCast**, (**DrawSphereCastHits**)
- **DrawBoxCast**, (**DrawBoxCastHits**)
- **DrawCapsuleCast**, (**DrawCapsuleCastHits**)

## 2D
### Shapes
- **DrawCircle2D**
- **DrawBox2D**
- **DrawArea2D**
- **DrawCapsule2D**
- **DrawRect**
- **DrawPoint2D**  
Draws a point without specifying direction
- **DrawAxis2D**  
Draws an XY direction gizmo
- **DrawArrow2D**  
Draws an arrow gizmo for a vector
### Casts
- **DrawRaycast2D**, (**DrawRaycast2DHits**)
- **DrawCircleCast2D**, (**DrawCircleCast2DHits**)
- **DrawBoxCast2D**, (**DrawBoxCast2DHits**)
- **DrawCapsuleCast2D**, (**DrawCapsuleCast2DHits**)

## Gizmos
- **GameViewGizmosEnabled**  
`bool`: whether Gizmos are enabled in the Game view.
- **DrawGizmosScope**  
`using (DebugUtils.DrawGizmosScope())`: allows the use of other `DebugUtils` methods inside of OnDrawGizmos methods.

## Installation

<details>
<summary>Add from OpenUPM <em>| via scoped registry, recommended</em></summary>

This package is available on OpenUPM: https://openupm.com/packages/com.vertx.debugging

To add it the package to your project:

- open `Edit/Project Settings/Package Manager`
- add a new Scoped Registry:
  ```
  Name: OpenUPM
  URL:  https://package.openupm.com/
  Scope(s): com.vertx
  ```
- click <kbd>Save</kbd>
- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `com.vertx.debugging`
- click <kbd>Add</kbd>
</details>

<details>
<summary>Add from GitHub | <em>not recommended, no updates through UPM</em></summary>

You can also add it directly from GitHub on Unity 2019.4+. Note that you won't be able to receive updates through Package Manager this way, you'll have to update manually.

- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `https://github.com/vertxxyz/Vertx.Debugging.git`
- click <kbd>Add</kbd>  
  **or**
- Edit your `manifest.json` file to contain `"com.vertx.debugging": "https://github.com/vertxxyz/Vertx.Debugging.git"`,

To update the package with new changes, remove the lock from the `packages-lock.json` file.
</details>
