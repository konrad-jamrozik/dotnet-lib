# Wikitools

## Hard rules (NO exceptions)

- No state mutations
  - No mutators: setters, Add() methods, Remove() methods, etc.
  - Instead: constructors, `with`
- No control loops: `foreach`, `for`
  - Instead: LINQ
- No `if` operators
  - Instead: ternary operator `?:`
- No sequential execution that is not compile-time checked
  In other words: no temporal coupling, i.e. no statement sequence operator
  (`;`) where the statements can be flipped causing a bug.
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