namespace ModelAnalyzer.Core.Test;

public record ItemTest : INameable<ItemNameTest>
{
    /// <inheritdoc/>
    required public ItemNameTest Name { get; set; }
}
