using System.Text;
using Markdown.Tokens;

namespace Markdown.TokenConverters;

public class ItalicHtmlConverter : HtmlTokenConverter
{
    protected override MarkdownTokenType MarkdownTokenType { get; } = MarkdownTokenType.ToItalic;
    public override void ProcessTokens(IEnumerable<Token> tokens, StringBuilder context)
    {
        throw new NotImplementedException();
    }
}