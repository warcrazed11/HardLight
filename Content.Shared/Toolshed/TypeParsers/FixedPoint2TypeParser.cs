using Content.Shared.FixedPoint;
using Robust.Shared.Console;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Syntax;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Shared.Toolshed.TypeParsers;

/// <summary>
/// Toolshed type parser for FixedPoint2, allowing commands to accept quantities like 1, 0.5, or 2.75.
/// </summary>
public sealed class FixedPoint2TypeParser : TypeParser<FixedPoint2>
{
    public override bool TryParse(ParserContext ctx, out FixedPoint2 result)
    {
        // Parse as a standard double using existing numeric parsers, then convert to FixedPoint2.
        if (Toolshed.TryParse<double>(ctx, out var value))
        {
            result = FixedPoint2.New(value);
            return true;
        }

        result = default;
        return false;
    }

    public override CompletionResult? TryAutocomplete(ParserContext parserContext, CommandArgument? arg)
    {
        return CompletionResult.FromHint(GetArgHint(arg));
    }
}
