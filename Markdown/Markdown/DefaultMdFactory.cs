using Markdown.Parser;
using Markdown.ParseTree;
using Markdown.SyntaxRules;
using Markdown.Token;
using Markdown.Tokenizer;

namespace Markdown;

public static class DefaultMdFactory
{
    private static readonly char[] _delimiters = { ' ', '\t', '\n', '\r', ',', '.', '!', '?' };
    
    private static readonly Dictionary<string, MdTokenType> _tokenAliases = new Dictionary<string, MdTokenType>
    {
        { "_", MdTokenType.Italic },
        { "__", MdTokenType.Bold },
        { "# ", MdTokenType.Heading },
        { "\n", MdTokenType.Line }
    };
    
    private static readonly Dictionary<MdTokenType, string> _tokenTags = new Dictionary<MdTokenType, string>
    {
        { MdTokenType.Italic, "em" },
        { MdTokenType.Bold, "strong" },
        { MdTokenType.Heading, "h1" }
    };
    
    private static readonly List<ISyntaxRule<MdTokenType>> _syntaxRules = new List<ISyntaxRule<MdTokenType>>
    {
        new NestingRule(),
        new NumberRule(),
        new TokensInDifferentWordsRule()
    };
    
    public static Md CreateMd()
    {
        return new Md(
            _tokenTags,
            new MdTokenizer(
                _tokenAliases, '\\', _delimiters),
            new MdParser(new MdParseTree()),
            _syntaxRules.ToArray());
    }
}