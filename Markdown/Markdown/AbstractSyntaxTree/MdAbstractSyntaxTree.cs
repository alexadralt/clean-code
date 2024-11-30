using System.Collections.ObjectModel;

namespace Markdown.AbstractSyntaxTree;

public class MdAbstractSyntaxTree(ReadOnlyDictionary<TokenType, string> TokenConverters) : IAbstractSyntaxTree<TokenType>
{
    private struct Node
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
        
        public TokenType? TokenType;
        public ReadOnlyMemory<char>? TokenValue;
        public List<Node> Children;
    }

    private Node _root = new();
    
    public void AddToken(TokenType tokenType, ReadOnlyMemory<char>? tokenValue = null)
    {
        throw new NotImplementedException();
    }

    public string ToText()
    {
        throw new NotImplementedException();
    }
}