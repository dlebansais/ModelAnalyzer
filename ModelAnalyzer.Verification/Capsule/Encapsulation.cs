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

    public static IArrayExprCapsule Encapsulate(this ArrayExpr expr)
    {
        return new ArrayExprCapsule() { Item = expr };
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
