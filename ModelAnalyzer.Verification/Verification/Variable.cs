﻿namespace ModelAnalyzer;

/// <summary>
/// Represents a variable.
/// </summary>
internal record Variable(IVariableName VariableName, ExpressionType VariableType);
