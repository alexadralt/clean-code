using Markdown.AbstractSyntaxTree;

namespace Markdown.Tokenizer;

public interface ITokenizer<TTokenType>
where TTokenType : Enum
{
    public IAbstractSyntaxTree<TTokenType> Tokenize(IAbstractSyntaxTree<TTokenType> tree, ReadOnlyMemory<char> input);
}