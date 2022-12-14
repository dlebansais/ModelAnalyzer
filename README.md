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

+ Private fields of type `int`. Ex: `private int X, Y;`. Fields can only use default initialization (to zero).
+ Private or public methods that return either `void` or `int` and take zero or more `int` parameters.
  * Parameters are not allowed to have the same name as fields.
  * Parameters cannot be assigned, they are read-only.
+ Assignment of an expression to a field.
+ `return`, but at the end of a method only.
+ The `if` `else` statement.
+ A restricted subset of expressions:
  * The `+`, `-`, `*` and `/` binary operators.
  * The `-` unary operator.
  * Parenthesis.
  * The `&&` and `||` logical operators.
  * The `==`, `!=`, `>`, `>=`, `<` and `<=` comparison operators.
  * Integer constants (ex: `0`), `true` and `false`.
  * Variables, either fields or parameters.

Everything else, attributes, preprocessor directives etc. is not supported.

## Method contract

To add a require clause to a method, put a line starting with `// Require:` below the method declaration and before the opening brace. To add an ensure clause put a line starting with `// Ensure:` below the method's closing brace.

For example:

````csharp
int N;

int Sum(int x, int y)
// Require: x >= 0 && x <= 256 
// Require: y >= 0 && y <= 256 
{
  N = x + y;
  return N;
}
// Ensure: N >= 0 && N <= 512
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

| Code          | Diagnostic               |
| ------------- | ------------------------ |
| MA0001        | Invalid ensure clause    |
| MA0002        | Invalid expression       |
| MA0003        | Invalid invariant clause |
| MA0004        | Invalid parameter        |
| MA0005        | Invalid require clause   |
| MA0006        | Invalid statement        |
| MA0007        | Error in class model     |
| MA0008        | Invariant violation      |
