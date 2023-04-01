namespace ModelAnalyzer;

using System.Diagnostics;

internal static class Encapsulation
{
    public static IBoolExprCapsule Encapsulate(this CodeProverBinding.IBooleanExpression expr)
    {
        return new BoolExprCapsule() { Item = expr };
    }

    public static IIntExprCapsule Encapsulate(this CodeProverBinding.IIntegerExpression expr)
    {
        return new IntExprCapsule() { Item = expr };
    }

    public static IArithExprCapsule Encapsulate(this CodeProverBinding.IArithmeticExpression expr)
    {
        switch (expr)
        {
            case CodeProverBinding.IIntegerExpression Int:
                return new IntExprCapsule() { Item = Int };
            default:
                return new ArithExprCapsule() { Item = expr };
        }
    }

    public static IRefExprCapsule EncapsulateAsRef(this CodeProverBinding.IReferenceExpression expr, CodeProverBinding.Reference index)
    {
        return new RefExprCapsule() { Item = expr, Index = index };
    }

    public static IObjectRefExprCapsule EncapsulateAsObjectRef(this CodeProverBinding.IReferenceExpression expr, ClassName className, CodeProverBinding.Reference index)
    {
        return new ObjectRefExprCapsule() { Item = expr, ClassName = className, Index = index };
    }

    public static IArrayRefExprCapsule EncapsulateAsArrayRef(this CodeProverBinding.IReferenceExpression expr, ExpressionType elementType, CodeProverBinding.Reference index)
    {
        Debug.Assert(!elementType.IsArray);

        return new ArrayRefExprCapsule() { Item = expr, ElementType = elementType, Index = index };
    }

    public static IArrayExprCapsule Encapsulate(this CodeProverBinding.IXxxArrayExpression expr, ExpressionType elementType)
    {
        return new ArrayExprCapsule() { Item = expr, ElementType = elementType };
    }

    public static IExprSingle<IBoolExprCapsule> ToSingleSet(this IBoolExprCapsule expr)
    {
        return new ExprSingle<IBoolExprCapsule>(expr);
    }

    public static IExprSingle<IIntExprCapsule> ToSingleSet(this IIntExprCapsule expr)
    {
        return new ExprSingle<IIntExprCapsule>(expr);
    }

    public static IExprSingle<IArithExprCapsule> ToSingleSet(this IArithExprCapsule expr)
    {
        switch (expr)
        {
            case IIntExprCapsule Int:
                return new ExprSingle<IIntExprCapsule>(Int);
            default:
                return new ExprSingle<IArithExprCapsule>(expr);
        }
    }

    public static IExprSingle<IRefExprCapsule> ToSingleSet(this IRefExprCapsule expr)
    {
        return new ExprSingle<IRefExprCapsule>(expr);
    }
}
