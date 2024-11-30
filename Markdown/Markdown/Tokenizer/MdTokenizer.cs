using System.Collections.ObjectModel;
using Markdown.AbstractSyntaxTree;

namespace Markdown.Tokenizer;

public class MdTokenizer(ReadOnlyDictionary<string, TokenType> TokenAliases) : ITokenizer<TokenType>
{
    public IAbstractSyntaxTree<TokenType> Tokenize(IAbstractSyntaxTree<TokenType> tree, ReadOnlyMemory<char> input)
    {
        throw new NotImplementedException();
    }
}