using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadLocalEx
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0, j = 0;
            string s = args.Length > 0 ? "1" : "0";
            var tl = new ThreadLocal<string>();
            var tlx = new ThreadLocal<string>();

            int x = 0, y = 0;

            Stopwatch tlW = new Stopwatch(), tlxW = new Stopwatch();

            int COUNT = 1000000, INTERVAL = 100000;
            Parallel.Invoke(() => Parallel.For(0, COUNT, delegate(int k) {
                tlW.Start();
                if(tl.IsValueCreated)
                    x++;
                else
                    y++;
                if(tl.Value == s)
                    x++;
                else
                    y++;
                tlW.Stop();
            }), () => Parallel.For(0, COUNT, delegate(int k) {
                tlxW.Start();
                if(tlx.IsValueCreated)
                    x++;
                else
                    y++;
                if(tlx.Value == s)
                    x++;
                else
                    y++;
                tlxW.Stop();
            }));

            tlW.Stop();
            tlxW.Stop();

            long tlms = tlW.ElapsedMilliseconds;
            long tlxms = tlxW.ElapsedMilliseconds;
            Console.WriteLine("tl: {0} ms, tlx: {1} ms, {2}, {3}", tlms,
                tlxms, x, y);
            
        }
    }

    public class ThreadLocalEx<T> : IDisposable
    {
        [ThreadStatic]
        static Box _slot;
        Func<T> _valueFactory;
        Exception _cached;
        bool _creating;

        class Box
        {
            public T v;
            public Box(T x)
            {
                v = x;
            }
        }

        public ThreadLocalEx() 
        {
        }

        public ThreadLocalEx(Func<T> valueFactory)
        {
            _valueFactory = valueFactory;
        }

        public void Dispose()
        {
            Dispose(true);
        }


        protected virtual void Dispose(bool disposing)
        {

        }
        
        public bool IsValueCreated
        {
            get
            {
                return _slot != null;
            }
        }

        [System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
        public T Value
        {
            get
            {
                try
                {
                    return _slot.v;
                }
                catch(NullReferenceException)
                {
                    if(_creating)
                        _cached = new InvalidOperationException();

                    if(_cached != null)
                        throw _cached;

                    _creating = true;
                    if(_valueFactory != null)
                    {
                        try
                        {
                            _slot = new Box(_valueFactory());
                        }
                        catch(Exception e)
                        {
                            _cached = e;
                            throw;
                        }
                    }
                    else
                        _slot = new Box(default(T));

                    _creating = false;
                    return _slot.v;
                }
            }

            set
            {
                _slot = new Box(value);
            }
        }

        public override string ToString()
        {
            return string.Format("[ThreadLocalEx: IsValueCreated={0}, Value={1}]", IsValueCreated, Value);
        }
    }
}
