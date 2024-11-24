using Markdown.Tokens;

namespace Markdown.Tokenizers;

public interface ITokenizer
{
    public IEnumerable<Token> Tokenize(ReadOnlySpan<char> input);
}