using System.Text;
using Markdown.Tokens;

namespace Markdown.TokenConverters;

public interface ITokenConverter
{
    public void ProcessTokens(IEnumerable<Token> tokens, StringBuilder context);
}