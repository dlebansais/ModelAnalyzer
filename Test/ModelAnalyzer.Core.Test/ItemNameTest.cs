namespace Core.Test;

public record ItemNameTest
{
    required public string Text { get; set; }

    public static implicit operator string(ItemNameTest itemName) => itemName.Text;
    public static implicit operator ItemNameTest(string text) => new ItemNameTest() { Text = text };
}
