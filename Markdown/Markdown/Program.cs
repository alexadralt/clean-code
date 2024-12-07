using Markdown;

var md = new Md();
Console.WriteLine(md.Render("# Hello World! _some words_ in italics\n__some other text__"));