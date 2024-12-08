using System.Runtime.InteropServices;
using Markdown.Token;

namespace Markdown.Tokenizer;

public class MdTokenizer(Dictionary<string, MdTokenType> tokenAliases, char escapeCharacter) : ITokenizer<MdTokenType, MdToken>
{
    public IEnumerable<MdToken> Tokenize(ReadOnlyMemory<char> input)
    {
        ArgumentExceptionHelpers.ThrowIfFalse(
            MemoryMarshal.TryGetString(input, out var str, out var start, out var length),
            "Underlying object in the input argument is not a string");
        
        var foundPlainText = false;
        var plainTextStart = 0;
        var increment = 1;
        for (var i = start; i < start + length; )
        {
            if (escapeCharacter == str![i] && i + 1 < str.Length)
            {
                if (TryMatchTokenAliases(str, i + 1, out _, out _, out _))
                {
                    increment = 2;
                    yield return new MdToken(MdTokenType.PlainText, MdTokenBehaviour.Undefined,
                        input.Slice(plainTextStart, i - plainTextStart));
                    yield return new MdToken(MdTokenType.PlainText, MdTokenBehaviour.Undefined, input.Slice(i + 1, 1));
                    foundPlainText = false;
                }
            }
            else if (TryMatchTokenAliases(str, i, out var tokenType, out var tokenAlias, out var tokenBehaviour))
            {
                increment = tokenAlias.Length;
                
                if (foundPlainText)
                    yield return new MdToken(MdTokenType.PlainText, MdTokenBehaviour.Undefined,
                        input.Slice(plainTextStart, i - plainTextStart));
                
                yield return new MdToken(tokenType, tokenBehaviour, input.Slice(i, tokenAlias.Length));
                
                foundPlainText = false;
            }
            else
            {
                if (!foundPlainText)
                    plainTextStart = i;
                foundPlainText = true;
            }
            
            i += increment;
            if (increment > 1)
                increment = 1;
        }
        
        if (foundPlainText)
            yield return new MdToken(MdTokenType.PlainText, MdTokenBehaviour.Undefined,
                input.Slice(plainTextStart, str!.Length - plainTextStart));
    }

    private bool TryMatchTokenAliases(
        string input,
        int index,
        out MdTokenType mdTokenType,
        out string tokenAlias,
        out MdTokenBehaviour tokenBehaviour)
    {
        var matchedClosingToken = false;
        var mathcedOpeningToken = false;
        
        var openingTokenType = default(MdTokenType);
        var closingTokenType = default(MdTokenType);

        if (TryMatchTokenAliases(input, index, true, out var closingTokenAlias))
            matchedClosingToken = tokenAliases.TryGetValue(closingTokenAlias, out closingTokenType);
        
        if (TryMatchTokenAliases(input, index, false, out var openingTokenAlias))
            mathcedOpeningToken = tokenAliases.TryGetValue(openingTokenAlias, out openingTokenType);

        if (mathcedOpeningToken && matchedClosingToken)
        {
            var (alias, type, behaviour) = openingTokenAlias.Length > closingTokenAlias.Length
                ? (openingTokenAlias, openingTokenType, MdTokenBehaviour.Opening)
                : (closingTokenAlias, closingTokenType, MdTokenBehaviour.Closing);
            
            if (IsInsideAWord(input, index, alias))
                tokenBehaviour = MdTokenBehaviour.InsideAWord;
            else
                tokenBehaviour = behaviour;
            tokenAlias = alias;
            mdTokenType = type;
            return true;
        }

        if (mathcedOpeningToken)
        {
            tokenBehaviour = MdTokenBehaviour.Opening;
            tokenAlias = openingTokenAlias;
            mdTokenType = openingTokenType;
            return true;
        }

        if (matchedClosingToken)
        {
            tokenBehaviour = MdTokenBehaviour.Closing;
            tokenAlias = closingTokenAlias;
            mdTokenType = closingTokenType;
            return true;
        }
        
        tokenBehaviour = MdTokenBehaviour.Undefined;
        tokenAlias = String.Empty;
        mdTokenType = MdTokenType.PlainText;
        return false;
    }
    
    private bool TryMatchTokenAliases(
        string input,
        int index,
        bool endToken,
        out string tokenAlias)
    {
        tokenAlias = String.Empty;
        foreach (var alias in tokenAliases.Keys)
        {
            if (TryMatchAlias(input, index, alias, endToken) && tokenAlias.Length < alias.Length)
                tokenAlias = alias;
        }

        return !String.IsNullOrEmpty(tokenAlias);
    }
    
    private bool TryMatchAlias(string input, int index, string alias, bool isEndToken)
    {
        return TryMatchPattern(input, index, alias)
               && HasANonDelimiterCharacterNearIt(input, index, alias, isEndToken);
    }

    private bool IsWordDelimiter(char c)
    {
        return c is ' ' or '\t' or '\n' or '\r' or ',' or '.'
            or '!' or '?';
    }

    private bool IsInsideAWord(string input, int index, string alias)
    {
        var trueForLeftEdge = index - 1 >= 0 && !IsWordDelimiter(input[index - 1]);
        var trueForRightEdge = index + alias.Length < input.Length
                               && !IsWordDelimiter(input[index + alias.Length]);
        return trueForLeftEdge && trueForRightEdge;
    }

    private bool HasANonDelimiterCharacterNearIt(string input, int index, string alias, bool closingToken)
    {
        var trueForOpening = index + alias.Length < input.Length
                             && !IsWordDelimiter(input[index + alias.Length]);
        var trueForClosing = index - 1 >= 0
                             && !IsWordDelimiter(input[index - 1]);
        return closingToken ? trueForClosing : trueForOpening;
    }

    private bool TryMatchPattern(string input, int index, string pattern)
    {
        int i = 0;
        foreach (var ch in pattern)
        {
            if (index + i >= input.Length || ch != input[index + i])
                return false;
            i++;
        }

        return true;
    }
}