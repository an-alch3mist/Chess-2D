# Source: `UTIL.cs` — Unity game development utility library with 2D vector math, board representation, input handling, and helper functions

* Comprehensive utility library providing 2D integer vector operations, grid-based board management, input system abstractions, animation helpers, and various mathematical/string processing functions for Unity game development.

## Short description (2–4 sentences)
This file implements a multi-purpose utility library for Unity game development, centered around integer-based 2D vector math (`v2` struct), generic board/grid representation (`Board<T>`), unified input handling for mouse/keyboard/UI, and numerous helper functions for math, string processing, file I/O, and debugging. It serves as a foundation layer for grid-based games, providing consistent APIs for coordinate systems, input detection, animation coroutines, and development tools like logging and visualization.

## Metadata

* **Filename:** `UTIL.cs`
* **Primary namespace:** `SPACE_UTIL`
* **Dependent namespace:** `System`, `System.Linq`, `System.Collections`, `System.Collections.Generic`, `System.Text`, `System.Reflection`, `System.Text.RegularExpressions`, `UnityEngine`, `System.Threading.Tasks`
* **Estimated lines:** 200
* **Estimated chars:** 5000
* **Public types:** `v2 (struct)`, `Board<T> (class)`, `Z (static class)`
* **Unity version / Target framework (if detectable):** Unity 2020.3+ / .NET Standard 2.0
* **Dependencies:** Unity Engine core

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

```

## Control flow / responsibilities & high-level algorithm summary /Side effects and I/O
 2D Grid Board<T> depends on v2, and any out of bounds during Board .GT or .ST shall be reported to Debug.LogError() 

## Performance, allocations, and hotspots / Threading / async considerations
 Main-thread only for Unity API calls.

## Security / safety / correctness concerns
Extensive Debug.LogError calls may impact performance.

## Tests, debugging & observability
Built-in Debug.Log throughout.

## Cross-file references
Depends on Unity Engine core (Vector2, Vector3).

## TODO / Known limitations / Suggested improvements
<!-- No explicit TODO comments found in source code -->

## Appendix
 none for now.

## General Note: important behaviors
Major functionality includes 2D integer vector math system, generic grid-based board representation.

`checksum: a4f7b8c1`