using System.Collections.ObjectModel;
using Markdown.AbstractSyntaxTree;
using Markdown.Tokenizer;

namespace Markdown;

public class Md : IMd
{
    private readonly ReadOnlyDictionary<string, TokenType> _tokenAliases;
    private readonly ReadOnlyDictionary<string, TokenType> _endTokenAliases;
    private readonly ReadOnlyDictionary<TokenType, string> _tokenTags;

    public Md()
    {
        var tokenAliases = new Dictionary<string, TokenType>();
        tokenAliases.Add("_", TokenType.Italic);
        tokenAliases.Add("__", TokenType.Bold);
        tokenAliases.Add("# ", TokenType.Heading);
        _tokenAliases = tokenAliases.AsReadOnly();
        
        var endTokenAliases = new Dictionary<string, TokenType>();
        endTokenAliases.Add("_", TokenType.Italic);
        endTokenAliases.Add("__", TokenType.Bold);
        endTokenAliases.Add("\n", TokenType.Heading);
        _endTokenAliases = endTokenAliases.AsReadOnly();

        var tokenTags = new Dictionary<TokenType, string>();
        tokenTags.Add(TokenType.Italic, "em");
        tokenTags.Add(TokenType.Bold, "strong");
        tokenTags.Add(TokenType.Heading, "h1");
        _tokenTags = tokenTags.AsReadOnly();
    }
    
    public string Render(string input)
    {
        var tokenizer = new MdTokenizer(_tokenAliases, _endTokenAliases);
        var syntaxTree = tokenizer.Tokenize(new MdAbstractSyntaxTree(_tokenTags), input.AsMemory());
        return syntaxTree.ToText();
    }
}