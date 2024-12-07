using System.Collections.Immutable;
using Markdown.NodeView;
using Markdown.ParseTree;
using Markdown.SyntaxRules;
using Markdown.Token;

namespace Markdown.AbstractSyntaxTree;

public class MdAbstractSyntaxTree : IAbstractSyntaxTree<MdTokenType>
{
    private class Node
    {
        public Node(
            MdTokenType type,
            ReadOnlyMemory<char>? text = null,
            Node? parent = null)
        {
            Type = type;
            Text = text ?? ReadOnlyMemory<char>.Empty;
            Parent = parent;
            Children = new List<Node>();
        }
        
        public ReadOnlyMemory<char> Text { get; }
        public MdTokenType Type { get; }
        public List<Node> Children { get; set; }
        public Node? Parent { get; set; }
    }

    private readonly Node _root;
    private Node _current;

    private readonly ImmutableList<ISyntaxRule<MdTokenType>> _rules;
    
    private MdAbstractSyntaxTree()
    {
        _root = new Node(MdTokenType.Document);
        _current = _root;
        _rules = ImmutableList<ISyntaxRule<MdTokenType>>.Empty;
    }

    private MdAbstractSyntaxTree(
        MdAbstractSyntaxTree tree, ISyntaxRule<MdTokenType> rule)
    {
        _root = tree._root;
        _current = _root;
        _rules = tree._rules.Add(rule);
    }
    
    public static MdAbstractSyntaxTree FromParseTree(IParseTree<MdTokenType> parseTree)
    {
        var syntaxTree = new MdAbstractSyntaxTree();
        foreach (var baseView in parseTree.Traverse())
        {
            if (baseView is ParseTreeNodeView<MdTokenType> nodeView)
            {
                if (nodeView.TokenType != MdTokenType.Document)
                {
                    if (nodeView.Complete)
                    {
                        var newNode = new Node(nodeView.TokenType, nodeView.Text, syntaxTree._current);
                        if (nodeView.TokenType != MdTokenType.PlainText)
                            syntaxTree.AddNode(newNode);
                        else
                            syntaxTree._current.Children.Add(newNode);
                    }
                    else
                    {
                        var newNode = new Node(MdTokenType.PlainText, nodeView.Text, syntaxTree._current);
                        syntaxTree._current.Children.Add(newNode);
                    }
                }
            }
            else if (baseView is ViewEnd<MdTokenType>)
                syntaxTree.EndCurrentNode();
            else
                throw new InvalidOperationException("Unexpected node type");
        }
        return syntaxTree;
    }

    private void AddNode(Node node)
    {
        _current.Children.Add(node);
        _current = node;
    }

    private void EndCurrentNode()
    {
        if (_current != _root)
            _current = _current.Parent!;
    }
    
    public IEnumerable<BaseNodeView<MdTokenType>> Traverse()
    {
        return Traverse(_root);
    }
    
    private static IEnumerable<BaseNodeView<MdTokenType>> Traverse(Node node)
    {
        yield return new AbstractSyntaxTreeNodeView<MdTokenType>(node.Text, node.Type);
        var childNodes = node.Children.SelectMany(Traverse).ToList();
        foreach (var childNode in childNodes)
            yield return childNode;
        if (childNodes.Count > 0)
            yield return new ViewEnd<MdTokenType>(node.Type);
    }

    public IAbstractSyntaxTree<MdTokenType> AddRule(ISyntaxRule<MdTokenType> rule)
    {
        return new MdAbstractSyntaxTree(this, rule);
    }

    public IAbstractSyntaxTree<MdTokenType> ApplyRules()
    {
        var newSyntaxTree = new MdAbstractSyntaxTree();
        foreach (var baseView in Traverse())
        {
            if (baseView is AbstractSyntaxTreeNodeView<MdTokenType> nodeView
                && nodeView.TokenType != MdTokenType.Document)
            {
                var parentNodeView = new AbstractSyntaxTreeNodeView<MdTokenType>(
                    newSyntaxTree._current.Text, newSyntaxTree._current.Type);
                foreach (var rule in _rules)
                {
                    var result = rule.Apply(nodeView, parentNodeView);
                    
                }
            }
        }
        
        return newSyntaxTree;
    }
}