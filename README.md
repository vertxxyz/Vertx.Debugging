Fast editor debugging and gizmo utilities for Unity.  
Uses instanced rendering to draw shapes efficiently.

> **Note**  
> Unity 2020.1+  
> Should support all render pipelines.  
> Supports drawing from jobs and burst. This package depends on **Burst** and **Mathematics**.

https://user-images.githubusercontent.com/21963717/194199755-a63d8ebc-0cc7-4268-9316-78f7d4fbea1a.mp4

## Usage
<details>
<summary>Shape drawing</summary>
<table><tr><td>

  
#### Example

```csharp
// Draw a sphere with the specified color.
D.raw(new Shape.Sphere(position, radius), color, duration);

// Draw green sphere if nothing was hit,
// or draw a red sphere if something was.
D.raw(new Shape.Sphere(position, radius), hit, duration);

// Casts draw in green, with red where hits were detected if no color is provided.
// Cast color and hit color can be overrided manually.
D.raw(new Shape.SphereCastAll(position, direction, radius, hits, hitCount, 10), duration);
```

#### Available contexts
You can call these methods from most places, `Update`, `LateUpdate`, `FixedUpdate`, `OnDrawGizmos`, and with `ExecuteAlways`/`ExecuteInEditMode`.  
If drawn from a gizmo context, `duration` parameters will be ignored. `Gizmos.matrix` works, `Gizmos.color` is unsupported. Gizmos are not pickable.

#### Code stripping
Calls to these methods are stripped when building. You do not have to remove code or use defines.  
If your code spans many statements, only the method call will be stripped. 
</td></tr></table>
  
</details>

<details>
  <summary>Drawing <code>Physics</code> and <code>Physics2D</code> operations</summary>
<table><tr><td>

#### Example
You can replace calls to `Physics` and `Physics2D` methods with `DrawPhysics` and `DrawPhysics2D` to simply draw the results of a physics operation.

```csharp
int count = DrawPhysics.RaycastNonAlloc(r, results, distance);
```

Use `DrawPhysicsSettings.SetDuration` or `Duration` to override the length of time the casts draw for. You will need to reset this value manually.
Calls to `Duration` cannot be stripped, I would recommend using `SetDuration` if this is important to you.

#### Code stripping
The drawing within these methods will be stripped, and the original method is attempted to be inlined, but this is not consistent.  
A single method call doesn't matter when compared to a physics operation, but you can completely strip these calls by instead declaring:

```csharp
#if UNITY_EDITOR
using Physics = Vertx.Debugging.DrawPhysics;
#endif
```
</td></tr></table>

</details>

> **Note**  
> If you find you have rendering issues like upside-down depth testing, or artifacts in the game view: This is a Unity bug.  
> You can disable Depth Write and Depth Test in the problematic view using the settings in **Project Settings > Vertx > Debugging**.  
> If you're on a version of Unity where the settings UI doesn't work, it's another Unity bug, thanks Unity!

## Shapes
Drawable shapes and casts are contained within the `Shape` class. You can statically import the class if you use them often:

```csharp
using static Vertx.Debugging.Shape;
```

<details>
<summary>Shape list</summary>

### General
| Name         | Description                                                                                                                                       |
|--------------|---------------------------------------------------------------------------------------------------------------------------------------------------|
| `Text`       | A label in the scene at the provided position. (Text respects 3D gizmo fade distance)                                                             |
| `ScreenText` | A label in the top left of the view.<br>Draws using an [Overlay](https://docs.unity3d.com/Manual/overlays.html) in the Scene view when available. |


### 3D
#### Shapes
| Name                                                         | Description                                                                                      |
|--------------------------------------------------------------|--------------------------------------------------------------------------------------------------|
| `Sphere`<br>`Hemisphere`<br>`Box`<br>`Capsule`<br>`Cylinder` | 3D shapes.                                                                                       |
| `Arc`                                                        | An arc (using `Angle`[^1] to define its length).                                                 |
| `Annulus`                                                    | An annulus or annulus sector.                                                                    |
| `SurfacePoint`                                               | A ray with a circle to indicate the surface.                                                     |
| `Point`                                                      | A point without a specified direction.                                                           |
| `Axis`                                                       | An XYZ direction gizmo.                                                                          |
| `Arrow`<br>`ArrowStrip`                                      | An arrow vector, or a collection of points forming an arrow.                                     |
| `Line`<br>`LineStrip`                                        | A line, or a collection of points that make up a line.                                           |
| `DashedLine`                                                 | A dashed line.                                                                                   |
| `HalfArrow`                                                  | An arrow with only one side of its head. Commonly used to represent the HalfEdge data structure. |
| `Arrow2DFromNormal`                                          | An 2D arrow aligned in 3D space using a normal vector perpendicular to the direction.            |
| `MeshNormals`                                                | The normals of a mesh.                                                                           |
| `Ray`                                                        | A line from a position and a direction vector.                                                   |
| `Ray` (Built-in)                                             | Fallback to `Ray`.                                                                               |
| `Vector3` (Built-in)                                         | Fallback to `Point`.                                                                             |
| `RaycastHit` (Built-in)                                      | Fallback to `SurfacePoint`.                                                                      |
| `Bounds` (Built-in)                                          | Fallback to `Box`.                                                                               |
| `Collider` (Built-in)                                        | Fallback to the correct shape matching the collider type (primitive colliders only).             |

#### Casts
| Name                                                                    | Description                                                                                                                                                                           |
|-------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Raycast`<br>`Linecast`<br>`SphereCast`<br>`BoxCast`<br>`CapsuleCast`   | Using similar parameters as<br>`Physics.Raycast`<br>`Physics.Linecast`<br>`Physics.SphereCast`<br>`Physics.BoxCast`<br>`Physics.CapsuleCast`<br>with an optional `RaycastHit` result. |
| <br>`RaycastAll`<br>`SphereCastAll`<br>`BoxCastAll`<br>`CapsuleCastAll` | `RaycastHit[]` results using similar parameters as<br>`Physics.RaycastAll`<br>`Physics.SphereCastAll`<br>`Physics.BoxCastAll`<br>`Physics.CapsuleCastAll`                             |

### 2D
#### Shapes
| Name                                                       | Description                                                  |
|------------------------------------------------------------|--------------------------------------------------------------|
| `Circle2D`<br>`Box2D`<br>`Area2D`<br>`Capsule2D`<br>`Rect` | 2D shapes.                                                   |
| `Arc2D`                                                    | An arc (using `Angle`[^1] to define its length).             |
| `Point2D`                                                  | A point without a specified direction.                       |
| `Axis2D`                                                   | An XY direction gizmo.                                       |
| `Arrow2D`<br>`ArrowStrip2D`                                | An arrow vector, or a collection of points forming an arrow. |
| `Ray2D`                                                    | A line from a position and a direction vector.               |
| `Spiral2D`                                                 | A spiral, useful for visualising rotation on wheels.         |
| `Vector2` (Built-in)                                       | Fallback to `Point2D`.                                       |
| `RaycastHit2D` (Built-in)                                  | Fallback to `Ray`.                                           |
| `Rect` (Built-in)                                          | Fallback to `Box2D`.                                         |

#### Casts
| Name                                                                            | Description                                                                                                                                                                                       |
|---------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Raycast2D`<br>`Linecast2D`<br>`CircleCast2D`<br>`BoxCast2D`<br>`CapsuleCast2D` | Using similar parameters as<br>`Physics2D.Raycast`<br>`Physics2D.Linecast`<br>`Physics2D.SphereCast`<br>`Physics2D.BoxCast`<br>`Physics2D.CapsuleCast`<br>with an optional `RaycastHit2D` result. |
| <br>`RaycastAll2D`<br>`CircleCastAll2D`<br>`BoxCastAll2D`<br>`CapsuleCastAll2D` | `RaycastHit2D[]` results using similar parameters as<br>`Physics2D.RaycastAll`<br>`Physics2D.SphereCastAll`<br>`Physics2D.BoxCastAll`<br>`Physics2D.CapsuleCastAll`                               |

[^1]: The helper class `Angle` is used to define angles, author it with the static methods like `Angle.FromDegrees`.
  
</details>

<details>
<summary>Authoring new shapes</summary>
<table><tr><td>

### Extensions
  
The `Shape` class is partial. You can add `IDrawable` and `IDrawableCast` structs to the class, which will be compatible with `D.raw<T>(T shape)`.  

Use the `UnmanagedCommandBuilder` `Append` functions to create your own shapes, or combine other shapes by calling their `Draw` functions.

</td></tr></table>
</details>

## Components
Components to draw physics events and common object attributes.
  
<details>
<summary>Component list</summary>
  
| Name                   | Description                                         |
|------------------------|-----------------------------------------------------|
| Debug Transform        | Draws up, right, forward axes of a Transform.       |
| Debug Renderer Bounds  | Draws the bounds of a Renderer.                     |
| Debug Collider Bounds  | Draws the bounds of a Collider or Collider2D.       |
| Debug Collision Events | Draws `OnCollisionEnter`, `Stay` and `Exit` events. |
| Debug Trigger Events   | Draws `OnTriggerEnter`, `Stay` and `Exit` events.   |
| Debug Mesh Normals     | Draws normals for a (read/write) Mesh.              |

</details>

## Installation
[![openupm](https://img.shields.io/npm/v/com.vertx.debugging?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.vertx.debugging/)
  
<table><tr><td>
  
#### Add the OpenUPM registry
1. Open `Edit/Project Settings/Package Manager`
1. Add a new Scoped Registry (or edit the existing OpenUPM entry):
   ```
   Name: OpenUPM
   URL:  https://package.openupm.com/
   Scope(s): com.vertx
   ```
1. **Save**

#### Add the package
1. Open the Package Manager via `Window/Package Manager`.
1. Select the <kbd>+</kbd> from the top left of the window.
1. Select **Add package by Name** or **Add package from Git URL**.
1. Enter `com.vertx.debugging`.
1. Select **Add**.
1. If Burst is added for the first time or its version is changed you will need to restart Unity.
</td></tr></table>
  
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z42ZYHB)
