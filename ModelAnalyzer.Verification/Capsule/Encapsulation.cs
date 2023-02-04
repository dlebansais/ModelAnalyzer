namespace ModelAnalyzer;

using System.Diagnostics;
using Microsoft.Z3;

internal static class Encapsulation
{
    public static IBoolExprCapsule Encapsulate(this BoolExpr expr)
    {
        return new BoolExprCapsule() { Item = expr };
    }

    public static IIntExprCapsule Encapsulate(this IntExpr expr)
    {
        return new IntExprCapsule() { Item = expr };
    }

    public static IArithExprCapsule Encapsulate(this ArithExpr expr)
    {
        switch (expr)
        {
            case IntExpr Int:
                return new IntExprCapsule() { Item = Int };
            default:
                return new ArithExprCapsule() { Item = expr };
        }
    }

    public static IRefExprCapsule EncapsulateAsRef(this IntExpr expr, ReferenceIndex index)
    {
        return new RefExprCapsule() { Item = expr, Index = index };
    }

    public static IObjectRefExprCapsule EncapsulateAsObjectRef(this IntExpr expr, ClassName className, ReferenceIndex index)
    {
        return new ObjectRefExprCapsule() { Item = expr, ClassName = className, Index = index };
    }

    public static IArrayRefExprCapsule EncapsulateAsArrayRef(this IntExpr expr, ExpressionType elementType, ReferenceIndex index)
    {
        Debug.Assert(!elementType.IsArray);

        return new ArrayRefExprCapsule() { Item = expr, ElementType = elementType, Index = index };
    }

    public static IExprSet<IBoolExprCapsule> ToSingleSet(this IBoolExprCapsule expr)
    {
        return new ExprSet<IBoolExprCapsule>(expr);
    }

    public static IExprSet<IIntExprCapsule> ToSingleSet(this IIntExprCapsule expr)
    {
        return new ExprSet<IIntExprCapsule>(expr);
    }

    public static IExprSet<IArithExprCapsule> ToSingleSet(this IArithExprCapsule expr)
    {
        switch (expr)
        {
            case IIntExprCapsule Int:
                return new ExprSet<IIntExprCapsule>(Int);
            default:
                return new ExprSet<IArithExprCapsule>(expr);
        }
    }

    public static IExprSet<IRefExprCapsule> ToSingleSet(this IRefExprCapsule expr)
    {
        return new ExprSet<IRefExprCapsule>(expr);
    }
}
