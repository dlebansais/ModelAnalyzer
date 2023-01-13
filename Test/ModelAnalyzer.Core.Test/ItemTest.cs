namespace Core.Test;

using ModelAnalyzer;

public record ItemTest : INameable<ItemNameTest>
{
    /// <inheritdoc/>
    required public ItemNameTest Name { get; set; }
}
