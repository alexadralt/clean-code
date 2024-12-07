using System.Text;
using Markdown.AbstractSyntaxTree;
using Markdown.NodeView;
using Markdown.Parser;
using Markdown.ParseTree;
using Markdown.Token;
using Markdown.Tokenizer;

namespace Markdown;

public class Md : IRenderer
{
    private readonly Dictionary<MdTokenType, string> _tokenTags;
    
    private readonly MdTokenizer _tokenizer;
    private readonly MdParser _parser;

    public Md()
    {
        var tokenAliases = new Dictionary<string, MdTokenType>();
        tokenAliases.Add("_", MdTokenType.Italic);
        tokenAliases.Add("__", MdTokenType.Bold);
        tokenAliases.Add("# ", MdTokenType.Heading);
        tokenAliases.Add("\n", MdTokenType.Line);

        _tokenTags = new Dictionary<MdTokenType, string>();
        _tokenTags.Add(MdTokenType.Italic, "em");
        _tokenTags.Add(MdTokenType.Bold, "strong");
        _tokenTags.Add(MdTokenType.Heading, "h1");
        
        _tokenizer = new MdTokenizer(tokenAliases, '\\');
        _parser = new MdParser(new MdParseTree());
    }
    
    public string Render(string input)
    {
        var tokens = _tokenizer.Tokenize(input.AsMemory());
        var parseTree = _parser.Parse(tokens);
        var syntaxTree = MdAbstractSyntaxTree.FromParseTree(parseTree);
        return syntaxTree
            .Traverse()
            .Aggregate(new StringBuilder(),
                (sb, node) => ProcessNode(node, sb))
            .ToString();
    }

    private StringBuilder ProcessNode(BaseNodeView<MdTokenType>? node, StringBuilder sb)
    {
        if (node is AbstractSyntaxTreeNodeView<MdTokenType> nodeView)
        {
            if (nodeView.TokenType is MdTokenType.PlainText or MdTokenType.Document or MdTokenType.Line)
                sb.Append(nodeView.Text);
            else
                sb.Append($"<{_tokenTags[nodeView.TokenType]}>");
        }
        else if (node is ViewEnd<MdTokenType> viewEnd)
        {
            if (viewEnd.TokenType is not (MdTokenType.PlainText or MdTokenType.Document or MdTokenType.Line))
            {
                sb.Append($"</{_tokenTags[viewEnd.TokenType]}>");
            }
        }

        return sb;
    }
}