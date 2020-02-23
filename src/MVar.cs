using System;
using System.Threading;

namespace skaktego {

    /// <summary>
    /// /// Synchronised variable.
    /// Inspired by Haskell's MVar: https://hackage.haskell.org/package/base-4.12.0.0/docs/Control-Concurrent-MVar.html
    /// 
    /// Taken from: https://www.pcreview.co.uk/threads/mvar-for-net.3816403/
    /// </summary>
    public class MVar<T> {

        // Properties
        private AutoResetEvent reader = new AutoResetEvent(false);
        private AutoResetEvent writer = new AutoResetEvent(true);

        private T varD;
        public T Var {
            get { return TakeMVar(x => x); }
            set { PutMVar(value, x => { }); }
        }

        public bool HasValue { get; private set; }

        // Constructors.
        public MVar() { HasValue = false; }
        public MVar(T initial) : this() { Var = initial; }

        // Set value or block.
        public R PutMVar<R>(T x, Func<T, R> f) {
            R res;

            writer.WaitOne();
            varD = x;
            res = f(varD);
            HasValue = true;
            reader.Set();
            return res;
        }

        // Set value or block.
        public void PutMVar(T v, Action<T> f) {
            PutMVar(v, x => { f(x); return true; });
        }

        // Get value or block.
        public R TakeMVar<R>(Func<T, R> f) {
            R v;

            reader.WaitOne();
            v = f(varD);
            HasValue = false;
            writer.Set();
            return v;
        }

        // Get value or block.
        public T TakeMVar(Action<T> f) {
            return TakeMVar(x => { f(x); return x; });
        }

    }

}
