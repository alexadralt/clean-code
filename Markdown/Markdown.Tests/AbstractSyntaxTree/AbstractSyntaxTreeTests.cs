using System;
using System.Collections.Generic;
using FluentAssertions;
using Markdown.AbstractSyntaxTree;
using NUnit.Framework;
using MdAbstractSyntaxTree = Markdown.AbstractSyntaxTree.MdAbstractSyntaxTree;

namespace Markdown.Tests.AbstractSyntaxTree;

[TestFixture]
[TestOf(typeof(MdAbstractSyntaxTree))]
public class AbstractSyntaxTreeTests
{
    private IAbstractSyntaxTree<TokenType> _syntaxTree;
    
    [SetUp]
    public void SetUp()
    {
        var tags = new Dictionary<TokenType, string>();
        tags.Add(TokenType.Italic, "em");
        tags.Add(TokenType.Bold, "strong");
        tags.Add(TokenType.Heading, "h1");
        _syntaxTree = new MdAbstractSyntaxTree(tags.AsReadOnly());
    }

    [Test]
    [Description("Проверяем, что метод AddToken кидает исключение если тип токена == PlainText" +
                 " и содержимое токена, переданное в аргумент имеет значение null")]
    [TestCase(TokenType.PlainText, "", false)]
    [TestCase(TokenType.PlainText, "some text", false)]
    [TestCase(TokenType.PlainText, null, true)]
    [TestCase(TokenType.Italic, "some text", false)]
    [TestCase(TokenType.Italic, null, false)]
    public void AddToken_ThrowsWhen_TokenValueIsNull_And_TokenTypeIsNotPlainText(
        TokenType tokenType,
        string value,
        bool expectedException = true)
    {
        Action act = () => _syntaxTree.AddToken(tokenType, value?.AsMemory());
        
        if (expectedException)
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("tokenValue must not be null");
        else
            act.Should().NotThrow();
    }

    [Test]
    [Description("Проверяем, что набор токенов правильно переводится в текст")]
    [TestCaseSource(nameof(AddTokenTestSource))]
    public void TreeConvertsTokensToTextCorrectly(TestToken[] tokens, string expectedResult)
    {
        AddTokens(tokens);
        
        _syntaxTree.ToText()
            .Should()
            .Be(expectedResult);
    }

    public readonly struct TestToken(TokenType tokenType, string value = null, bool endToken = false)
    {
        public readonly bool EndToken = endToken;
        public readonly TokenType TokenType = tokenType;
        public readonly string Value = value;
    }

    private void AddTokens(TestToken[] tokens)
    {
        foreach (var token in tokens)
        {
            if (token.TokenType == TokenType.PlainText)
                _syntaxTree.AddToken(token.TokenType, token.Value.AsMemory());
            else if (token.EndToken && token.TokenType == TokenType.Heading)
                _syntaxTree.TryEndToken(TokenType.Heading);
            else if (token.EndToken)
                _syntaxTree.TryEndCurrentToken();
            else
                _syntaxTree.AddToken(token.TokenType);
        }
    }

    public static object[] AddTokenTestSource =
    {
        new object[]
        {
            new TestToken[]
            {
                new TestToken(TokenType.PlainText, "Next "),
                new TestToken(TokenType.Italic),
                new TestToken(TokenType.PlainText, "word"),
                new TestToken(TokenType.Italic, null, true),
                new TestToken(TokenType.PlainText, " is italic")
            },
            "Next <em>word</em> is italic"
        },
        new object[]
        {
            new TestToken[]
            {
                new TestToken(TokenType.Heading),
                new TestToken(TokenType.PlainText, "This text is a heading. And "),
                new TestToken(TokenType.Italic),
                new TestToken(TokenType.PlainText, "this"),
                new TestToken(TokenType.Italic, null, true),
                new TestToken(TokenType.PlainText, " word is italic. And "),
                new TestToken(TokenType.Bold),
                new TestToken(TokenType.PlainText, "these words"),
                new TestToken(TokenType.Bold, null, true),
                new TestToken(TokenType.PlainText, " are in bold.")
            },
            "<h1>This text is a heading. And <em>this</em> word is italic. And <strong>these words</strong> are in bold.</h1>"
        },
        new object[]
        {
            new TestToken[]
            {
                new TestToken(TokenType.Heading),
                new TestToken(TokenType.PlainText, "This text is a heading. And "),
                new TestToken(TokenType.Bold),
                new TestToken(TokenType.PlainText, "these words are in bold."),
                new TestToken(TokenType.Heading, null, true),
                new TestToken(TokenType.PlainText, "\nThis text is after a heading.")
            },
            "<h1>This text is a heading. And <strong>these words are in bold.</strong></h1>\nThis text is after a heading."
        }
    };
}