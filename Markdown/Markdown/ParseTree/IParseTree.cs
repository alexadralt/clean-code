using Markdown.NodeView;
using Markdown.Traversable;

namespace Markdown.ParseTree;

public interface IParseTree<TTokenType> : ITraversable<BaseNodeView<TTokenType>>
{
    public ParseTreeNodeView<TTokenType> CurrentToken { get; }
    public void OpenToken(TTokenType tokenType, ReadOnlyMemory<char> text);
    public void CloseCurrentToken(bool complete);
}