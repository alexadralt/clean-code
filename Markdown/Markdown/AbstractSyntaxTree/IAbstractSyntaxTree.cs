namespace Markdown.AbstractSyntaxTree;

public interface IAbstractSyntaxTree<TTokenType>
where TTokenType : Enum
{
    public void AddToken(TTokenType tokenType, ReadOnlyMemory<char> tokenValue);
    public bool HasTokenInContext(TTokenType tokenType);
    public void EndToken(TTokenType tokenType, ReadOnlyMemory<char>? tokenValue = null);
    public string ToText();
}