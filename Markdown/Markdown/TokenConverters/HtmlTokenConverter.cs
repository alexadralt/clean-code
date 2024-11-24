using System.Text;
using Markdown.Tokens;

namespace Markdown.TokenConverters;

public abstract class HtmlTokenConverter : ITokenConverter
{
    protected IEnumerable<Token>? TokensOnPreviousSlice { get; set; }
    protected abstract MarkdownTokenType MarkdownTokenType { get; }
    public abstract void ProcessTokens(IEnumerable<Token> tokens, StringBuilder context);
}