using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
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
            var tlx = new ThreadLocalEx<string>();

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
        static ConcurrentDictionary<Int32, T> _slot;
        static int _instanceId;
        static int _threadId;

        [ThreadStatic]
        static UInt16? _tid;

        static ThreadLocalEx()
        {
            _slot = new ConcurrentDictionary<Int32, T>(Environment.ProcessorCount * 2,
                    Environment.ProcessorCount * 20);
        }

        readonly Func<T> _valueFactory;
        Exception _cached;
        readonly UInt16 _id;

        public ThreadLocalEx() 
        {
        }

        public ThreadLocalEx(Func<T> valueFactory)
        {
            _valueFactory = valueFactory;
            unchecked
            {
                _id = (UInt16) Interlocked.Increment(ref _instanceId);
            }
        }

        public void Dispose()
        {
            // TODO: Remove all dictionary entries for this instance.
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
        
        public bool IsValueCreated
        {
            get
            {
                try
                {
                    return _slot.ContainsKey(_id);
                }
                catch(Exception)
                {
                    return false;
                }
            }
        }

        [System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
        public T Value
        {
            get
            {
                try
                {
                    return _slot[_tid.Value << 16 | _id];
                }
                catch(Exception e)
                {
                    if(!_tid.HasValue) unchecked {
                        _tid = (UInt16) Interlocked.Increment(ref _threadId);
                    }

                    if(_slot.ContainsKey(_tid.Value << 16 | _id))
                        _cached = e;

                    if(_cached != null)
                        throw _cached;

                    if(_valueFactory != null)
                    {
                        try
                        {
                            _slot[_tid.Value << 16 | _id] = _valueFactory();
                        }
                        catch(Exception x)
                        {
                            _slot[_tid.Value << 16 | _id] = default(T);
                            _cached = x;
                            throw;
                        }
                    }
                    else
                        _slot[_tid.Value << 16 | _id] = default(T);

                    return _slot[_tid.Value << 16 | _id]; 
                }
            }

            set
            {
                if(_cached != null)
                    throw _cached;

                if(! _tid.HasValue) unchecked {
                    _tid = (UInt16) Interlocked.Increment(ref _threadId);
                }

                _slot[_tid.Value << 16 | _id] = value;
            }
        }

        public override string ToString()
        {
            return string.Format("[ThreadLocalEx: IsValueCreated={0}, Value={1}]", IsValueCreated, Value);
        }
    }

    //[HostProtection (SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    //public class ThreadLocal<T> : IDisposable
    //{
    //    readonly Func<T> valueFactory;
    //    LocalDataStoreSlot localStore;
    //    Exception cachedException;
		
    //    class DataSlotWrapper
    //    {
    //        public bool Creating;
    //        public bool Init;
    //        public Func<T> Getter;
    //    }
		
    //    public ThreadLocal () : this (LazyInitializer.GetDefaultValueFactory<T>)
    //    {
    //    }

    //    public ThreadLocal (Func<T> valueFactory)
    //    {
    //        if (valueFactory == null)
    //            throw new ArgumentNullException ("valueFactory");
			
    //        localStore = Thread.AllocateDataSlot ();
    //        this.valueFactory = valueFactory;
    //    }
		
    //    public void Dispose ()
    //    {
    //        Dispose (true);
    //    }
		
    //    protected virtual void Dispose (bool disposing)
    //    {
			
    //    }
		
    //    public bool IsValueCreated {
    //        get {
    //            ThrowIfNeeded ();
    //            return IsInitializedThreadLocal ();
    //        }
    //    }

    //    [System.Diagnostics.DebuggerBrowsableAttribute (System.Diagnostics.DebuggerBrowsableState.Never)]
    //    public T Value {
    //        get {
    //            ThrowIfNeeded ();
    //            return GetValueThreadLocal ();
    //        }
    //        set {
    //            ThrowIfNeeded ();

    //            DataSlotWrapper w = GetWrapper ();
    //            w.Init = true;
    //            w.Getter = () => value;
    //        }
    //    }
		
    //    public override string ToString ()
    //    {
    //        return string.Format ("[ThreadLocal: IsValueCreated={0}, Value={1}]", IsValueCreated, Value);
    //    }
		
    //    T GetValueThreadLocal ()
    //    {
    //        DataSlotWrapper myWrapper = GetWrapper ();
    //        if (myWrapper.Creating)
    //            throw new InvalidOperationException ("The initialization function attempted to reference Value recursively");

    //        return myWrapper.Getter ();
    //    }
		
    //    bool IsInitializedThreadLocal ()
    //    {
    //        DataSlotWrapper myWrapper = GetWrapper ();

    //        return myWrapper.Init;
    //    }

    //    DataSlotWrapper GetWrapper ()
    //    {
    //        DataSlotWrapper myWrapper = (DataSlotWrapper)Thread.GetData (localStore);
    //        if (myWrapper == null) {
    //            myWrapper = DataSlotCreator ();
    //            Thread.SetData (localStore, myWrapper);
    //        }

    //        return myWrapper;
    //    }

    //    void ThrowIfNeeded ()
    //    {
    //        if (cachedException != null)
    //            throw cachedException;
    //    }

    //    DataSlotWrapper DataSlotCreator ()
    //    {
    //        DataSlotWrapper wrapper = new DataSlotWrapper ();
    //        Func<T> valSelector = valueFactory;
	
    //        wrapper.Getter = delegate {
    //            wrapper.Creating = true;
    //            try {
    //                T val = valSelector ();
    //                wrapper.Creating = false;
    //                wrapper.Init = true;
    //                wrapper.Getter = () => val;
    //                return val;
    //            } catch (Exception e) {
    //                cachedException = e;
    //                throw e;
    //            }
    //        };
			
    //        return wrapper;
    //    }
    //}
}
