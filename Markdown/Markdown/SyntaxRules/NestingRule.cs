using Markdown.AbstractSyntaxTree;
using Markdown.Token;

namespace Markdown.SyntaxRules;

public class NestingRule : ISyntaxRule<MdTokenType>
{
    public (AbstractSyntaxTreeNodeView<MdTokenType> result, bool shouldCopy) Apply(
        AbstractSyntaxTreeNodeView<MdTokenType> node,
        AbstractSyntaxTreeNodeView<MdTokenType> parentNode,
        AbstractSyntaxTreeNodeView<MdTokenType>? leftNeighbour,
        AbstractSyntaxTreeNodeView<MdTokenType>? rightNeighbour)
    {
        if (node.TokenType == MdTokenType.Bold && parentNode.TokenType == MdTokenType.Italic)
            return (new AbstractSyntaxTreeNodeView<MdTokenType>(node.Text, MdTokenType.PlainText), true);
        
        return (node, false);
    }
}