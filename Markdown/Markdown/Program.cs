using Markdown;
using Markdown.Parser;
using Markdown.ParseTree;
using Markdown.SyntaxRules;
using Markdown.Token;
using Markdown.Tokenizer;

var tokenAliases = new Dictionary<string, MdTokenType>();
tokenAliases.Add("_", MdTokenType.Italic);
tokenAliases.Add("__", MdTokenType.Bold);
tokenAliases.Add("# ", MdTokenType.Heading);
tokenAliases.Add("\n", MdTokenType.Line);

var tokenTags = new Dictionary<MdTokenType, string>();
tokenTags.Add(MdTokenType.Italic, "em");
tokenTags.Add(MdTokenType.Bold, "strong");
tokenTags.Add(MdTokenType.Heading, "h1");

var syntaxRules = new List<ISyntaxRule<MdTokenType>>();
syntaxRules.Add(new NestingRule());
syntaxRules.Add(new NumberRule());
syntaxRules.Add(new TokensInDifferentWordsRule());

var md = new Md(
    tokenTags, new MdTokenizer(tokenAliases, '\\'), new MdParser(new MdParseTree()), syntaxRules.ToArray());
Console.WriteLine(md.Render("# Hello World! _some words_ in italics\n__some other text__"));
