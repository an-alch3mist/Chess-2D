# v2 & Board<T> — Integer 2D vector and grid utilities

Core utility structures for 2D coordinate math and grid-based data storage in Unity games.

## Short description
Provides `v2` struct for integer-based 2D coordinates with comprehensive operators and direction utilities, plus `Board<T>` generic class for type-safe 2D grid storage. Designed for tile-based games, pathfinding, and grid-based gameplay systems with seamless Unity Vector2/Vector3 conversion.

## Metadata
- **Filename:** `UTIL.cs` (v2 & Board<T> sections)
- **Primary namespace:** `SPACE_UTIL`
- **Estimated lines:** ~150 (v2: ~80, Board<T>: ~70)
- **Estimated chars:** ~4,200
- **Public types:** `v2, Board<T>`
- **Unity version:** Unity 2019.4+ / .NET Standard 2.0
- **Dependencies:** UnityEngine (Vector2/Vector3), System.Collections.Generic

## Public API summary

| Type | Member | Signature | Purpose |
|------|--------|-----------|---------|
| v2 | Constructor | `v2(int x, int y)` | Create coordinate |
| v2 | Operators | `+, -, *, ==, !=, >, <, >=, <=` | Math operations |
| v2 | dot | `static float dot(v2 a, v2 b)` | Dot product |
| v2 | area | `static float area(v2 a, v2 b)` | Cross product area |
| v2 | getDIR | `static List<v2> getDIR(bool diagonal = false)` | Get directional vectors |
| v2 | getdir | `static v2 getdir(string dir_str = "r")` | Parse direction string |
| v2 | Conversions | `implicit operator v2((int, int))` | Tuple conversion |
| v2 | Unity Convert | `implicit operator Vector2/Vector3(v2)` | Unity vector conversion |
| Board<T> | Constructor | `Board(v2 size, T default_val)` | Create grid |
| Board<T> | GT | `T GT(v2 coord)` | Get value at coordinate |
| Board<T> | ST | `void ST(v2 coord, T val)` | Set value at coordinate |
| Board<T> | clone | `Board<T> clone { get; }` | Deep copy board |

## Important types — details

### `v2`
- **Kind:** struct
- **Responsibility:** Integer-based 2D coordinate with comprehensive math operations and Unity integration
- **Constructor:** `v2(int x, int y)` — creates coordinate pair
- **Public fields:** 
  - `x` — int — horizontal coordinate
  - `y` — int — vertical coordinate
  - `axisY` — static char — controls 3D conversion axis ('y' or 'z')
- **Public methods:**
  - **`static List<v2> getDIR(bool diagonal = false)`**
    - **Description:** Returns list of directional unit vectors (4 or 8 directions)
    - **Parameters:** diagonal : bool — include diagonal directions
    - **Returns:** List<v2> — direction vectors [(1,0), (0,1), (-1,0), (0,-1), ...]
  - **`static v2 getdir(string dir_str = "r")`**
    - **Description:** Parse direction string into vector ("r"=right, "u"=up, "ru"=right-up)
    - **Parameters:** dir_str : string — direction chars (r/u/l/d combinations)
    - **Returns:** v2 — resulting direction vector
  - **`static float dot(v2 a, v2 b)`** — dot product calculation
  - **`static float area(v2 a, v2 b)`** — cross product area (2D determinant)
- **Operators:** Full arithmetic (+, -, *, scalar multiply), comparison (==, !=, >, <, >=, <=)
- **Conversions:** Implicit from/to Unity Vector2/Vector3, tuples
- **Notes:** Y-axis handling configurable via static `axisY` field

### `Board<T>`
- **Kind:** class (generic)
- **Responsibility:** Type-safe 2D grid storage with bounds checking and coordinate-based access
- **Constructor:** `Board(v2 size, T default_val)` — creates grid filled with default value
- **Public properties:**
  - `w, h` — int — width and height
  - `m, M` — v2 — minimum (0,0) and maximum coordinates
  - `B` — T[][] — internal jagged array storage
  - `clone` — Board<T> — property returning deep copy
- **Public methods:**
  - **`T GT(v2 coord)`**
    - **Description:** Get value at coordinate
    - **Parameters:** coord : v2 — grid coordinate
    - **Returns:** T — value at position
    - **Throws:** Debug.LogError if coordinate out of bounds
  - **`void ST(v2 coord, T val)`**
    - **Description:** Set value at coordinate
    - **Parameters:** coord : v2 — grid coordinate, val : T — value to set
    - **Side effects:** Modifies grid state
    - **Throws:** Debug.LogError if coordinate out of bounds
  - **`string ToString()`** — renders grid as string (top-to-bottom, left-to-right)

## Example usage

```csharp
// v2 usage
v2 pos = new v2(5, 3);
v2 dir = v2.getdir("ru"); // (1, 1) - right-up
v2 newPos = pos + dir * 2; // (7, 5)

// Implicit conversions
v2 fromTuple = (10, 20);
Vector3 unity3D = fromTuple; // becomes (10, 20, 0) or (10, 0, 20)

// Board usage
Board<char> grid = new Board<char>((10, 10), '.');
grid.ST((5, 3), 'X');
char cell = grid.GT((5, 3)); // 'X'
string visual = grid.ToString(); // ASCII grid representation
```

## Control flow / responsibilities
`v2` acts as drop-in replacement for Unity's Vector2Int with extended functionality for game grids. Direction utilities support movement systems and pathfinding. `Board<T>` provides safe grid access with automatic bounds checking and visual debugging through ToString().

## Side effects and I/O
- `v2` conversions depend on static `axisY` configuration
- `Board<T>` logs errors to Unity console on out-of-bounds access
- Both types are serializable for Unity Inspector

## Performance considerations
- `v2` is value type (stack allocated, efficient copying)
- `Board<T>` uses jagged arrays (T[][]) for row-major access
- Clone operation performs full deep copy (O(w*h))
- No dynamic allocations during normal get/set operations

## Security / safety concerns
- Out-of-bounds access logs error but doesn't throw exceptions
- Relies on Unity Debug.LogError (may be stripped in release builds)
- Static `axisY` affects all v2↔Vector3 conversions globally

## Cross-file references
- Uses Unity's Vector2, Vector3, Debug.LogError(when Board access GT, ST is not in range)

## TODO / Known limitations for future
- none for now