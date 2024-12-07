using Markdown.AbstractSyntaxTree;

namespace Markdown.SyntaxRules;

public interface ISyntaxRule<TTokenType>
{
    public AbstractSyntaxTreeNodeView<TTokenType> Apply(
        AbstractSyntaxTreeNodeView<TTokenType> node,
        AbstractSyntaxTreeNodeView<TTokenType> parentNode);
}