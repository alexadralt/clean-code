using System.Text;
using Markdown.TokenConverters;
using Markdown.Tokenizers;
using Markdown.Tokens;

namespace Markdown;

public class Md : IMd
{
    private readonly ITokenizer[] _tokenizers =
    {
        new BoldTokenizer(),
        new ItalicTokenizer(),
        new HeadingTokenizer()
    };
    private readonly ITokenConverter[] _converters =
    {
        new BoldHtmlConverter(),
        new ItalicHtmlConverter(),
        new HeadingHtmlConverter()
    };
    
    private readonly int _sliceSize = 1024;
    
    public string Render(string markdown)
    {
        var markdownSpan = markdown.AsSpan();
        var context = new StringBuilder();
        var stepCount = markdownSpan.Length / _sliceSize;
        stepCount = markdownSpan.Length % _sliceSize == 0 ? stepCount : stepCount + 1;
        var tokenList = new List<Token>();
        for (var step = 0; step < stepCount; step++)
        {
            var sliceStart = step * _sliceSize;
            var sliceSize = Math.Min(_sliceSize, markdownSpan.Length - sliceStart);
            var stepSpan = markdownSpan.Slice(sliceStart, sliceSize);
            foreach (var tokenizer in _tokenizers)
                tokenList.AddRange(tokenizer.Tokenize(stepSpan));
            foreach (var converter in _converters)
                converter.ProcessTokens(tokenList, context);
            tokenList.Clear();
        }
        
        return context.ToString();
    }
}