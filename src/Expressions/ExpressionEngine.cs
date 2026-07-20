using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Fluxion.src.Expressions;

/// <summary>
/// Entry point for compiling user-entered mathematical expressions.
/// </summary>
public static class ExpressionEngine
{
    public static CompiledExpression Compile(string expression)
    {
        string normalized = NormalizeExpression(expression);

        var lexer = new ExpressionLexer(normalized);
        IReadOnlyList<ExpressionToken> tokens = lexer.Tokenize();

        var parser = new ExpressionParser(tokens);
        ExpressionNode root = parser.Parse();

        return new CompiledExpression(normalized, root);
    }

    private static string NormalizeExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ExpressionParseException(
                "Enter a mathematical expression",
                0);
        }

        string normalized = expression
            .Trim()
            .Replace("−", "-")
            .Replace("–", "-")
            .Replace("×", "*")
            .Replace("·", "*")
            .Replace("÷", "/")
            .Replace("π", "pi")
            .Replace("²", "^2")
            .Replace("³", "^3");

        int equalsIndex = normalized.IndexOf('=');

        if (equalsIndex < 0)
        {
            return normalized;
        }

        if (normalized.IndexOf('=', equalsIndex + 1) >= 0)
        {
            throw new ExpressionParseException(
                "Step 1 supports only one assignment symbol",
                equalsIndex);
        }

        string leftSide = normalized[..equalsIndex]
            .Trim()
            .ToLowerInvariant();

        string rightSide = normalized[(equalsIndex + 1)..]
            .Trim();

        if (!IsSupportedAssignment(leftSide))
        {
            throw new ExpressionParseException(
                "Use an expression, y = expression, z = expression, " +
                "f(x) = expression, or f(x,y) = expression",
                0);
        }

        if (rightSide.Length == 0)
        {
            throw new ExpressionParseException(
                "An expression is required after '='",
                equalsIndex + 1);
        }

        return rightSide;
    }

    private static bool IsSupportedAssignment(string leftSide)
    {
        if (leftSide is "y" or "z")
        {
            return true;
        }

        int openParenthesis = leftSide.IndexOf('(');
        int closeParenthesis = leftSide.LastIndexOf(')');

        if (openParenthesis <= 0 ||
            closeParenthesis != leftSide.Length - 1)
        {
            return false;
        }

        string functionName =
            leftSide[..openParenthesis].Trim();

        string parameters =
            leftSide[(openParenthesis + 1)..closeParenthesis]
                .Replace(" ", string.Empty);

        if ((parameters != "x" &&
             parameters != "x,y") ||
            functionName.Length == 0)
        {
            return false;
        }

        foreach (char character in functionName)
        {
            if (!char.IsLetter(character) && character != '_')
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// A parsed expression that can be evaluated repeatedly.
/// </summary>
public sealed class CompiledExpression
{
    private readonly ExpressionNode _root;

    internal CompiledExpression(string source, ExpressionNode root)
    {
        Source = source;
        _root = root;
    }

    public string Source { get; }

    public bool ContainsX => _root.ContainsX;

    public bool ContainsY => _root.ContainsY;

    public bool ContainsVariable =>
        ContainsX || ContainsY;

    public double Evaluate(
        double x = 0.0,
        double y = 0.0)
    {
        return _root.Evaluate(x, y);
    }

    public Func<double, double> ToFunction()
    {
        if (ContainsY)
        {
            throw new InvalidOperationException(
                "This expression uses y and must be plotted as a 3D surface.");
        }

        return x => Evaluate(x, 0.0);
    }

    public Func<double, double, double> ToSurface()
    {
        return Evaluate;
    }
}

/// <summary>
/// A parsing error with the location of the invalid input.
/// </summary>
public sealed class ExpressionParseException : Exception
{
    public ExpressionParseException(string message, int position)
        : base($"{message} at position {position + 1}.")
    {
        Position = position;
    }

    public int Position { get; }
}

internal enum ExpressionTokenKind
{
    Number,
    Identifier,
    Plus,
    Minus,
    Multiply,
    Divide,
    Power,
    LeftParenthesis,
    RightParenthesis,
    Comma,
    End
}

internal readonly record struct ExpressionToken(
    ExpressionTokenKind Kind,
    string Text,
    double Number,
    int Position);

internal sealed class ExpressionLexer
{
    private readonly string _text;
    private readonly List<ExpressionToken> _tokens = new();
    private int _position;

    public ExpressionLexer(string text)
    {
        _text = text;
    }

    public IReadOnlyList<ExpressionToken> Tokenize()
    {
        while (!IsAtEnd)
        {
            char current = Current;

            if (char.IsWhiteSpace(current))
            {
                _position++;
                continue;
            }

            if (char.IsDigit(current) ||
                (current == '.' && char.IsDigit(Peek(1))))
            {
                ReadNumber();
                continue;
            }

            if (char.IsLetter(current) || current == '_')
            {
                ReadIdentifier();
                continue;
            }

            int tokenPosition = _position;
            _position++;

            ExpressionTokenKind kind = current switch
            {
                '+' => ExpressionTokenKind.Plus,
                '-' => ExpressionTokenKind.Minus,
                '*' => ExpressionTokenKind.Multiply,
                '/' => ExpressionTokenKind.Divide,
                '^' => ExpressionTokenKind.Power,
                '(' => ExpressionTokenKind.LeftParenthesis,
                ')' => ExpressionTokenKind.RightParenthesis,
                ',' => ExpressionTokenKind.Comma,
                _ => throw new ExpressionParseException(
                    $"Unexpected character '{current}'",
                    tokenPosition)
            };

            AddSimpleToken(kind, current.ToString(), tokenPosition);
        }

        _tokens.Add(new ExpressionToken(
            ExpressionTokenKind.End,
            string.Empty,
            0.0,
            _position));

        return _tokens;
    }

    private bool IsAtEnd => _position >= _text.Length;

    private char Current => IsAtEnd ? '\0' : _text[_position];

    private char Peek(int distance)
    {
        int index = _position + distance;
        return index >= _text.Length ? '\0' : _text[index];
    }

    private void ReadNumber()
    {
        int start = _position;

        while (char.IsDigit(Current))
        {
            _position++;
        }

        if (Current == '.')
        {
            _position++;

            while (char.IsDigit(Current))
            {
                _position++;
            }
        }

        if (Current is 'e' or 'E')
        {
            int exponentMarker = _position;
            int cursor = _position + 1;

            if (cursor < _text.Length &&
                _text[cursor] is '+' or '-')
            {
                cursor++;
            }

            int digitStart = cursor;

            while (cursor < _text.Length &&
                   char.IsDigit(_text[cursor]))
            {
                cursor++;
            }

            _position = cursor > digitStart
                ? cursor
                : exponentMarker;
        }

        string numberText = _text[start.._position];

        if (!double.TryParse(
                numberText,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double value))
        {
            throw new ExpressionParseException(
                $"'{numberText}' is not a valid number",
                start);
        }

        _tokens.Add(new ExpressionToken(
            ExpressionTokenKind.Number,
            numberText,
            value,
            start));
    }

    private void ReadIdentifier()
    {
        int start = _position;

        while (char.IsLetterOrDigit(Current) || Current == '_')
        {
            _position++;
        }

        string identifier = _text[start.._position]
            .ToLowerInvariant();

        _tokens.Add(new ExpressionToken(
            ExpressionTokenKind.Identifier,
            identifier,
            0.0,
            start));
    }

    private void AddSimpleToken(
        ExpressionTokenKind kind,
        string text,
        int position)
    {
        _tokens.Add(new ExpressionToken(
            kind,
            text,
            0.0,
            position));
    }
}

internal sealed class ExpressionParser
{
    private readonly IReadOnlyList<ExpressionToken> _tokens;
    private int _current;

    public ExpressionParser(IReadOnlyList<ExpressionToken> tokens)
    {
        _tokens = tokens;
    }

    public ExpressionNode Parse()
    {
        ExpressionNode expression = ParseAdditionAndSubtraction();

        if (Current.Kind != ExpressionTokenKind.End)
        {
            throw new ExpressionParseException(
                $"Unexpected token '{Current.Text}'",
                Current.Position);
        }

        return expression;
    }

    private ExpressionNode ParseAdditionAndSubtraction()
    {
        ExpressionNode left = ParseMultiplicationAndDivision();

        while (true)
        {
            if (Match(ExpressionTokenKind.Plus))
            {
                left = new BinaryExpressionNode(
                    BinaryOperator.Add,
                    left,
                    ParseMultiplicationAndDivision());
                continue;
            }

            if (Match(ExpressionTokenKind.Minus))
            {
                left = new BinaryExpressionNode(
                    BinaryOperator.Subtract,
                    left,
                    ParseMultiplicationAndDivision());
                continue;
            }

            return left;
        }
    }

    private ExpressionNode ParseMultiplicationAndDivision()
    {
        ExpressionNode left = ParseUnary();

        while (true)
        {
            if (Match(ExpressionTokenKind.Multiply))
            {
                left = new BinaryExpressionNode(
                    BinaryOperator.Multiply,
                    left,
                    ParseUnary());
                continue;
            }

            if (Match(ExpressionTokenKind.Divide))
            {
                left = new BinaryExpressionNode(
                    BinaryOperator.Divide,
                    left,
                    ParseUnary());
                continue;
            }

            if (CanStartImplicitFactor(Current.Kind))
            {
                left = new BinaryExpressionNode(
                    BinaryOperator.Multiply,
                    left,
                    ParseUnary());
                continue;
            }

            return left;
        }
    }

    private ExpressionNode ParseUnary()
    {
        if (Match(ExpressionTokenKind.Plus))
        {
            return new UnaryExpressionNode(
                UnaryOperator.Positive,
                ParseUnary());
        }

        if (Match(ExpressionTokenKind.Minus))
        {
            return new UnaryExpressionNode(
                UnaryOperator.Negative,
                ParseUnary());
        }

        return ParsePower();
    }

    private ExpressionNode ParsePower()
    {
        ExpressionNode left = ParsePrimary();

        if (Match(ExpressionTokenKind.Power))
        {
            return new BinaryExpressionNode(
                BinaryOperator.Power,
                left,
                ParseUnary());
        }

        return left;
    }

    private ExpressionNode ParsePrimary()
    {
        if (Match(ExpressionTokenKind.Number))
        {
            return new NumberExpressionNode(Previous.Number);
        }

        if (Match(ExpressionTokenKind.Identifier))
        {
            ExpressionToken identifier = Previous;

            if (Match(ExpressionTokenKind.LeftParenthesis))
            {
                return ParseFunctionCall(identifier);
            }

            return ParseIdentifier(identifier);
        }

        if (Match(ExpressionTokenKind.LeftParenthesis))
        {
            int openingPosition = Previous.Position;
            ExpressionNode inner = ParseAdditionAndSubtraction();

            if (!Match(ExpressionTokenKind.RightParenthesis))
            {
                throw new ExpressionParseException(
                    "Expected ')'",
                    openingPosition);
            }

            return inner;
        }

        if (Current.Kind == ExpressionTokenKind.End)
        {
            throw new ExpressionParseException(
                "The expression ended unexpectedly",
                Current.Position);
        }

        throw new ExpressionParseException(
            $"Expected a number, variable, function, or '(' instead of '{Current.Text}'",
            Current.Position);
    }

    private ExpressionNode ParseFunctionCall(ExpressionToken identifier)
    {
        var arguments = new List<ExpressionNode>();

        if (!Check(ExpressionTokenKind.RightParenthesis))
        {
            do
            {
                arguments.Add(ParseAdditionAndSubtraction());
            }
            while (Match(ExpressionTokenKind.Comma));
        }

        if (!Match(ExpressionTokenKind.RightParenthesis))
        {
            throw new ExpressionParseException(
                $"Expected ')' after function '{identifier.Text}'",
                identifier.Position);
        }

        FunctionCatalog.Validate(
            identifier.Text,
            arguments.Count,
            identifier.Position);

        return new FunctionExpressionNode(
            identifier.Text,
            arguments);
    }

    private static ExpressionNode ParseIdentifier(ExpressionToken identifier)
    {
        return identifier.Text switch
        {
            "x" =>
                new VariableExpressionNode(
                    ExpressionVariable.X),

            "y" =>
                new VariableExpressionNode(
                    ExpressionVariable.Y),

            "pi" => new NumberExpressionNode(Math.PI),
            "e" => new NumberExpressionNode(Math.E),
            "tau" => new NumberExpressionNode(Math.Tau),

            _ when FunctionCatalog.IsKnown(identifier.Text) =>
                throw new ExpressionParseException(
                    $"Function '{identifier.Text}' requires parentheses, for example {identifier.Text}(x)",
                    identifier.Position),

            _ => throw new ExpressionParseException(
                $"Unknown identifier '{identifier.Text}'. " +
                "Fluxion currently supports the variables x and y",
                identifier.Position)
        };
    }

    private static bool CanStartImplicitFactor(ExpressionTokenKind kind)
    {
        return kind is
            ExpressionTokenKind.Number or
            ExpressionTokenKind.Identifier or
            ExpressionTokenKind.LeftParenthesis;
    }

    private ExpressionToken Current => _tokens[_current];
    private ExpressionToken Previous => _tokens[_current - 1];

    private bool Check(ExpressionTokenKind kind)
    {
        return Current.Kind == kind;
    }

    private bool Match(ExpressionTokenKind kind)
    {
        if (!Check(kind))
        {
            return false;
        }

        _current++;
        return true;
    }
}

internal abstract class ExpressionNode
{
    public abstract bool ContainsX { get; }

    public abstract bool ContainsY { get; }

    public abstract double Evaluate(
        double x,
        double y);
}

internal sealed class NumberExpressionNode : ExpressionNode
{
    private readonly double _value;

    public NumberExpressionNode(double value)
    {
        _value = value;
    }

    public override bool ContainsX => false;

    public override bool ContainsY => false;

    public override double Evaluate(
        double x,
        double y)
    {
        return _value;
    }
}

internal enum ExpressionVariable
{
    X,
    Y
}

internal sealed class VariableExpressionNode : ExpressionNode
{
    private readonly ExpressionVariable _variable;

    public VariableExpressionNode(
        ExpressionVariable variable)
    {
        _variable = variable;
    }

    public override bool ContainsX =>
        _variable == ExpressionVariable.X;

    public override bool ContainsY =>
        _variable == ExpressionVariable.Y;

    public override double Evaluate(
        double x,
        double y)
    {
        return _variable switch
        {
            ExpressionVariable.X => x,
            ExpressionVariable.Y => y,

            _ => throw new InvalidOperationException(
                "Unknown expression variable.")
        };
    }
}

internal enum UnaryOperator
{
    Positive,
    Negative
}

internal sealed class UnaryExpressionNode : ExpressionNode
{
    private readonly UnaryOperator _operator;
    private readonly ExpressionNode _operand;

    public UnaryExpressionNode(
        UnaryOperator @operator,
        ExpressionNode operand)
    {
        _operator = @operator;
        _operand = operand;
    }

    public override bool ContainsX =>
        _operand.ContainsX;

    public override bool ContainsY =>
        _operand.ContainsY;

    public override double Evaluate(
        double x,
        double y)
    {
        double value =
            _operand.Evaluate(x, y);

        return _operator switch
        {
            UnaryOperator.Positive => value,
            UnaryOperator.Negative => -value,
            _ => throw new InvalidOperationException("Unknown unary operator.")
        };
    }
}

internal enum BinaryOperator
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Power
}

internal sealed class BinaryExpressionNode : ExpressionNode
{
    private readonly BinaryOperator _operator;
    private readonly ExpressionNode _left;
    private readonly ExpressionNode _right;

    public BinaryExpressionNode(
        BinaryOperator @operator,
        ExpressionNode left,
        ExpressionNode right)
    {
        _operator = @operator;
        _left = left;
        _right = right;
    }

    public override bool ContainsX =>
        _left.ContainsX ||
        _right.ContainsX;

    public override bool ContainsY =>
        _left.ContainsY ||
        _right.ContainsY;

    public override double Evaluate(
        double x,
        double y)
    {
        double leftValue =
            _left.Evaluate(x, y);

        double rightValue =
            _right.Evaluate(x, y);

        return _operator switch
        {
            BinaryOperator.Add => leftValue + rightValue,
            BinaryOperator.Subtract => leftValue - rightValue,
            BinaryOperator.Multiply => leftValue * rightValue,
            BinaryOperator.Divide => leftValue / rightValue,
            BinaryOperator.Power => Math.Pow(leftValue, rightValue),
            _ => throw new InvalidOperationException("Unknown binary operator.")
        };
    }
}

internal sealed class FunctionExpressionNode : ExpressionNode
{
    private readonly string _name;
    private readonly IReadOnlyList<ExpressionNode> _arguments;

    public FunctionExpressionNode(
        string name,
        IReadOnlyList<ExpressionNode> arguments)
    {
        _name = name;
        _arguments = arguments;
    }

    public override bool ContainsX =>
        _arguments.Any(argument =>
            argument.ContainsX);

    public override bool ContainsY =>
        _arguments.Any(argument =>
            argument.ContainsY);

    public override double Evaluate(
        double x,
        double y)
    {
        var values =
            new double[_arguments.Count];

        for (int index = 0;
             index < _arguments.Count;
             index++)
        {
            values[index] =
                _arguments[index].Evaluate(x, y);
        }

        return FunctionCatalog.Evaluate(
            _name,
            values);
    }
}

internal static class FunctionCatalog
{
    private static readonly HashSet<string> KnownFunctions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "sin",
            "cos",
            "tan",
            "asin",
            "acos",
            "atan",
            "sqrt",
            "abs",
            "exp",
            "ln",
            "log",
            "floor",
            "ceil",
            "round",
            "sign",
            "min",
            "max",
            "pow",
            "clamp"
        };

    public static bool IsKnown(string name)
    {
        return KnownFunctions.Contains(name);
    }

    public static void Validate(
        string name,
        int argumentCount,
        int position)
    {
        if (!IsKnown(name))
        {
            throw new ExpressionParseException(
                $"Unknown function '{name}'",
                position);
        }

        bool valid = name switch
        {
            "min" or "max" or "pow" => argumentCount == 2,
            "clamp" => argumentCount == 3,
            "log" => argumentCount is 1 or 2,
            _ => argumentCount == 1
        };

        if (valid)
        {
            return;
        }

        string expected = name switch
        {
            "min" or "max" or "pow" => "2 arguments",
            "clamp" => "3 arguments",
            "log" => "1 or 2 arguments",
            _ => "1 argument"
        };

        throw new ExpressionParseException(
            $"Function '{name}' expects {expected}, but received {argumentCount}",
            position);
    }

    public static double Evaluate(
        string name,
        IReadOnlyList<double> arguments)
    {
        return name switch
        {
            "sin" => Math.Sin(arguments[0]),
            "cos" => Math.Cos(arguments[0]),
            "tan" => Math.Tan(arguments[0]),
            "asin" => Math.Asin(arguments[0]),
            "acos" => Math.Acos(arguments[0]),
            "atan" => Math.Atan(arguments[0]),
            "sqrt" => Math.Sqrt(arguments[0]),
            "abs" => Math.Abs(arguments[0]),
            "exp" => Math.Exp(arguments[0]),
            "ln" => Math.Log(arguments[0]),
            "log" when arguments.Count == 1 => Math.Log10(arguments[0]),
            "log" => Math.Log(arguments[0], arguments[1]),
            "floor" => Math.Floor(arguments[0]),
            "ceil" => Math.Ceiling(arguments[0]),
            "round" => Math.Round(arguments[0]),
            "sign" => Math.Sign(arguments[0]),
            "min" => Math.Min(arguments[0], arguments[1]),
            "max" => Math.Max(arguments[0], arguments[1]),
            "pow" => Math.Pow(arguments[0], arguments[1]),
            "clamp" => Math.Clamp(arguments[0], arguments[1], arguments[2]),
            _ => throw new InvalidOperationException($"Unknown function '{name}'.")
        };
    }
}
