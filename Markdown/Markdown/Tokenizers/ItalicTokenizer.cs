using System.Text;
using Markdown.Tokens;

namespace Markdown.Tokenizers;

public class ItalicTokenizer : MarkdownTokenizer
{
    protected override MarkdownTokenType MarkdownTokenType { get; } = MarkdownTokenType.ToItalic;
    
    public override IEnumerable<Token> Tokenize(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}