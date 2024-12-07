using System.Collections.ObjectModel;
using System.Text;
using Markdown.Token;

namespace Markdown.AbstractSyntaxTree;

public class MdAbstractSyntaxTree_
{
    public class Node
    {
        public Node()
        {
            Children = new List<Node>();
        }

        public Node(MdTokenType tokenType, ReadOnlyMemory<char>? tokenValue)
        {
            TokenType = tokenType;
            TokenValue = tokenValue;
            Children = new List<Node>();
        }
        
        public MdTokenType? TokenType { get; set; }
        public ReadOnlyMemory<char>? TokenValue { get; }
        public Node? Parent { get; set; }
        public List<Node> Children { get; }

        public void AddChild(Node node)
        {
            node.Parent = this;
            Children.Add(node);
        }

        public IEnumerable<Node> RemoveChildren()
        {
            var children = new List<Node>(Children);
            Children.Clear();
            return children;
        }
    }

    private readonly ReadOnlyDictionary<MdTokenType, string> _tokenTags;
    private readonly Node _root;
    private Node _current;

    public MdAbstractSyntaxTree_(ReadOnlyDictionary<MdTokenType, string> tokenTags)
    {
        _tokenTags = tokenTags;
        _root = new Node();
        _current = _root;
    }
    
    public void AddToken(MdTokenType mdTokenType, ReadOnlyMemory<char> tokenValue)
    {
        ArgumentExceptionHelpers.ThrowIfNull(tokenValue, "tokenValue must not be null");
        if (mdTokenType == MdTokenType.PlainText)
        {
            _current.AddChild(new Node(mdTokenType, tokenValue));
        }
        else
        {
            var newNode = new Node(mdTokenType, tokenValue);
            _current.AddChild(newNode);
            _current = newNode;
        }
    }

    public bool HasTokenInContext(MdTokenType mdTokenType) => HasParent(mdTokenType, _current);

    private bool HasParent(MdTokenType mdTokenType, Node node)
    {
        if (node == _root)
            return false;
        if (node.TokenType == mdTokenType)
            return true;
        return HasParent(mdTokenType, node.Parent!);
    }

    public void EndToken(MdTokenType? mdTokenType = null)
    {
        WalkUpToTheRoot(_current, mdTokenType);
    }

    public List<Node> GetChildrenForCurrentToken()
    {
        return _current.Children;
    }

    private void WalkUpToTheRoot(Node node, MdTokenType? mdTokenType)
    {
        if (node == _root)
            _current = _root;
        else if (node.TokenType == mdTokenType)
        {
            if (node.TokenType == MdTokenType.Italic)
            {
                var childNodes = new List<Node>(node.Children);
                for (var i = 0; i < childNodes.Count; i++)
                {
                    var childNode = childNodes[i];
                    if (childNode.TokenType == MdTokenType.Bold)
                    {
                        var children = childNode.RemoveChildren().ToList();
                        foreach (var child in children)
                        {
                            node.Children.Insert(i + 1, child);
                            child.Parent = node;
                        }
                        childNode.TokenType = MdTokenType.PlainText;
                        node.Children.Insert(i + 1 + children.Count, childNode);
                    }
                }
            }
            _current = node.Parent!;
        }
        else
        {
            var parent = node.Parent!;
            var children = node.RemoveChildren();
            foreach (var child in children)
                parent.AddChild(child);
            WalkUpToTheRoot(parent, mdTokenType);
        }
    }
    
    public void ReviseForIntersection(MdTokenType tokenType, ReadOnlyMemory<char> tokenText)
    {
        throw new NotImplementedException();
    }

    public string ToText()
    {
        var sb = new StringBuilder();
        ProcessChildren(_root, sb);
        return sb.ToString();
    }

    private void ProcessChildren(Node node, StringBuilder sb)
    {
        foreach (var child in node.Children)
        {
            if (child.TokenType == MdTokenType.PlainText || child.Children.Count == 0)
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