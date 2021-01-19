# Wikitools

## What is encouraged

- Primary constructors
- Records
- 'with'
- LINQ
- Pure functions and functional composition
- Extension methods for collection types
- Ternary operator `?:`
- Tuples

## Hard rules (almost no exceptions)

- No static classes, no singletons
- No mutators: setters, Add() methods, Remove() methods, etc.
- No control loops: `foreach`, `for`
- No `if` operators, expect when necessary, e.g. to throw in precondition checks
- No extenion methods for types that are not collections
- No methods that both do side effects and anything else (like computations or returning value)
- No sequential execution that is not compile-time checked
  In other words: no temporal coupling, i.e. no statement sequence operator
  (`;`) where the statements can be flipped causing a bug that compiled.
  - Instead: flat code; declarative & functional programing
- No broken abstractions, including vague names with:
  `Factory`, `Tools`, `Utilities`, `Context`, `Coordinator`, `Client`, `Constants`
  `Details`, `Common`, `Engine`, `Descriptor`, `Helper`, `Wrapper`, `Adapter`
  - "do-er" / "verb-er" names, including:
   `Validator`, `Parser`, `Mapper`, `Resolver`, `Manager`, `Provider`,
  `Controller`,  `Writer`, `Reader`, `Selector`, `Logger`
- No dependency injection containers

## Defaults, requiring good justification

- Avoid classes in prod code, be it plain, `static`, `partial`, `abstract` or what not.
  - Instead: records
- Avoid methods with no return value
  - Allowed only when inside side-effectful block
- Avoid side-effects
  - Allowed in well-known locations of:
    - System boundaries, interacting with external systems
    - Internal database
- Avoid environment dependencies
  - Allowed in well-known locations for I/O
- Avoid interfaces. Allowed when:
  - More than one production implementations
  - Simulation needed, for testing

## Design principles

- State management: minimize, defer, concentrate
- The value of values: algorithms operating on arbitrary deeply-nested `object`s
  (that could be serialized to JSON)
- By default the code is functional composition of pure functions,
  with any deviations cleanly, decoupled and marked.

- [Solving Problems the Clojure Way - Rafal Dittwald](https://www.youtube.com/watch?v=vK1DazRK_a0)
- [The Value of Values - Rich Hickey](https://github.com/matthiasn/talk-transcripts/blob/master/Hickey_Rich/ValueOfValues.md)
- [Seven Virtues of a Good Object - Yegor Bugayenko](https://www.yegor256.com/2014/11/20/seven-virtues-of-good-object.html)
- [Don't Create Objects That End With -ER - Yegor Bugayenko](https://www.yegor256.com/2015/03/09/objects-end-with-er.html)
- [OP Alternative to Utility Classes](https://www.yegor256.com/2014/05/05/oop-alternative-to-utility-classes.html)