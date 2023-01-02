# ModelAnalyzer

Roslyn-based analysis of class models with informal contracts.

[![Build status](https://ci.appveyor.com/api/projects/status/adtm9d6che63hlf2?svg=true)](https://ci.appveyor.com/project/dlebansais/modelanalyzer)
[![codecov](https://codecov.io/gh/dlebansais/ModelAnalyzer/branch/master/graph/badge.svg?token=EiqUBv7cWP)](https://codecov.io/gh/dlebansais/ModelAnalyzer)
[![CodeFactor](https://www.codefactor.io/repository/github/dlebansais/modelanalyzer/badge)](https://www.codefactor.io/repository/github/dlebansais/modelanalyzer)
[![NuGet](https://img.shields.io/nuget/v/Test.CSharp.ModelAnalyzer.svg)](https://www.nuget.org/packages/Test.CSharp.ModelAnalyzer)

## How to install

To install this analyzer, in Visual Studio:

+ Open menu `Tools` -> `NuGet Package Manager` -> `Manage NuGet Packages for Solution`. The `NuGet - Solution` window appears.  
+ In the top right corner, make sure `Package Source` is selected to be either `nuget.org` or `All`.
+ Click the `Browse` tab and in the search prompt type `ModelAnalyzer`.
+ A list of packages appears, one one them called `CSharp.ModelAnalyzer`.
+ Click to select this package and in the right pane check projects you want to be analyzed.
+ Click the `Install` button.

## How to uninstall

To uninstall this analyzer, in Visual Studio:

+ Open menu `Tools` -> `NuGet Package Manager` -> `Manage NuGet Packages for Solution`. The `NuGet - Solution` window appears.  
+ Click to select the `CSharp.ModelAnalyzer` package and in the right pane uncheck projects you no longer want to be analyzed.
+ Click the `Uninstall` button.

# Model Analysis

This analyzer looks for inconsistencies by creating a model of a class from the source code. The model is basically a big mathematical proof of consistency. The proof is then analyzed by a theorem solver, and if found true it proves the source is consistent (in some abstract sense).

## Caveat

A big caveat to what might look like magic is that this analyzer only supports a very restricted subset of C# (see below for a list). Anything that is not supported prevents the analysis from starting and displays a warning.

## Contracts
To help the analyzer, programmers can add contract clauses:

+ A require clause puts some requirement to the input of a method.
+ An ensure clause provides some guarantees when a method exits.
+ An invariant clause added to a class puts some requirement on the system after a class is initialized, and every time a method exits.

## Features support

The analyzer supports:

+ A [limited set](#supported-types) of C# pre-defined types.
+ Private fields of a supported type. Ex: `private int X, Y;`. Fields can be initialized but only with a literal constant value (ex: `bool B = true;`, `double F = 2.0;`).
+ Private or public methods that return either `void` or one of the supported types and take zero or more parameters (also of a supported type).
  * Parameters are not allowed to have the same name as fields.
  * Parameters cannot be assigned, they are read-only.
+ The `Result` name is reserved and cannot be used for fields or parameters. If `Result` is a local variable, then a return statement must be `return Result;`. 
+ Local variables of a supported type. They are not allowed to have the same name as fields or parameters.
+ Assignment of an expression to a field or local variable.
+ The `return` statement, but at the end of a method only.
+ The `if` `else` statement.
+ Invocation of a method - of the same class only - that has no return value.
+ A restricted subset of expressions:
  * The `+`, `-`, `*` and `/` binary operators.
  * The remainder operator `%` for integers.
  * The `-` unary operator.
  * Parenthesis.
  * The `!`, `&&` and `||` logical operators.
  * The `==`, `!=`, `>`, `>=`, `<` and `<=` comparison operators.
  * Integer or double constants (ex: `0`, `1.0`), `true` and `false`.
  * Variables, either fields, local variables or parameters. Local variables are not allowed in method contracts (see below).
  * Invocation of a function, but of the same class only.
  * In ensure expressions (see below), `Result` can be used and represents the value after `return`. 

Everything else, attributes, preprocessor directives etc. is not supported.

### Supported types

The analyzer supports `bool`, `int` and `double` types only.

## Method contract

To add a require clause to a method, put a line starting with `// Require:` below the method declaration and before the opening brace. To add an ensure clause put a line starting with `// Ensure:` below the method's closing brace.

For example:

````csharp
int Sum(int x, int y)
// Require: x >= 0 && x <= 256 
// Require: y >= 0 && y <= 256 
{
  return x + y;
}
// Ensure: Result >= 0 && Result <= 512
````

Assertions support the same subset of expressions as the code.
 
## Class contract

To add an invariant, put a line starting with `// Invariant:` below the class' closing brace. For example:

````csharp
public class Test
{
  int N;

  int Sum(int x, int y)
  // Require: x >= 16 && x <= 256 
  // Require: y >= 16 && y <= 256 
  {
    N = x + y;
    return N;
  }
  // Ensure: N >= 32 && N <= 512
}
// Invariant: N == 0 || (N >= 32 && N <= 512)
````

## Suppressing warnings

If the code includes unsupported features, this is reported as one or more warnings. To remove these warnings and turn off analysis for a class, put a line starting with `// No model` before the class declaration. For example:

````csharp
// No model
public class Test
{
  /* ... */
}
````

## List of diagnostics

| Code   | Diagnostic                                         |
| ------ | -------------------------------------------------- |
| [MA0001](doc/MA0001.md) | Error in class model              |
| [MA0002](doc/MA0002.md) | Field is not supported            |
| [MA0003](doc/MA0003.md) | Method is not supported           |
| [MA0004](doc/MA0004.md) | Parameter is not supported        |
| [MA0005](doc/MA0005.md) | Require clause is invalid         |
| [MA0006](doc/MA0006.md) | Ensure clause is invalid          |
| [MA0007](doc/MA0007.md) | Local variable is not supported   |
| [MA0008](doc/MA0008.md) | Statement is invalid              |
| [MA0009](doc/MA0009.md) | Expression is invalid             |
| [MA0010](doc/MA0010.md) | Class invariant is invalid        |
| [MA0011](doc/MA0011.md) | Method require clause is violated |
| [MA0012](doc/MA0012.md) | Method ensure clause is violated  |
| [MA0013](doc/MA0013.md) | Class invariant is violated       |
| [MA0014](doc/MA0014.md) | Flow check error detected         |
