using System.Text;
using Markdown.Tokens;

namespace Markdown.Tokenizers;

public abstract class MarkdownTokenizer : ITokenizer
{
    protected abstract MarkdownTokenType MarkdownTokenType { get; }
    public abstract IEnumerable<Token> Tokenize(ReadOnlySpan<char> input);
}