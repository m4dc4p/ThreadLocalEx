using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        ConcurrentDictionary<int, T> _dict;
        Func<T> _valueFactory;
        Exception _cached;
        bool _creating;

        public ThreadLocalEx() 
        {
            _dict = new ConcurrentDictionary<int, T>();
        }

        public ThreadLocalEx(Func<T> valueFactory)
        {
            _dict = new ConcurrentDictionary<int, T>();
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
                return _dict.ContainsKey(Thread.CurrentThread.ManagedThreadId);
            }
        }

        [System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
        public T Value
        {
            get
            {
                try
                {
                    return _dict[Thread.CurrentThread.ManagedThreadId];
                }
                catch(KeyNotFoundException)
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
                            _dict[Thread.CurrentThread.ManagedThreadId] = _valueFactory();
                        }
                        catch(Exception e)
                        {
                            _cached = e;
                            throw;
                        }
                    }
                    else
                        _dict[Thread.CurrentThread.ManagedThreadId] = default(T);

                    _creating = false;
                    return _dict[Thread.CurrentThread.ManagedThreadId];
                }
            }

            set
            {
                _dict[Thread.CurrentThread.ManagedThreadId] = value;
            }
        }

        public override string ToString()
        {
            return string.Format("[ThreadLocalEx: IsValueCreated={0}, Value={1}]", IsValueCreated, Value);
        }
    }
}
