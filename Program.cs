﻿using System;
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
        LocalDataStoreSlot _slot;
        
        readonly Func<T> _valueFactory;
        Exception _cached;

        class Box
        {
            public T v;

            public Box(T value)
            {
                v = value;
            }
        }

        public ThreadLocalEx() 
        {
            _slot = Thread.AllocateDataSlot();
        }

        public ThreadLocalEx(Func<T> valueFactory)
        {
            _slot = Thread.AllocateDataSlot();
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
                return Thread.GetData(_slot) != null;
            }
        }

        [System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
        public T Value
        {
            get
            {
                try
                {
                    return ((Box) Thread.GetData(_slot)).v;
                }
                catch(Exception)
                {
                    if(_cached != null) 
                        throw _cached;

                    if(_valueFactory != null)
                    {
                        try
                        {
                            Thread.SetData(_slot, new Box(_valueFactory()));
                        }
                        catch(Exception x)
                        {
                            _cached = x;
                            throw;
                        }
                    }
                    else
                        Thread.SetData(_slot, new Box(default(T)));

                    return ((Box) Thread.GetData(_slot)).v;
                }
            }

            set
            {
                if(_cached != null)
                    throw _cached;

                Thread.SetData(_slot, new Box(value));
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
