using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Markdown.AbstractSyntaxTree;

namespace Markdown.Tokenizer;

public class MdTokenizer(
    ReadOnlyDictionary<string, TokenType> StartTokenAliases,
    ReadOnlyDictionary<string, TokenType> EndTokenAliases
    ) : ITokenizer<TokenType>
{
    public IAbstractSyntaxTree<TokenType> Tokenize(IAbstractSyntaxTree<TokenType> tree, ReadOnlyMemory<char> input)
    {
        ArgumentExceptionHelpers.ThrowIfFalse(
            MemoryMarshal.TryGetString(input, out var str, out var start, out var length),
            "Underlying object in the input argument is not a string");
        
        var foundPlainText = false;
        var plainTextStart = 0;
        var increment = 1;
        for (var i = start; i < length; )
        {
            if (TryMatchTokenAliases(str!, i, tree, out var tokenType, out var tokenAlias, out var endToken))
            {
                increment = tokenAlias.Length;
                
                if (endToken && tree.HasTokenInContext(tokenType))
                {
                    increment = tokenAlias.Length;
                    if (foundPlainText)
                        tree.EndToken(tokenType, input.Slice(plainTextStart, i - plainTextStart));
                    else
                    {
                        tree.EndToken(tokenType);
                        tree.AddToken(TokenType.PlainText, input.Slice(i, tokenAlias.Length));
                    }
                    foundPlainText = false;
                }
                else if (!endToken && !tree.HasTokenInContext(tokenType))
                {
                    if (tokenType != TokenType.Bold || !tree.HasTokenInContext(TokenType.Italic)) // двойное выделение не может быть внутри одинарного
                    {
                        if (foundPlainText)
                            tree.AddToken(TokenType.PlainText, input.Slice(plainTextStart, i - plainTextStart));
                        tree.AddToken(tokenType, input.Slice(i, tokenAlias.Length));
                        foundPlainText = false;
                    }
                }
                else
                    UpdatePlainTextState(ref foundPlainText, ref i, ref plainTextStart);
            }
            else
                UpdatePlainTextState(ref foundPlainText, ref i, ref plainTextStart);
            
            i += increment;
            if (increment > 1)
                increment = 1;
        }
        
        tree.EndToken(TokenType.Heading,
            foundPlainText ? input.Slice(plainTextStart, str!.Length - plainTextStart) : null);
        
        return tree;
    }

    private void UpdatePlainTextState(ref bool foundPlainText, ref int index, ref int plainTextStart)
    {
        if (!foundPlainText)
            plainTextStart = index;
        foundPlainText = true;
    }

    private bool TryMatchTokenAliases(
        string input,
        int index,
        IAbstractSyntaxTree<TokenType> tree,
        out TokenType tokenType,
        out string tokenAlias,
        out bool endToken)
    {
        var matchedEndToken = false;
        var mathcedStartToken = false;
        
        var startTokenType = default(TokenType);
        var endTokenType = default(TokenType);

        endToken = true;
        if (TryMatchTokenAliases(input, index, endToken, out var endTokenAlias))
            matchedEndToken = EndTokenAliases.TryGetValue(endTokenAlias, out endTokenType);
        
        endToken = false;
        if (TryMatchTokenAliases(input, index, endToken, out var startTokenAlias)
            && EnsureNotInSeparatedWords(input, index, startTokenAlias))
            mathcedStartToken = StartTokenAliases.TryGetValue(startTokenAlias, out startTokenType);

        if (matchedEndToken && mathcedStartToken)
        {
            if (startTokenAlias.Length > endTokenAlias.Length)
            {
                endToken = false;
                tokenAlias = startTokenAlias;
                tokenType = startTokenType;
                return true;
            }

            if (startTokenAlias.Length == endTokenAlias.Length)
            {
                // в этом случае startTokenType и endTokenType должны совпадать
                if (tree.HasTokenInContext(startTokenType))
                {
                    endToken = true;
                    tokenAlias = endTokenAlias;
                    tokenType = endTokenType;
                    return true;
                }

                endToken = false;
                tokenAlias = startTokenAlias;
                tokenType = startTokenType;
                return true;
            }

            endToken = true;
            tokenAlias = endTokenAlias;
            tokenType = endTokenType;
            return true;
        }

        if (matchedEndToken)
        {
            endToken = true;
            tokenAlias = endTokenAlias;
            tokenType = endTokenType;
            return true;
        }

        if (mathcedStartToken)
        {
            endToken = false;
            tokenAlias = startTokenAlias;
            tokenType = startTokenType;
            return true;
        }

        endToken = default;
        tokenAlias = default;
        tokenType = default;
        return false;
    }

    private bool TryMatchTokenAliases(
        string input,
        int index,
        bool endToken,
        out string tokenAlias)
    {
        tokenAlias = String.Empty;
        var tokenAliases = endToken ? EndTokenAliases : StartTokenAliases;
        foreach (var alias in tokenAliases.Keys)
        {
            if (TryMatchAlias(input, index, alias, endToken) && tokenAlias.Length < alias.Length)
                tokenAlias = alias;
        }

        return !String.IsNullOrEmpty(tokenAlias);
    }

    /*
     * для случаев типа: "сл_ово дру_гое слово", когда подчерки должны оставаться подчерками
     */
    private bool EnsureNotInSeparatedWords(string input, int index, string alias)
    {
        if (!IsInsideAWord(input, index, alias))
            return true;
        
        var i = index + alias.Length;
        while (i < input.Length && !IsWordDelimiter(input[i]))
        {
            if (TryMatchAlias(input, i, alias, true))
                return true;
            i++;
        }
        
        return false;
    }

    private bool TryMatchAlias(string input, int index, string alias, bool isEndToken)
    {
        return TryMatchPattern(input, index, alias)
               && (!IsBoldOrItalicAlias(alias, isEndToken)
                   || (IsInsideAWord(input, index, alias)
                       && !IsSurroundedByNumbers(input, index, alias))
                   || HasAWhiteSpaceNearIt(input, index, alias, isEndToken));
    }

    private bool IsWordDelimiter(char c)
    {
        return c is ' ' or '\t' or '\n' or '\r';
    }

    private bool IsInsideAWord(string input, int index, string alias)
    {
        var trueForLeftEdge = index - 1 >= 0 && input[index - 1] != ' ';
        var trueForRightEdge = index + alias.Length < input.Length
                               && input[index + alias.Length] != ' ';
        return trueForLeftEdge && trueForRightEdge;
    }

    private bool HasAWhiteSpaceNearIt(string input, int index, string alias, bool isEndToken)
    {
        var trueForLeftEdge = index - 1 < 0 || input[index - 1] == ' ';
        var trueForRightEdge = index + alias.Length >= input.Length
                               || input[index + alias.Length] == ' ';
        return isEndToken ? trueForRightEdge : trueForLeftEdge;
    }

    private bool IsSurroundedByNumbers(string input, int index, string alias)
    {
        var trueForLeftEdge = index - 1 >= 0
                              && int.TryParse(input[index - 1].ToString(), out _);
        var trueForRightEdge = index + alias.Length < input.Length
                               && int.TryParse(input[index + alias.Length].ToString(), out _);
        return trueForLeftEdge || trueForRightEdge;
    }

    private bool IsBoldOrItalicAlias(string alias, bool isEndToken)
    {
        var tokenAliases = isEndToken ? EndTokenAliases : StartTokenAliases;
        return tokenAliases[alias] == TokenType.Bold
               || tokenAliases[alias] == TokenType.Italic;
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