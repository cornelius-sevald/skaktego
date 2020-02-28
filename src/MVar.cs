using System;
using System.Threading;

namespace skaktego {

    /// <summary>
    /// Synchronised variable.
    /// <para>Inspired by Haskell's MVar:
    /// https://hackage.haskell.org/package/base-4.12.0.0/docs/Control-Concurrent-MVar.html</para>
    /// 
    /// <para>Taken from:
    /// https://www.pcreview.co.uk/threads/mvar-for-net.3816403/</para>
    /// </summary>
    public class MVar<T> {

        // Properties
        private AutoResetEvent reader = new AutoResetEvent(false);
        private AutoResetEvent writer = new AutoResetEvent(true);

        // The variable
        private T varD;

        /// <summary>
        /// Take or put the variable
        /// </summary>
        public T Var {
            get { return TakeMVar(x => x); }
            set { PutMVar(value, x => { }); }
        }

        /// <summary>
        /// True if the MVar contains a variable
        /// </summary>
        public bool HasValue { get; private set; }

        /// <summary>
        /// Construct a new empty MVar
        /// </summary>
        public MVar() { HasValue = false; }

        /// <summary>
        /// Construct a new MVar with an initial value
        /// </summary>
        public MVar(T initial) : this() { Var = initial; }

        /// <summary>
        /// Put a variable into the MVar,
        /// and then return <c>f</c> applied to the variable.
        /// </summary>
        /// <param name="x">The variable to put</param>
        /// <param name="f">A function over <c>x</c></param>
        public R PutMVar<R>(T x, Func<T, R> f) {
            R res;

            writer.WaitOne();
            varD = x;
            res = f(varD);
            HasValue = true;
            reader.Set();
            return res;
        }

        /// <summary>
        /// Put a variable into the MVar,
        /// and the execute an action using the variable.
        /// </summary>
        /// <param name="v">The variable</param>
        /// <param name="f">An action to execute when setting the variable</param>
        public void PutMVar(T v, Action<T> f) {
            PutMVar(v, x => { f(x); return true; });
        }

        /// <summary>
        /// Take the variable out of the MVar,
        /// and then apply a function over it and return
        /// its result.
        /// </summary>
        /// <param name="f">A function over the variable</param>
        public R TakeMVar<R>(Func<T, R> f) {
            R v;

            reader.WaitOne();
            v = f(varD);
            HasValue = false;
            writer.Set();
            return v;
        }

        /// <summary>
        /// Take the variable out of the MVar,
        /// and then execute an action using the variable
        /// and return the variable.
        /// </summary>
        /// <param name="f">The action to execute when getting the variable</param>
        /// <returns></returns>
        public T TakeMVar(Action<T> f) {
            return TakeMVar(x => { f(x); return x; });
        }

    }

}
