namespace Markdown.AbstractSyntaxTree;

public interface IAbstractSyntaxTree<TTokenType>
where TTokenType : Enum
{
    public void AddToken(TTokenType tokenType, ReadOnlyMemory<char>? tokenValue = null);
    public bool TryEndCurrentToken();
    public bool TryEndToken(TokenType tokenType);
    public string ToText();
}