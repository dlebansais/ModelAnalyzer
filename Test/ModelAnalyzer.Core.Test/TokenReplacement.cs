namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Threading;

internal class TokenReplacement : IDisposable
{
    public static TokenReplacement None { get; } = new TokenReplacement();

    private TokenReplacement()
    {
        Token = default;
        NewKind = SyntaxKind.None;
    }

    public TokenReplacement(SyntaxToken token, SyntaxKind newKind)
    {
        Token = token;
        NewKind = newKind;
    }

    public SyntaxToken Token { get; }
    public SyntaxKind NewKind { get; }

    public bool IsReplaced => OldKind != SyntaxKind.None;

    public void Replace()
    {
        if (NewKind != SyntaxKind.None && OldKind == SyntaxKind.None)
        {
            TokenManipulationMutex.WaitOne();

            OldKind = ChangeTokenKind(Token, NewKind);
        }
    }

    public void Restore()
    {
        if (OldKind != SyntaxKind.None)
        {
            ChangeTokenKind(Token, OldKind);
            OldKind = SyntaxKind.None;

            TokenManipulationMutex.ReleaseMutex();
        }
    }

    public void Dispose()
    {
        Restore();
    }

    private SyntaxKind OldKind = SyntaxKind.None;

    private static SyntaxKind ChangeTokenKind(SyntaxToken token, SyntaxKind newKind)
    {
        Type TokenType = typeof(SyntaxToken);
        PropertyInfo NodeProperty = TokenType.GetProperty("Node", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo KindField = NodeProperty.PropertyType.GetField("_kind", BindingFlags.Instance | BindingFlags.NonPublic)!;

        object Node = NodeProperty.GetValue(token)!;
        SyntaxKind OldKind = (SyntaxKind)(ushort)KindField.GetValue(Node)!;
        KindField.SetValue(Node, newKind);

        return OldKind;
    }

    // When replacing a token, the replacement is for all compiled programs. Therefore, use this mutex to compile programs only one at a time, and make sure you restore the replaced token once done.
    private static Mutex TokenManipulationMutex = new();
}
