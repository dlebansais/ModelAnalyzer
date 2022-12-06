namespace System.Runtime.CompilerServices;

using System.ComponentModel;

#if !NET5_0_OR_GREATER

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit { }

#endif // !NET5_0_OR_GREATER
