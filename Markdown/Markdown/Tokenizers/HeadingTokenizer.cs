using Markdown.Tokens;

namespace Markdown.Tokenizers;

public class HeadingTokenizer : MarkdownTokenizer
{
    protected override MarkdownTokenType MarkdownTokenType { get; } = MarkdownTokenType.ToHeading;

    public override IEnumerable<Token> Tokenize(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}