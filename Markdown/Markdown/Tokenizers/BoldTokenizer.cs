using Markdown.Tokens;

namespace Markdown.Tokenizers;

public class BoldTokenizer : MarkdownTokenizer
{
    protected override MarkdownTokenType MarkdownTokenType { get; } = MarkdownTokenType.ToBold;
    
    public override IEnumerable<Token> Tokenize(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}