using Markdown.Extensions;
using Markdown.NodeView;
using Markdown.Token;

namespace Markdown.SyntaxRules;

public class NumberRule : ISyntaxRule<MdTokenType>
{
    public INodeView<MdTokenType> Apply(INodeView<MdTokenType> nodeView)
    {
        for (var i = 0; i < nodeView.Children.Count; i++)
        {
            var childNode = nodeView.Children[i];
            if (childNode is { InsideWord: true, Type: MdTokenType.Bold or MdTokenType.Italic }
                && childNode.Children.Any(
                    n => n.Text.ContainsNumber()))
            {
                childNode.Type = MdTokenType.PlainText;
                foreach (var toMove in childNode.Children.AsEnumerable().Reverse())
                {
                    nodeView.Children.Insert(i + 1, toMove);
                    toMove.Parent = nodeView;
                }
                
                nodeView.Children.Insert(i + 1 + childNode.Children.Count, childNode);
                childNode.Children.Clear();
            }
            
            Apply(childNode);
        }
        
        return nodeView;
    }
}