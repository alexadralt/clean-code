namespace Markdown.Tokens;

public readonly struct Token(
    ReadOnlyMemory<char> content,
    MarkdownTokenType markdownTokenType,
    int startIndex,
    int endIndex)
{
    public ReadOnlyMemory<char> Content { get; } = content;
    public MarkdownTokenType MarkdownTokenType { get; } = markdownTokenType;
    public int StartIndex { get; } = startIndex;
    public int EndIndex { get; } = endIndex;
}