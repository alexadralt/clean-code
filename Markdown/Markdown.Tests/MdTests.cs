using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FluentAssertions;
using Markdown.Parser;
using Markdown.ParseTree;
using Markdown.Token;
using Markdown.Tokenizer;
using NUnit.Framework;

namespace Markdown.Tests;

[TestFixture]
[TestOf(typeof(Md))]
public class MdTests
{
    private IRenderer _md;

    [SetUp]
    public void SetUp()
    {
        var tokenAliases = new Dictionary<string, MdTokenType>();
        tokenAliases.Add("_", MdTokenType.Italic);
        tokenAliases.Add("__", MdTokenType.Bold);
        tokenAliases.Add("# ", MdTokenType.Heading);
        tokenAliases.Add("\n", MdTokenType.Line);

        var tokenTags = new Dictionary<MdTokenType, string>();
        tokenTags.Add(MdTokenType.Italic, "em");
        tokenTags.Add(MdTokenType.Bold, "strong");
        tokenTags.Add(MdTokenType.Heading, "h1");

        _md = new Md(tokenTags, new MdTokenizer(tokenAliases, '\\'), new MdParser(new MdParseTree()));
    }
    
    [Test]
    [TestCase("", "")]
    [TestCase("Hello world", "Hello world")]
    [TestCase("Hello _world_!", "Hello <em>world</em>!")]
    [TestCase("# _Hello_ __world__!", "<h1><em>Hello</em> <strong>world</strong>!</h1>")]
    public void Render_ReturnsCorrectMarkdown_ForSimpleCases(
        string input,
        string expectedOutput)
    {
        _md.Render(input)
            .Should()
            .Be(expectedOutput);
    }

    [Test]
    [TestCase("This __text _contains_ nested__ markdown", "This <strong>text <em>contains</em> nested</strong> markdown")]
    [TestCase("This is _an example __of inversed__ nested_ markdown", "This is <em>an example __of inversed__ nested</em> markdown")]
    public void Render_ReturnsCorrectMarkdown_ForCasesWithNesting(
        string input,
        string expectedOutput)
    {
        _md.Render(input)
            .Should()
            .Be(expectedOutput);
    }
    
    [Test]
    [TestCase("Text_12_3", "Text_12_3")]
    [TestCase("This _Text_12_3_ should be italic", "This <em>Text_12_3</em> should be italic")]
    [TestCase("5__12_3__4", "5__12_3__4")]
    [TestCase("Text __that_12__3__ is in bold", "Text <strong>that_12__3</strong> is in bold")]
    public void Render_ReturnsCorrectMarkdown_ForTextWithNumbers(
        string input,
        string expectedOutput)
    {
        _md.Render(input)
            .Should()
            .Be(expectedOutput);
    }
    
    [Test]
    [TestCase("_begin_ning", "<em>begin</em>ning")]
    [TestCase("mi_ddl_e", "mi<em>ddl</em>e")]
    [TestCase("end_ing_", "end<em>ing</em>")]
    [TestCase("__begin__ning", "<strong>begin</strong>ning")]
    [TestCase("mi__ddl__e", "mi<strong>ddl</strong>e")]
    [TestCase("end__ing__", "end<strong>ing</strong>")]
    public void Render_ReturnsCorrectMarkdown_ForPartsOfWords(
        string input,
        string expectedOutput)
    {
        _md.Render(input)
            .Should()
            .Be(expectedOutput);
    }
    
    [Test]
    [TestCase("This sh_ould not cha_nge", "This sh_ould not cha_nge")]
    [TestCase("As w__ell a__s this", "As w__ell a__s this")]
    [TestCase("This sh__o_uld_ wo__rk like this", "This sh__o<em>uld</em> wo__rk like this")]
    public void Render_ReturnsCorrectMarkdown_ForMarkdownInDifferentWords(
        string input,
        string expectedOutput)
    {
        _md.Render(input)
            .Should()
            .Be(expectedOutput);
    }
    
    [Test]
    [TestCase("__Unpaired_ markdown", "__Unpaired_ markdown")]
    [TestCase("Another _unpaired markdown__", "Another _unpaired markdown__")]
    public void Render_ReturnsCorrectMarkdown_ForUnpairedMarkdownSymbols(
        string input,
        string expectedOutput)
    {
        _md.Render(input)
            .Should()
            .Be(expectedOutput);
    }
    
    [Test]
    [TestCase("This_ should not_ change", "This_ should not_ change")]
    [TestCase("This _should _be in_ italics", "This <em>should _be in</em> italics")]
    public void Render_ReturnsCorrectMarkdown_ForIncorrectlyPlacedUnderscores(
        string input,
        string expectedOutput)
    {
        _md.Render(input)
            .Should()
            .Be(expectedOutput);
    }
    
    [Test]
    [TestCase("Intersecting _markdown __should_ work__ like this", "Intersecting _markdown __should_ work__ like this")]
    [TestCase("Another __example of _intersecting__ markdown_", "Another __example of _intersecting__ markdown_")]
    public void Render_ReturnsCorrectMarkdown_ForIntersectingMarkdown(
        string input,
        string expectedOutput)
    {
        _md.Render(input)
            .Should()
            .Be(expectedOutput);
    }
    
    [Test]
    [TestCase("This should ____ remain the same", "This should ____ remain the same")]
    [TestCase("This also should __ not change", "This also should __ not change")]
    public void Render_ReturnsCorrectMarkdown_ForEmptyMarkdown(
        string input,
        string expectedOutput)
    {
        _md.Render(input)
            .Should()
            .Be(expectedOutput);
    }
    
    [Test]
    [TestCase(@"This should \_not turn\_ into tags", "This should _not turn_ into tags")]
    [TestCase(@"This should \remain the\ same", @"This should \remain the\ same")]
    public void Render_ReturnsCorrectMarkdown_ForEscapeCharacters(
        string input,
        string expectedOutput)
    {
        _md.Render(input)
            .Should()
            .Be(expectedOutput);
    }

    [Test]
    public void Render_PerformanceTest()
    {
        var fullStr = ArrangePerformanceTest("_Hello_ world_12. Hel_lo world_", 50000);
        Console.WriteLine($"Total length: {fullStr.Length}");

        var totalTime = MeasureTime(fullStr);
        Console.WriteLine($"Time elapsed in ms: {totalTime}");
        
        totalTime
            .Should()
            .BeLessThan(1000);
    }

    private long MeasureTime(string fullStr)
    {
        var sw = new Stopwatch();
        sw.Start();
        _md.Render(fullStr);
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    private string ArrangePerformanceTest(string input, int copyCount)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < copyCount; i++)
            sb.Append(input);
        return sb.ToString();
    }
}