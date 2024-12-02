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

        public IEnumerable<Node> RemoveChildren()
        {
            var children = new List<Node>(GetChildren());
            Children.Clear();
            return children;
        }

        public IEnumerable<Node> GetChildren() => Children.AsReadOnly();
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
    
    public void AddToken(TokenType tokenType, ReadOnlyMemory<char> tokenValue)
    {
        ArgumentExceptionHelpers.ThrowIfNull(tokenValue, "tokenValue must not be null");
        if (tokenType == TokenType.PlainText)
        {
            _current.AddChild(new Node(tokenType, tokenValue));
        }
        else
        {
            var newNode = new Node(tokenType, tokenValue);
            _current.AddChild(newNode);
            _current = newNode;
        }
    }

    public bool HasTokenInContext(TokenType tokenType) => HasParent(tokenType, _current);

    private bool HasParent(TokenType tokenType, Node node)
    {
        if (node == _root)
            return false;
        if (node.TokenType == tokenType)
            return true;
        return HasParent(tokenType, node.Parent!);
    }

    public void EndToken(TokenType tokenType, ReadOnlyMemory<char>? tokenValue = null)
    {
        WalkUpToTheRoot(_current, tokenType, tokenValue);
    }

    private void WalkUpToTheRoot(Node node, TokenType tokenType, ReadOnlyMemory<char>? tokenValue)
    {
        if (node == _root)
        {
            AddTextToNodeAndMakeCurrent(node, tokenValue);
        }
        else if (node.TokenType == tokenType)
        {
            AddTextToNodeAndMakeCurrent(node, tokenValue);
            _current = node.Parent!;
        }
        else
        {
            var parent = node.Parent!;
            var children = node.RemoveChildren();
            foreach (var child in children)
                parent.AddChild(child);
            WalkUpToTheRoot(parent, tokenType, tokenValue);
        }
    }

    private void AddTextToNodeAndMakeCurrent(Node node, ReadOnlyMemory<char>? tokenValue)
    {
        _current = node;
        if (tokenValue != null)
            AddToken(TokenType.PlainText, tokenValue.Value);
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
            if (child.TokenType == TokenType.PlainText || !child.GetChildren().Any())
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