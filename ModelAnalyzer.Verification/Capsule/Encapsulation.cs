namespace ModelAnalyzer;

using Microsoft.Z3;

internal static class Encapsulation
{
    public static IExprCapsule Encapsulate(this Expr expr)
    {
        switch (expr)
        {
            case BoolExpr Bool:
                return new BoolExprCapsule() { Item = Bool };
            case IntExpr Int:
                return new IntExprCapsule() { Item = Int };
            case ArithExpr Arith:
                return new ArithExprCapsule() { Item = Arith };
            default:
                return new ExprCapsule() { Item = expr };
        }
    }

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

    public static IExprSet<IExprCapsule> ToSingleSet(this IExprCapsule expr)
    {
        switch (expr)
        {
            case IBoolExprCapsule Bool:
                return new ExprSet<IBoolExprCapsule>(Bool);
            case IIntExprCapsule Int:
                return new ExprSet<IIntExprCapsule>(Int);
            case IArithExprCapsule Arith:
                return new ExprSet<IArithExprCapsule>(Arith);
            default:
                return new ExprSet<IExprCapsule>(expr);
        }
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
}
