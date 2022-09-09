# Debugging
Editor debugging utilities for Unity.  
All of these methods are accessed through `DebugUtils.`.

> **Warning**  
> Unity 2019.4+

https://user-images.githubusercontent.com/21963717/153703387-cc55e3c6-26b6-4474-815a-0e65e27a73f0.mov

> **Note**  
> Calls to these methods are stripped when building. You do not have to remove or use defines.
> Debugging from jobs and builds is not supported, I would recommend [Aline](http://arongranberg.com/aline/) if you need that functionality.

## General
| Name       | Description                                               |
|------------|-----------------------------------------------------------|
| `DrawText` | Draws a label in the scene view at the provided position. |


## 3D
### Shapes
| Name                                                          | Description                                                        |
|---------------------------------------------------------------|--------------------------------------------------------------------|
| `DrawSphere`<br/>`DrawBox`<br/>`DrawCapsule`<br/>`DrawBounds` | Draws a shape at the provided position.                            |
| `DrawSurfacePoint`                                            | Draws a ray with a circle to indicate the surface.                 |
| `DrawPoint`                                                   | Draws a point without specifying direction.                        |
| `DrawAxis`                                                    | Draws an XYZ direction gizmo.                                      |
| `DrawArrow`                                                   | Draws an arrow gizmo for a vector. +Support for IList/IEnumerable. |
| `DrawLine`                                                    | +Support for IList/IEnumerable.                                    |


### Casts
| Name                                   | Description                                                                     |
|----------------------------------------|---------------------------------------------------------------------------------|
| `DrawRaycast`                          | Draws a `Ray` using similar parameters as `Physics.Raycast`.                    |
| `DrawSphereCast`                       | Draws using similar parameters as `Physics.SphereCast`.                         |
| `DrawBoxCast`                          | Draws using similar parameters as `Physics.BoxCast`.                            |
| `DrawCapsuleCast`                      | Draws using similar parameters as `Physics.CapsuleCast`.                        |
| `DrawRaycastHit`<br/>`DrawRaycastHits` | Draws `RaycastHit` and `RaycastHit[]`.                                          |
| `DrawSphereCastHits`                   | Draws `RaycastHit[]` results using similar parameters as `Physics.SphereCast`.  |
| `DrawBoxCastHits`                      | Draws `RaycastHit[]` results using similar parameters as `Physics.BoxCast`.     |
| `DrawCapsuleCastHits`                  | Draws `RaycastHit[]` results using similar parameters as `Physics.CapsuleCast`. |

## 2D
### Shapes
| Name                                                                               | Description                                                        |
|------------------------------------------------------------------------------------|--------------------------------------------------------------------|
| `DrawCircle2D`<br/>`DrawBox2D`<br/>`DrawArea2D`<br/>`DrawCapsule2D`<br/>`DrawRect` | Draws a shape at the provided position.                            |
| `DrawPoint2D`                                                                      | Draws a point without specifying direction.                        |
| `DrawAxis2D`                                                                       | Draws an XY direction gizmo.                                       |
| `DrawArrow2D`                                                                      | Draws an arrow gizmo for a vector. +Support for IList/IEnumerable. |

### Casts
| Name                                       | Description                                                                         |
|--------------------------------------------|-------------------------------------------------------------------------------------|
| `DrawRaycast2D`                            | Draws a `Ray` using similar parameters as `Physics2D.Raycast`.                      |
| `DrawCircleCast2D`                         | Draws using similar parameters as `Physics2D.CircleCast`.                           |
| `DrawBoxCast2D`                            | Draws using similar parameters as `Physics2D.BoxCast`.                              |
| `DrawCapsuleCast2D`                        | Draws using similar parameters as `Physics2D.CapsuleCast`.                          |
| `DrawRaycast2DHit`<br/>`DrawRaycast2DHits` | Draws `RaycastHit2D` and `RaycastHit2D[]`.                                          |
| `DrawCircleCast2DHits`                     | Draws `RaycastHit2D[]` results using similar parameters as `Physics2D.CircleCast`.  |
| `DrawBoxCast2DHits`                        | Draws `RaycastHit2D[]` results using similar parameters as `Physics2D.BoxCast`.     |
| `DrawCapsuleCast2DHits`                    | Draws `RaycastHit2D[]` results using similar parameters as `Physics2D.CapsuleCast`. |

## Gizmos
| Name                                   | Description                                                                  |
|----------------------------------------|------------------------------------------------------------------------------|
| `GameViewGizmosEnabled`                | `bool`: whether Gizmos are enabled in the Game view.                         |
| `using (DebugUtils.DrawGizmosScope())` | allows the use of other `DebugUtils` methods inside of OnDrawGizmos methods. |

## Components
| Name                  | Description                                 |
|-----------------------|---------------------------------------------|
| Debug Mesh Normals    | Draws normals for a (read/write) mesh.      |
| Debug Renderer Bounds | Draws the bounds of a renderer.             |
| Debug Transform       | Draws up/right/forward axes of a transform. |

---
If you find this resource helpful:

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z42ZYHB)

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
