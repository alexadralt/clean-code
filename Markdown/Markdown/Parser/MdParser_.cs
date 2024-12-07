using Markdown.AbstractSyntaxTree;
using Markdown.Token;

namespace Markdown.Parser;

public class MdParser_
{
    public MdAbstractSyntaxTree_ Parse(IEnumerable<MdToken> tokens, MdAbstractSyntaxTree_ syntaxTree)
    {
        foreach (var token in tokens)
        {
            if (token.Behaviour == MdTokenBehaviour.Closing && syntaxTree.HasTokenInContext(token.Type))
            {
                if (syntaxTree.GetChildrenForCurrentToken().Count == 0)
                {
                    syntaxTree.EndToken(token.Type);
                    syntaxTree.AddToken(MdTokenType.PlainText, token.Text);
                }
                else
                    syntaxTree.EndToken(token.Type);
            }
            else if (token.Behaviour == MdTokenBehaviour.Opening && !syntaxTree.HasTokenInContext(token.Type))
            {
                syntaxTree.AddToken(token.Type, token.Text);
            }
            else if (token.Behaviour == MdTokenBehaviour.Closing && !syntaxTree.HasTokenInContext(token.Type))
            {
                // TODO intersecting markdown
                syntaxTree.EndToken();
                syntaxTree.AddToken(MdTokenType.PlainText, token.Text);
            }
            else if (token.Behaviour == MdTokenBehaviour.Opening && syntaxTree.HasTokenInContext(token.Type))
            {
                syntaxTree.AddToken(MdTokenType.PlainText, token.Text);
            }
            else if (token.Type == MdTokenType.PlainText || token.Behaviour == MdTokenBehaviour.Undefined)
            {
                syntaxTree.AddToken(MdTokenType.PlainText, token.Text);
            }
            else
            {
                throw new ArgumentException(
                    $"Unexpected token of type {token.Type}, with behaviour {token.Behaviour}, " +
                    $"with text: {token.Text}");
            }
        }
        
        return syntaxTree;
    }
}