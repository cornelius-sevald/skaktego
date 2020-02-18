using System;
using System.Threading;

/// <summary>
/// Synchronised variable.
/// Inspired by Haskell's MVar: https://hackage.haskell.org/package/base-4.12.0.0/docs/Control-Concurrent-MVar.html
/// 
/// Taken from: https://www.pcreview.co.uk/threads/mvar-for-net.3816403/
/// </summary>
public class MVar<T>
{

    // Properties
    private AutoResetEvent reader = new AutoResetEvent(false);
    private AutoResetEvent writer = new AutoResetEvent(true);

    private T varD;
    public T var
    {
        get { return takeMVar(x => x); }
        set { putMVar(value, x => { }); }
    }

    // Constructors.
    public MVar() { }
    public MVar(T initial) : this() { var = initial; }

    // Set value or block.
    public R putMVar<R>(T x, Func<T, R> f)
    {
        R res;

        writer.WaitOne();
        varD = x;
        res = f(varD);
        reader.Set();
        return res;
    }

    // Set value or block.
    public void putMVar(T v, Action<T> f)
    {
        putMVar(v, x => { f(x); return true; });
    }

    // Get value or block.
    public R takeMVar<R>(Func<T, R> f)
    {
        R v;

        reader.WaitOne();
        v = f(varD);
        writer.Set();
        return v;
    }

    // Get value or block.
    public T takeMVar(Action<T> f)
    {
        return takeMVar(x => { f(x); return x; });
    }

}