using Markdown.AbstractSyntaxTree;

namespace Markdown.SyntaxRules;

public interface ISyntaxRule<TTokenType>
{
    public (AbstractSyntaxTreeNodeView<TTokenType> result, bool shouldCopy) Apply(
        AbstractSyntaxTreeNodeView<TTokenType> node,
        AbstractSyntaxTreeNodeView<TTokenType> parentNode,
        AbstractSyntaxTreeNodeView<TTokenType>? leftNeighbour,
        AbstractSyntaxTreeNodeView<TTokenType>? rightNeighbour);
}