namespace Celmah.Assertions;

internal interface IContextExpression
{
    object? Evaluate(object context);
}