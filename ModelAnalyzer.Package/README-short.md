# ModelAnalyzer

Roslyn-based analysis of class models with informal contracts.

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
