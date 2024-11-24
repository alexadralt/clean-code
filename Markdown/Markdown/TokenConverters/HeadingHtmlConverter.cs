using System.Text;
using Markdown.Tokens;

namespace Markdown.TokenConverters;

public class HeadingHtmlConverter : HtmlTokenConverter
{
    protected override MarkdownTokenType MarkdownTokenType { get; } = MarkdownTokenType.ToHeading;
    public override void ProcessTokens(IEnumerable<Token> tokens, StringBuilder context)
    {
        throw new NotImplementedException();
    }
}