# Source: `UTIL.cs` — Unity game development utility library with 2D vector math, board representation, input handling, and helper functions

* Comprehensive utility library providing 2D integer vector operations, grid-based board management, input system abstractions, animation helpers, and various mathematical/string processing functions for Unity game development.

## Short description (2–4 sentences)
This file implements a multi-purpose utility library for Unity game development, centered around integer-based 2D vector math (`v2` struct), generic board/grid representation (`Board<T>`), unified input handling for mouse/keyboard/UI, and numerous helper functions for math, string processing, file I/O, and debugging. It serves as a foundation layer for grid-based games, providing consistent APIs for coordinate systems, input detection, animation coroutines, and development tools like logging and visualization.

## Metadata

* **Filename:** `UTIL.cs`
* **Primary namespace:** `SPACE_UTIL`
* **Dependent namespace:** `System`, `System.Linq`, `System.Collections`, `System.Collections.Generic`, `System.Text`, `System.Reflection`, `System.Text.RegularExpressions`, `UnityEngine`, `UnityEngine.EventSystems`, `UnityEngine.UI`, `TMPro`, `System.Threading.Tasks`
* **Estimated lines:** 1200
* **Estimated chars:** 45000
* **Public types:** `v2 (struct)`, `Board<T> (class)`, `Z (static class)`, `INPUT (static class)`, `INPUT.M (static class)`, `INPUT.K (static class)`, `INPUT.UI (static class)`, `AN (static class)`, `C (static class)`, `C.SYS (static class)`, `U (static class)`, `ITER (static class)`, `LOG (static class)`, `DRAW (static class)`
* **Unity version / Target framework (if detectable):** Unity 2020.3+ / .NET Standard 2.0
* **Dependencies:** Unity Engine core, TextMeshPro, Unity UI system

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| v2 (struct) | x, y | `public int x, y` | Integer coordinates | `var pos = myV2.x; myV2.y = 5;` |
| v2 (struct) | Constructor | `public v2(int x, int y)` | Create vector from coordinates | `var v = new v2(1, 2);` |
| v2 (struct) | ToString | `public override string ToString()` | String representation | `string s = myV2.ToString();` |
| v2 (struct) | + operator | `public static v2 operator +(v2 a, v2 b)` | Vector addition | `v2 result = a + b;` |
| v2 (struct) | - operator | `public static v2 operator -(v2 a, v2 b)` | Vector subtraction | `v2 result = a - b;` |
| v2 (struct) | * operator | `public static v2 operator *(v2 a, v2 b)` | Component-wise multiplication | `v2 result = a * b;` |
| v2 (struct) | * operator | `public static v2 operator *(v2 v, int m)` | Scalar multiplication | `v2 result = v * 3;` |
| float (basic) | dot | `public static float dot(v2 a, v2 b)` | Dot product | `float d = v2.dot(a, b);` |
| float (basic) | area | `public static float area(v2 a, v2 b)` | Cross product (2D area) | `float area = v2.area(a, b);` |
| bool (basic) | == operator | `public static bool operator ==(v2 a, v2 b)` | Equality comparison | `bool equal = (a == b);` |
| bool (basic) | != operator | `public static bool operator !=(v2 a, v2 b)` | Inequality comparison | `bool notEqual = (a != b);` |
| List<v2> | getDIR | `public static List<v2> getDIR(bool diagonal = false)` | Get direction vectors | `var dirs = v2.getDIR(true);` |
| v2 (struct) | getdir | `public static v2 getdir(string dir_str = "r")` | Get direction from string | `v2 dir = v2.getdir("ru");` |
| char (basic) | axisY | `public static char axisY` | Y-axis mode ('y' or 'z') | `v2.axisY = 'z';` |
| Board<T> (class) | w, h | `public int w, h` | Board dimensions | `int width = board.w;` |
| Board<T> (class) | m, M | `public v2 m, M` | Min/max coordinates | `v2 min = board.m;` |
| Board<T> (class) | Constructor | `public Board(v2 size, T default_val)` | Create board with size | `var board = new Board<int>((10, 10), 0);` |
| T | GT | `public T GT(v2 coord)` | Get value at coordinate | `int val = board.GT((3, 4));` |
| void | ST | `public void ST(v2 coord, T val)` | Set value at coordinate | `board.ST((3, 4), 42);` |
| string (basic) | ToString | `public override string ToString()` | Board string representation | `string s = board.ToString();` |
| Board<T> (class) | clone | `public Board<T> clone { get; }` | Create deep copy | `Board<int> copy = board.clone;` |
| float (basic) | dot | `public static float dot(Vector3 a, Vector3 b)` | 3D dot product | `float d = Z.dot(vec1, vec2);` |
| float (basic) | lerp | `public static float lerp(float a, float b, float t)` | Linear interpolation | `float result = Z.lerp(0f, 10f, 0.5f);` |
| Vector3 | lerp | `public static Vector3 lerp(Vector3 a, Vector3 b, float t)` | Vector3 interpolation | `Vector3 result = Z.lerp(v1, v2, 0.5f);` |
| Vector3 | Path | `public static Vector3 Path(float t, params Vector3[] P)` | Linear path interpolation | `Vector3 pos = Z.Path(0.5f, p1, p2, p3);` |
| Vector3 | Bezier | `public static Vector3 Bezier(float t, params Vector3[] P)` | Bezier curve interpolation | `Vector3 pos = Z.Bezier(0.5f, p1, p2, p3);` |
| void | Init | `public static void Init(Camera MainCam, RectTransform CanvasRectTransform)` | Initialize input system | `INPUT.Init(mainCam, canvasRect);` |
| Vector3 | getPos3D | `public static Vector3 getPos3D { get; }` | Mouse world position | `Vector3 mousePos = INPUT.M.getPos3D;` |
| bool (basic) | InstantDown | `public static bool InstantDown(int mouse_btn_type = 0)` | Mouse button pressed this frame | `bool clicked = INPUT.M.InstantDown(0);` |
| bool (basic) | HeldDown | `public static bool HeldDown(int mouse_btn_type = 0)` | Mouse button held down | `bool held = INPUT.M.HeldDown(0);` |
| KeyCode | KeyCodeInstantDown | `public static KeyCode KeyCodeInstantDown { get; }` | Get pressed key this frame | `KeyCode key = INPUT.K.KeyCodeInstantDown;` |
| bool (basic) | Hover | `public static bool Hover { get; }` | Mouse over UI element | `bool overUI = INPUT.UI.Hover;` |
| Vector2 | pos | `public static Vector2 pos { get; }` | UI mouse position | `Vector2 uiPos = INPUT.UI.pos;` |
| IEnumerator | typewriter_effect | `public static IEnumerator typewriter_effect(this TextMeshProUGUI tm_gui, float waitInBetween = 0.05f)` | Typewriter text animation | `yield return textUI.typewriter_effect(0.1f);` or `StartCoroutine(textUI.typewriter_effect());` |
| void | Init | `public static void Init()` | Initialize utility system | `C.Init();` |
| float (basic) | clamp | `public static float clamp(float x, float min, float max, float e = 0f)` | Clamp value to range | `float result = C.clamp(5f, 0f, 10f);` |
| int (basic) | round | `public static int round(float x)` | Round to nearest integer | `int result = C.round(3.7f);` |
| string (basic) | clean | `public static string clean(this string raw_str)` | Remove \\r and trim whitespace | `string clean = rawStr.clean();` |
| string[] | split | `public static string[] split(this string str, string re, string flags = "gx")` | Regex split | `string[] parts = text.split(@"\\n\\n");` |
| string[] | match | `public static string[] match(this string str, string re, string flags = "gm")` | Regex match all | `string[] matches = text.match(@"\\w+");` |
| bool (basic) | fmatch | `public static bool fmatch(this string str, string re, string flags = "g")` | Test regex match | `bool matches = text.fmatch(@"\\d+");` |
| Task | delay | `public static async Task delay(int ms = 1000)` | Async delay | `await C.delay(2000);` |
| IEnumerator | wait | `public static IEnumerator wait(int ms = 1000)` | Coroutine wait | `yield return C.wait(1000);` or `StartCoroutine(C.wait(1000));` |
| string (basic) | ToJson | `public static string ToJson(this object obj, bool pretify = true)` | Serialize to JSON | `string json = myObject.ToJson();` |
| T | FromJson | `public static T FromJson<T>(this string json)` | Deserialize from JSON | `MyClass obj = jsonStr.FromJson<MyClass>();` |
| Transform | NameStartsWith | `public static Transform NameStartsWith(this Transform transform, string name)` | Find child by name prefix | `Transform child = parent.NameStartsWith("Player");` |
| Transform | Query | `public static Transform Query(this Transform transform, string query, char sep = '>')` | Query nested children | `Transform target = root.Query("UI > Panel > Button");` |
| bool (basic) | CanPlaceObject2D | `public static bool CanPlaceObject2D(Vector2 pos2D, GameObject gameObject, int rotationZ = 0)` | Check 2D placement collision | `bool canPlace = U.CanPlaceObject2D(pos, prefab);` |
| bool (basic) | CanPlaceObject3D | `public static bool CanPlaceObject3D(Vector3 pos3D, GameObject _prefab, int rotationY = 0)` | Check 3D placement collision | `bool canPlace = U.CanPlaceObject3D(pos, prefab);` |
| bool (basic) | iter_inc | `public static bool iter_inc(double limit = 1e4)` | Increment iteration counter | `bool exceeded = ITER.iter_inc(1000);` |
| void | SaveLog | `public static void SaveLog(params object[] args)` | Save to log file | `LOG.SaveLog("Debug info", data);` |
| string (basic) | ToTable | `public static string ToTable<T>(this IEnumerable<T> list, bool toString = false, string name = "LIST<>")` | Format collection as table | `string table = myList.ToTable(false, "MyData");` |
| void | LINE | `public static void LINE(Vector3 a, Vector3 b, float e = 1f / 200)` | Draw debug line | `DRAW.LINE(start, end);` |

## Important types — details

### `v2`
* **Kind:** struct (SPACE_UTIL.v2)
* **Responsibility:** Integer-based 2D vector for grid coordinates with arithmetic operations and Unity conversions.
* **Constructor(s):** `v2(int x, int y)` — creates vector with specified coordinates
* **Public properties / fields:** 
  * `x — int — X coordinate (get/set)`
  * `y — int — Y coordinate (get/set)`
  * `axisY — char — static field controlling Y-axis interpretation ('y' or 'z') (get/set)`
* **Public methods:**
  * **Signature:** `public static v2 operator +(v2 a, v2 b)`
  * **Description:** Vector addition.
  * **Parameters:** a : v2 — first vector, b : v2 — second vector
  * **Returns:** v2 — sum vector, `v2 result = a + b;`
  * **Signature:** `public static List<v2> getDIR(bool diagonal = false)`
  * **Description:** Gets cardinal and optionally diagonal direction vectors.
  * **Parameters:** diagonal : bool — include diagonal directions
  * **Returns:** List<v2> — direction vectors, `List<v2> dirs = v2.getDIR(true);`
  * **Signature:** `public static v2 getdir(string dir_str = "r")`
  * **Description:** Parse direction string into vector (r=right, u=up, l=left, d=down).
  * **Parameters:** dir_str : string — direction string like "ru" for right-up
  * **Returns:** v2 — parsed direction, `v2 dir = v2.getdir("ld");`

### `Board<T>`
* **Kind:** class (SPACE_UTIL.Board<T>)
* **Responsibility:** Generic 2D grid storage with bounds checking and coordinate-based access.
* **Constructor(s):** `Board(v2 size, T default_val)` — creates board with dimensions and fills with default value
* **Public properties / fields:**
  * `w — int — board width (get/set)`
  * `h — int — board height (get/set)`
  * `m — v2 — minimum coordinates (0,0) (get/set)`
  * `M — v2 — maximum coordinates (w-1,h-1) (get/set)`
  * `clone — Board<T> — deep copy of board (get)`
* **Public methods:**
  * **Signature:** `public T GT(v2 coord)`
  * **Description:** Get value at coordinate with bounds checking.
  * **Parameters:** coord : v2 — target coordinate
  * **Returns:** T — stored value, `T val = board.GT((3, 4));`
  * **Throws:** Debug.LogError on out-of-bounds access
  * **Signature:** `public void ST(v2 coord, T val)`
  * **Description:** Set value at coordinate with bounds checking.
  * **Parameters:** coord : v2 — target coordinate, val : T — value to store
  * **Returns:** void, `board.ST((3, 4), newValue);`

### `Z`
* **Kind:** static class (SPACE_UTIL.Z)
* **Responsibility:** Mathematical utilities for interpolation, dot products, and curve calculations.
* **Public methods:**
  * **Signature:** `public static Vector3 Path(float t, params Vector3[] P)`
  * **Description:** Linear interpolation along multi-point path.
  * **Parameters:** t : float — parameter 0-1, P : Vector3[] — path points
  * **Returns:** Vector3 — interpolated position, `Vector3 pos = Z.Path(0.5f, p1, p2, p3);`
  * **Signature:** `public static Vector3 Bezier(float t, params Vector3[] P)`
  * **Description:** Cubic Bezier curve evaluation (supports 3-4 points).
  * **Parameters:** t : float — parameter 0-1, P : Vector3[] — control points
  * **Returns:** Vector3 — curve position, `Vector3 pos = Z.Bezier(0.3f, start, cp1, cp2, end);`

### `INPUT`
* **Kind:** static class (SPACE_UTIL.INPUT)
* **Responsibility:** Unified input handling for mouse, keyboard, and UI interactions with Unity integration.
* **Public methods:**
  * **Signature:** `public static void Init(Camera MainCam, RectTransform CanvasRectTransform)`
  * **Description:** Initialize input system with camera and UI canvas references.
  * **Parameters:** MainCam : Camera — main camera, CanvasRectTransform : RectTransform — UI canvas
  * **Returns:** void, `INPUT.Init(Camera.main, canvasRect);`

### `INPUT.M`
* **Kind:** static class (SPACE_UTIL.INPUT.M)
* **Responsibility:** Mouse input detection and 3D world position calculation.
* **Public properties / fields:**
  * `MainCam — Camera — main camera reference (get/set)`
  * `up — Vector3 — plane normal for 3D projection (get/set)`
  * `getPos3D — Vector3 — current mouse world position (get)`
* **Public methods:**
  * **Signature:** `public static bool InstantDown(int mouse_btn_type = 0)`
  * **Description:** Check if mouse button was pressed this frame.
  * **Parameters:** mouse_btn_type : int — button index (0=left, 1=right, 2=middle)
  * **Returns:** bool — true if pressed, `bool clicked = INPUT.M.InstantDown(0);`

### `INPUT.K`
* **Kind:** static class (SPACE_UTIL.INPUT.K)
* **Responsibility:** Keyboard input detection and key polling.
* **Public properties / fields:**
  * `KeyCodeInstantDown — KeyCode — first key pressed this frame or KeyCode.Backslash if none (get)`
* **Public methods:**
  * **Signature:** `public static bool InstantDown(KeyCode keyCode)`
  * **Description:** Check if specific key was pressed this frame.
  * **Parameters:** keyCode : KeyCode — target key
  * **Returns:** bool — true if pressed, `bool pressed = INPUT.K.InstantDown(KeyCode.Space);`

### `INPUT.UI`
* **Kind:** static class (SPACE_UTIL.INPUT.UI)
* **Responsibility:** UI-specific input handling and coordinate conversion.
* **Public properties / fields:**
  * `Hover — bool — true if mouse is over any UI element (get)`
  * `pos — Vector2 — mouse position in UI coordinates (get)`
  * `size — Vector2 — screen size in UI coordinates (get)`
* **Public methods:**
  * **Signature:** `public static Vector2 convert(Vector2 v)`
  * **Description:** Convert screen coordinates to canvas coordinates.
  * **Parameters:** v : Vector2 — screen position
  * **Returns:** Vector2 — canvas position, `Vector2 canvasPos = INPUT.UI.convert(screenPos);`

### `AN`
* **Kind:** static class (SPACE_UTIL.AN)
* **Responsibility:** Animation utilities, particularly text effects.
* **Public methods:**
  * **Signature:** `public static IEnumerator typewriter_effect(this TextMeshProUGUI tm_gui, float waitInBetween = 0.05f)`
  * **Description:** Coroutine that reveals text character by character with underscore cursor.
  * **Parameters:** tm_gui : TextMeshProUGUI — target text component, waitInBetween : float — delay per character
  * **Returns:** IEnumerator — coroutine, `yield return textUI.typewriter_effect(0.1f);` or `StartCoroutine(textUI.typewriter_effect());`
  * **Side effects / state changes:** Modifies tm_gui.text property over time
  * **Notes:** Duration calculated as (text.Length * waitInBetween) seconds, uses yield return new WaitForSeconds()

### `C`
* **Kind:** static class (SPACE_UTIL.C)
* **Responsibility:** Core utilities for math, string processing, JSON, and system operations.
* **Public properties / fields:**
  * `PrefabHolder — Transform — container for instantiated prefabs (get/set)`
  * `e — float — small epsilon value (1/100) (get)`
  * `pi — float — Pi constant (get)`
* **Public methods:**
  * **Signature:** `public static void Init()`
  * **Description:** Initialize utility system, creates PrefabHolder GameObject.
  * **Returns:** void, `C.Init();`
  * **Signature:** `public static string clean(this string raw_str)`
  * **Description:** Remove carriage returns and trim whitespace.
  * **Parameters:** raw_str : string — input string
  * **Returns:** string — cleaned string, `string clean = rawStr.clean();`
  * **Signature:** `public static string[] split(this string str, string re, string flags = "gx")`
  * **Description:** Split string using regex pattern.
  * **Parameters:** str : string — input string, re : string — regex pattern, flags : string — regex flags
  * **Returns:** string[] — split parts, `string[] parts = text.split(@"\\n\\n");`
  * **Signature:** `public static async Task delay(int ms = 1000)`
  * **Description:** Asynchronous delay.
  * **Parameters:** ms : int — delay in milliseconds
  * **Returns:** Task — awaitable task, `await C.delay(2000);`
  * **Signature:** `public static IEnumerator wait(int ms = 1000)`
  * **Description:** Coroutine delay.
  * **Parameters:** ms : int — delay in milliseconds
  * **Returns:** IEnumerator — coroutine, `yield return C.wait(1000);` or `StartCoroutine(C.wait(1000));`
  * **Notes:** Duration is (ms / 1000.0) seconds, uses yield return new WaitForSeconds()

### `U`
* **Kind:** static class (SPACE_UTIL.U)
* **Responsibility:** Unity-specific utilities for Transform searches, collision detection, and collection operations.
* **Public methods:**
  * **Signature:** `public static bool CanPlaceObject3D(Vector3 pos3D, GameObject _prefab, int rotationY = 0)`
  * **Description:** Check if 3D object can be placed without collision using BoxCollider overlap.
  * **Parameters:** pos3D : Vector3 — world position, _prefab : GameObject — object to test, rotationY : int — rotation steps (90° each)
  * **Returns:** bool — true if placement valid, `bool canPlace = U.CanPlaceObject3D(pos, prefab, 1);`
  * **Side effects / state changes:** Calls Physics.SyncTransforms()
  * **Performance:** O(n) colliders tested per BoxCollider on prefab
  * **Signature:** `public static Transform NameStartsWith(this Transform transform, string name)`
  * **Description:** Find direct child with name starting with given string (case insensitive).
  * **Parameters:** transform : Transform — parent transform, name : string — name prefix
  * **Returns:** Transform — found child or null, `Transform child = parent.NameStartsWith("Player");`

### `ITER`
* **Kind:** static class (SPACE_UTIL.ITER)
* **Responsibility:** Iteration counter for infinite loop protection in algorithms.
* **Public methods:**
  * **Signature:** `public static bool iter_inc(double limit = 1e4)`
  * **Description:** Increment counter and check if limit exceeded.
  * **Parameters:** limit : double — maximum iterations allowed
  * **Returns:** bool — true if limit exceeded, `bool exceeded = ITER.iter_inc(1000);`
  * **Signature:** `public static void reset()`
  * **Description:** Reset iteration counter to zero.
  * **Returns:** void, `ITER.reset();`

### `LOG`
* **Kind:** static class (SPACE_UTIL.LOG)
* **Responsibility:** File-based logging and data serialization to Application.dataPath/LOG/.
* **Public properties / fields:**
  * `LoadGame — string — contents of GameData.txt file (get)`
* **Public methods:**
  * **Signature:** `public static void Init()`
  * **Description:** Create LOG directory and files if they don't exist.
  * **Returns:** void, `LOG.Init();`
  * **Side effects / state changes:** Creates directories and files on disk
  * **Signature:** `public static void SaveLog(params object[] args)`
  * **Description:** Append objects to LOG.txt file with newlines.
  * **Parameters:** args : object[] — objects to log
  * **Returns:** void, `LOG.SaveLog("Debug info", dataObject, 42);`
  * **Side effects / state changes:** Writes to Application.dataPath/LOG/LOG.txt
  * **Signature:** `public static string ToTable<T>(this IEnumerable<T> list, bool toString = false, string name = "LIST<>")`
  * **Description:** Format collection as ASCII table showing all public/private fields.
  * **Parameters:** list : IEnumerable<T> — collection, toString : bool — use ToString() per row, name : string — table header
  * **Returns:** string — formatted table, `string table = myList.ToTable(false, "PlayerData");`

### `DRAW`
* **Kind:** static class (SPACE_UTIL.DRAW)
* **Responsibility:** Debug visualization using Unity's Debug.DrawLine.
* **Public properties / fields:**
  * `col — Color — line color (default red) (get/set)`
  * `dt — float — line duration (default 10s) (get/set)`
* **Public methods:**
  * **Signature:** `public static void LINE(Vector3 a, Vector3 b, float e = 1f / 200)`
  * **Description:** Draw thick debug line between two points.
  * **Parameters:** a : Vector3 — start point, b : Vector3 — end point, e : float — line thickness
  * **Returns:** void, `DRAW.LINE(startPos, endPos);`
  * **Signature:** `public static void ARROW(Vector3 a, Vector3 b, float t = 1f, float s = 1f / 15, float e = 1f / 200)`
  * **Description:** Draw arrow with arrowhead at specified position along line.
  * **Parameters:** a : Vector3 — start, b : Vector3 — end, t : float — arrowhead position (0-1), s : float — arrowhead size, e : float — thickness
  * **Returns:** void, `DRAW.ARROW(start, end, 0.8f, 0.1f);`

## Example usage
```csharp
// Namespace required: using SPACE_UTIL;

// Basic v2 vector math
v2 pos = new v2(5, 3);
v2 dir = v2.getdir("ru"); // right-up
v2 newPos = pos + dir * 2;

// Board operations
Board<int> grid = new Board<int>((10, 10), 0);
grid.ST((3, 4), 42);
int value = grid.GT((3, 4));

// Input handling
INPUT.Init(Camera.main, canvasRectTransform);
if (INPUT.M.InstantDown(0)) {
    Vector3 worldPos = INPUT.M.getPos3D;
}

// Coroutine usage
StartCoroutine(textComponent.typewriter_effect(0.08f));
yield return C.wait(2000); // 2 second delay
```

## Control flow / responsibilities & high-level algorithm summary /Side effects and I/O
Operates as static utility layer with initialization phase (INPUT.Init, C.Init, LOG.Init) followed by per-frame input polling and on-demand mathematical/string operations. Key algorithms include 3D mouse raycasting, regex-based string processing, Unity collider overlap testing for placement validation, and reflection-based table formatting. File I/O occurs in LOG class writing to Application.dataPath/LOG/.

## Performance, allocations, and hotspots / Threading / async considerations
Heavy allocations in string operations (split, match, replace), reflection in ToTable, and Physics overlap queries. Main-thread only for Unity API calls.

## Security / safety / correctness concerns
Extensive Debug.LogError calls may impact performance; regex operations can be expensive; file I/O operations lack error handling; Physics.OverlapBox requires careful layermask usage.

## Tests, debugging & observability
Built-in Debug.Log throughout, file-based LOG.SaveLog system, DRAW visualization utilities, ITER counter for loop protection, and ToTable for collection inspection.

## Cross-file references
Depends on Unity Engine core (Vector2, Vector3, Transform, GameObject, MonoBehaviour, Camera), Unity UI (RectTransform, EventSystem), and TextMeshPro (TextMeshProUGUI, TextMeshPro). References System.Threading.Tasks for async operations.

## TODO / Known limitations / Suggested improvements
<!-- No explicit TODO comments found in source code. Potential improvements: add async file I/O, optimize string operations for GC, add bounds checking to more math functions, consider thread-safe variants for multi-threading scenarios. (only if I explicitly mentioned in the prompt) -->

## Appendix
Key private helpers include `str_to_flags()` for regex parsing, `DepthSearch()` for recursive Transform traversal, and `RenderElemValue()` for ToTable formatting. Call graph flows from Init → Input polling → Math/String processing → Logging/Drawing.

## General Note: important behaviors
Major functionality includes 2D integer vector math system, generic grid-based board representation, unified input abstraction layer, file-based logging system, and debug visualization tools. Inferred: designed for grid-based games requiring precise integer coordinates and extensive debugging capabilities.

`checksum: a4f7b8c2`