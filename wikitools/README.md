# Wikitools

## Hard rules (NO exceptions)

- No classes, be it plain, `static`, `partial`, `abstract` or what not.
  - Instead: records
- No state mutations
  - No mutators: setters, Add() methods, Remove() methods, etc.
  - Instead: constructors, `with`
- No control loops: `foreach`, `for`
  - Instead: LINQ
- No `if` operators
  - Instead: ternary operator `?:`
- No sequential execution that is not compile-time checked
  In other words: no temporal coupling, i.e. no statement sequence operator
  (`;`) where the statements can be flipped without compilation failure.
  - Instead: flat code; declarative & functional programing
- No broken abstractions, including names ending with:
  `Factory`, `Tools`, `Utilities`, `Context`, `Coordinator`
  - "do-er" names, including:
  `Helper`,`Validator`, `Parser`, `Mapper`, `Resolver`, `Manager`, `Provider`

## Hard rules with important exceptions

- No side-effects
  - Exception: allowed in well-known locations of:
    - System boundaries, interacting with external systems
    - Internal database
- No environment dependencies
  - Exception: allowed in well-known locations for I/O
- No interfaces
  - Exception: more than one production implementations
  - Exception: simulation needed, for testing

## Design principles

- State management: minimize, defer, concentrate
- The value of values: algorithms operating on arbitrary deeply-nested `object`s
  (that could be serialized to JSON)
- By default the code is functional composition of pure functions,
  with any deviations cleanly, decoupled and marked.