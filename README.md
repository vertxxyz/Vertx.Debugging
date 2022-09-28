Fast editor debugging and gizmo utilities for Unity.  

Should support all render pipelines. Uses instanced rendering to efficiently batch drawing functions.

> **Warning**  
> Unity 2019.4+  
> Debugging from jobs and builds is not supported, I recommend [Aline](http://arongranberg.com/aline/) if you need that functionality.

https://user-images.githubusercontent.com/21963717/153703387-cc55e3c6-26b6-4474-815a-0e65e27a73f0.mov

## Usage

```csharp
// Draw a sphere with the specified color.
D.raw(new Shapes.Sphere(position, radius), color, duration);

// Draw green sphere if nothing was hit,
// or draw a red sphere if something was.
D.raw(new Shapes.Sphere(position, radius), hit, duration);

// Draw SphereCast in green, with red spheres where hits were detected.
D.raw(new Shapes.SphereCastAll(position, direction, radius, hits, hitCount, 10), Shapes.CastColor, Shapes.HitColor, duration);
```

> **Note**  
> Calls to these methods are stripped when building. You do not have to remove code or use defines.  
> If your code spans many statements external to the method calls, it is unlikely to be stripped.

You can call these methods from most places, `Update`, `LateUpdate`, `FixedUpdate`, `OnDrawGizmos`, and with `ExecuteAlways`/`ExecuteInEditMode`.  
If drawn from a gizmo context, `duration` parameters will be ignored.

## Shapes
All new shapes are contained within the Shapes class. I recommend statically importing the class if you are using them often:

```csharp
using static Vertx.Debugging.Shapes;
```

### General
| Name         | Description                                                                                              |
|--------------|----------------------------------------------------------------------------------------------------------|
| `Text`       | A label in the scene at the provided position.                                                           |
| `ScreenText` | A label in the top left of the view.<br>(Order is not maintained when mixing durations with other text). |


### 3D
#### Shapes
| Name                                                       | Description                                                  |
|------------------------------------------------------------|--------------------------------------------------------------|
| `Sphere`<br>`Hemisphere`<br>`Box`<br>`Capsule`<br>`Bounds` | 3D shapes.                                                   |
| `Arc`                                                      | An arc (using `Angle`[^1] to define its length).             |
| `SurfacePoint`                                             | A ray with a circle to indicate the surface.                 |
| `Point`                                                    | A point without a specified direction.                       |
| `Axis`                                                     | An XYZ direction gizmo.                                      |
| `Arrow`<br>`ArrowStrip`                                    | An arrow vector, or a collection of points forming an arrow. |
| `Line`<br>`LineStrip`                                      | A line, or a collection of points that make up a line.       |
| `MeshNormals`                                              | The normals of a mesh.                                       |
| `Ray`                                                      | A line from a position and a direction vector.               |
| `Ray` (Built-in)                                           | Fallback to `Ray`.                                           |
| `Vector3` (Built-in)                                       | Fallback to `Point`.                                         |
| `RaycastHit` (Built-in)                                    | Fallback to `SurfacePoint`.                                  |
| `Bounds` (Built-in)                                        | Fallback to `Box`.                                           |


#### Casts
| Name                                                                    | Description                                                                                                                                                     |
|-------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Raycast`<br>`SphereCast`<br>`BoxCast`<br>`CapsuleCast`                 | Using similar parameters as<br>`Physics.Raycast`<br>`Physics.SphereCast`<br>`Physics.BoxCast`<br>`Physics.CapsuleCast`<br>with an optional `RaycastHit` result. |
| <br>`RaycastAll`<br>`SphereCastAll`<br>`BoxCastAll`<br>`CapsuleCastAll` | `RaycastHit[]` results using similar parameters as<br>`Physics.RaycastAll`<br>`Physics.SphereCastAll`<br>`Physics.BoxCastAll`<br>`Physics.CapsuleCastAll`       |

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

#### Casts
| Name                                                                            | Description                                                                                                                                                               |
|---------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Raycast2D`<br>`CircleCast2D`<br>`BoxCast2D`<br>`CapsuleCast2D`                 | Using similar parameters as<br>`Physics2D.Raycast`<br>`Physics2D.SphereCast`<br>`Physics2D.BoxCast`<br>`Physics2D.CapsuleCast`<br>with an optional `RaycastHit2D` result. |
| <br>`RaycastAll2D`<br>`CircleCastAll2D`<br>`BoxCastAll2D`<br>`CapsuleCastAll2D` | `RaycastHit2D[]` results using similar parameters as<br>`Physics2D.RaycastAll`<br>`Physics2D.SphereCastAll`<br>`Physics2D.BoxCastAll`<br>`Physics2D.CapsuleCastAll`       |

[^1]: The helper class `Angle` is used to define angles, author it with the static methods like `Angle.FromDegrees`.

## Components
| Name                   | Description                                         |
|------------------------|-----------------------------------------------------|
| Debug Transform        | Draws up, right, forward axes of a Transform.       |
| Debug Renderer Bounds  | Draws the bounds of a Renderer.                     |
| Debug Collider Bounds  | Draws the bounds of a Collider or Collider2D.       |
| Debug Collision Events | Draws `OnCollisionEnter`, `Stay` and `Exit` events. |
| Debug Trigger Events   | Draws `OnTriggerEnter`, `Stay` and `Exit` events.   |
| Debug Mesh Normals     | Draws normals for a (read/write) Mesh.              |

## Extensions

The `Shapes` class is partial. You can add `IDrawable` and `IDrawableCast` structs to the class, which will be compatible with `D.raw<T>(T shape)`.  
Use the `CommandBuilder` `Append` functions to create your own shapes, or combine other shapes by calling their `Draw` functions.

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

You can also add it directly from GitHub. Note that you won't be able to receive updates through Package Manager this way, you'll have to update manually.

- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `https://github.com/vertxxyz/Vertx.Debugging.git`
- click <kbd>Add</kbd>  
  **or**
- Edit your `manifest.json` file to contain `"com.vertx.debugging": "https://github.com/vertxxyz/Vertx.Debugging.git"`,

To update the package with new changes, remove the lock from the `packages-lock.json` file.
</details>
