namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Represents the context to use when calculating a location.
/// </summary>
internal class LocationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocationContext"/> class.
    /// </summary>
    /// <param name="trivia">The trivia where the location context begins.</param>
    /// <param name="header">The header in the trivia.</param>
    /// <param name="offset">The offset of non significant data after the header.</param>
    public LocationContext(SyntaxTrivia trivia, string header, int offset)
    {
        Trivia = trivia;
        Header = header;
        Offset = offset;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationContext"/> class.
    /// </summary>
    /// <param name="rootNode">The syntax node where the location context begins.</param>
    public LocationContext(SyntaxNode rootNode)
    {
        RootNode = rootNode;
    }

    /// <summary>
    /// Gets the node location using the context as a starting point.
    /// </summary>
    /// <param name="node">The node.</param>
    public Location GetLocation(SyntaxNode node)
    {
        return GetLocation(node.GetLocation().SourceSpan);
    }

    /// <summary>
    /// Gets the token location using the context as a starting point.
    /// </summary>
    /// <param name="token">The token.</param>
    public Location GetLocation(SyntaxToken token)
    {
        return GetLocation(token.GetLocation().SourceSpan);
    }

    private Location GetLocation(TextSpan textSpan)
    {
        SyntaxTree SourceTree;
        TextSpan ContextSpan;
        Location Result = null!;

        if (Header is not null)
        {
            SourceTree = Trivia.GetLocation().SourceTree!;
            TextSpan TriviaTextSpan = Trivia.GetLocation().SourceSpan;
            ContextSpan = new TextSpan(TriviaTextSpan.Start + Header.Length, TriviaTextSpan.Length - Header.Length);
            TextSpan ResultSpan = new TextSpan(ContextSpan.Start + textSpan.Start - Offset, textSpan.Length);
            Result = Location.Create(SourceTree, ResultSpan);
        }

        if (RootNode is not null)
        {
            SourceTree = RootNode.GetLocation().SourceTree!;
            Result = Location.Create(SourceTree, textSpan);
        }

        return Result;
    }

    private SyntaxTrivia Trivia;
    private string? Header;
    private int Offset;
    private SyntaxNode? RootNode;
}
