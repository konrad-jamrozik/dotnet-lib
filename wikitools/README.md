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

## Hard rules (NO exceptions)

- No mutators: setters, Add() methods, Remove() methods, etc.
- No control loops: `foreach`, `for`
- No `if` operators
- No extenion methods for types that are not collections
- No sequential execution that is not compile-time checked
  In other words: no temporal coupling, i.e. no statement sequence operator
  (`;`) where the statements can be flipped causing a bug that compiled.
  - Instead: flat code; declarative & functional programing
- No broken abstractions, including names ending with:
  `Factory`, `Tools`, `Utilities`, `Context`, `Coordinator`, `Client`
  - "do-er" names, including:
  `Helper`,`Validator`, `Parser`, `Mapper`, `Resolver`, `Manager`, `Provider`,
  `Wrapper`
- No dependency injection containers

## Defaults, requiring good justification

- Avoid classes, be it plain, `static`, `partial`, `abstract` or what not.
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