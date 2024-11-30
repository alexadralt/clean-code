using System.Collections.ObjectModel;
using System.Text;

namespace Markdown.AbstractSyntaxTree;

public class MdAbstractSyntaxTree : IAbstractSyntaxTree<TokenType>
{
    private class Node
    {
        public Node()
        {
            Children = new List<Node>();
        }

        public Node(TokenType tokenType, ReadOnlyMemory<char>? tokenValue)
        {
            TokenType = tokenType;
            TokenValue = tokenValue;
            Children = new List<Node>();
        }
        
        public TokenType? TokenType { get; }
        public ReadOnlyMemory<char>? TokenValue { get; }
        public Node? Parent { get; private set; }
        private List<Node> Children { get; }

        public void AddChild(Node node)
        {
            node.Parent = this;
            Children.Add(node);
        }

        public IEnumerable<Node> GetChildren() => Children;
    }

    private readonly ReadOnlyDictionary<TokenType, string> _tokenTags;
    private readonly Node _root;
    private Node _current;

    public MdAbstractSyntaxTree(ReadOnlyDictionary<TokenType, string> tokenTags)
    {
        _tokenTags = tokenTags;
        _root = new Node();
        _current = _root;
    }
    
    public void AddToken(TokenType tokenType, ReadOnlyMemory<char>? tokenValue = null)
    {
        if (tokenType == TokenType.PlainText)
        {
            ArgumentExceptionHelpers.ThrowIfNull(tokenValue, "tokenValue must not be null");
            _current.AddChild(new Node(tokenType, tokenValue));
        }
        else
        {
            var newNode = new Node(tokenType, null);
            _current.AddChild(newNode);
            _current = newNode;
        }
    }

    public bool TryEndCurrentToken()
    {
        if (_current == _root)
            return false;
        _current = _current.Parent!;
        return true;
    }

    public bool TryEndToken(TokenType tokenType)
    {
        var tokenNode = FindTokenNode(_current, tokenType);
        if (tokenNode != null)
        {
            _current = tokenNode.Parent!;
            return true;
        }
        
        return false;
    }

    private Node? FindTokenNode(Node node, TokenType tokenType)
    {
        if (node == _root)
            return null;
        if (node.TokenType == tokenType)
            return node;
        
        return FindTokenNode(node.Parent!, tokenType);
    }

    public string ToText()
    {
        var sb = new StringBuilder();
        ProcessChildren(_root, sb);
        return sb.ToString();
    }

    private void ProcessChildren(Node node, StringBuilder sb)
    {
        foreach (var child in node.GetChildren())
        {
            if (child.TokenType == TokenType.PlainText)
                sb.Append(child.TokenValue);
            else
                SurroundWithTag(_tokenTags[child.TokenType!.Value], child, sb);
        }
    }

    private void SurroundWithTag(string tag, Node node, StringBuilder sb)
    {
        sb.Append($"<{tag}>");
        ProcessChildren(node, sb);
        sb.Append($"</{tag}>");
    }
}