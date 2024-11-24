using System.Text;
using Markdown.Tokens;

namespace Markdown.TokenConverters;

public class BoldHtmlConverter : HtmlTokenConverter
{
    protected override MarkdownTokenType MarkdownTokenType { get; } = MarkdownTokenType.ToBold;
    public override void ProcessTokens(IEnumerable<Token> tokens, StringBuilder context)
    {
        throw new NotImplementedException();
    }
}