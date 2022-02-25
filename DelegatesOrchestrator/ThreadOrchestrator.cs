using DelegatesOrchestrator.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multithreading.DelegatesOrchestrator
{
    /// <summary>
    /// T is going to be a request Wrapper Class ex. T:{payload,headers} and 
    /// R is going to be a response Wrapper Class ex. R:{result,metadata}
    /// </summary>
    public class ThreadOrchestrator
    {

        
        public ThreadOrchestrator()
        {
            //doNothing
        }

        //fundamentals
        public delegate void Actions(); //parameterless void action
        public delegate void Actions<T>(T item1); //generic payload void
        public delegate R Funcs<out R>(); //parameterless func
        public delegate R Funcs<T, R>(T item1); //generic payload func
        //-----

        #region Data Structures - Exception Aggregation
        internal static List<Actions> actions = new List<Actions>();

        internal static class ActionsList<T>
        {
            internal static List<Tuple<Actions<T>, T>> actions = new List<Tuple<Actions<T>, T>>();
        }

        internal static class FuncList<R>
        {
            internal static List<Funcs<R>> funcs = new List<Funcs<R>>();
        }

        internal static class FuncList<T,R>
        {
            internal static List<Tuple<Funcs<T, R>, T>> funcs = new List<Tuple<Funcs<T, R>, T>>();
        }

        #endregion

        #region Data Structures - Indexed
        internal static ConcurrentDictionary<int, Actions> delegates = new ConcurrentDictionary<int, Actions>();

        internal static class DelegatesActionDict<T>
        {
            internal static ConcurrentDictionary<int,Tuple<Actions<T>, T>> delegates = new ConcurrentDictionary<int, Tuple<Actions<T>, T>>();
        }

        internal static class DelegatesFuncDict<R>
        {
            internal static ConcurrentDictionary<int,Funcs<R>> delegates = new ConcurrentDictionary<int, Funcs<R>>();
        }

        internal static class DelegatesFuncDict<T, R>
        {
            internal static ConcurrentDictionary<int,Tuple<Funcs<T, R>, T>> delegates = new ConcurrentDictionary<int, Tuple<Funcs<T, R>, T>>();
        }
        #endregion

        #region Initialize & Build Data Structures
        public void AddDelegates(List<Actions> input)
        {
            foreach (var action in input)
            {
                actions.Add(new Actions(action));
            }
        }

        public void AddDelegates<T>(List<Tuple<Actions<T>, T>> input)
        {

            foreach (var action in input)
            {
                ActionsList<T>.actions.Add(new Tuple<Actions<T>, T>(action.Item1, action.Item2));
            }
        }

        public void AddDelegates<R>(List<Funcs<R>> input)
        {
            foreach (var func in input)
            {
                FuncList<R>.funcs.Add(func);
            }
        }

        public void AddDelegates<T, R>(List<Tuple<Funcs<T, R>, T>> input)
        {
            foreach (var func in input)
            {
                FuncList<T, R>.funcs.Add(new Tuple<Funcs<T, R>,T>(func.Item1, func.Item2));
            }
        }

        #endregion

        #region Iitialize & Build Indexed Data Structures
        public void AddIndexedDelegates(Dictionary<int,Actions> input)
        {
            foreach (var action in input)
            {
                delegates.TryAdd(action.Key,new Actions(action.Value));
            }
        }

        public void AddIndexedDelegates<T>(Dictionary<int,Tuple<Actions<T>, T>> input)
        {
            foreach (var action in input)
            {
                DelegatesActionDict<T>.delegates.TryAdd
                    (action.Key,new Tuple<Actions<T>, T>(action.Value.Item1, action.Value.Item2));
            }
        }

        public void AddIndexedDelegates<R>(Dictionary<int,Funcs<R>> input)
        {
            foreach (var func in input)
            {
                DelegatesFuncDict<R>.delegates.TryAdd(func.Key,func.Value);
            }
        }

        public void AddIndexedDelegates<T, R>(Dictionary<int,Tuple<Funcs<T, R>,T>> input)
        {
            foreach (var func in input)
            {
                DelegatesFuncDict<T, R>.delegates.TryAdd
                    (func.Key,new Tuple<Funcs<T, R>, T>(func.Value.Item1, func.Value.Item2));
            }
        }
        #endregion

        #region Execution

        //execution just for parameterless voids - No wrapper classes
        public void Execute()
        {
            int workerThreads;
            int completionPortThreads;

            var voidActionsNum = actions.Count;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            //ThreadPool.QueueUserWorkItem(new WaitCallback())
            //Parallel.Invoke(actions.ToArray()); 
            var actionArray = new Action[actions.Count];
            int index = 0;
            foreach (var item in actions) { actionArray[index] = new Action(item);index++; };
            Parallel.Invoke(actionArray);
        }

        /// <summary>
        /// Aggregate exception
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, R> ExecuteParallel<T, R>()
        {
            var response = new ConcurrentDictionary<string, R>();
            var exceptionsQueue = new ConcurrentQueue<Exception>();

            Parallel.Invoke
            (
                () =>
                { 
                    Parallel.ForEach(actions, action =>
                    {
                        try
                        { 
                            //action(); //to fix make this List<action> instead of tuple
                        }
                        catch (Exception ex)
                        {
                            exceptionsQueue.Enqueue(ex);
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(ActionsList<T>.actions, action =>
                    {
                        try 
                        { 
                            action.Item1(action.Item2);
                        }
                        catch (Exception ex)
                        {
                            exceptionsQueue.Enqueue(ex);
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(FuncList<R>.funcs, func =>
                    {
                        try
                        {
                            string key = func.Method.Name;
                            var output = func.Invoke();
                            response.TryAdd(key, output);
                        }
                        catch (Exception ex)
                        {
                            exceptionsQueue.Enqueue(ex);
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(FuncList<T, R>.funcs, func =>
                    {
                        try
                        {
                            string key = func.Item1.Method.Name;
                            var output = func.Item1(func.Item2);
                            response.TryAdd(key, output);
                        }
                        catch (Exception ex)
                        {
                            exceptionsQueue.Enqueue(ex);
                        }
                    });
                }
            );

            if (exceptionsQueue.Count > 0) 
                throw new AggregateException(exceptionsQueue);
            
            return response;
        }

        /// <summary>
        /// returns exceptions in dictionary
        /// </summary>
        /// <returns></returns>
        
        public ConcurrentDictionary<int,object> ExecuteParallelWithExceptions<T, R>()
        {
            var response = new ConcurrentDictionary<int, object>();

            Parallel.Invoke
            (
                () =>
                {
                    Parallel.ForEach(delegates, action =>
                    {
                        try
                        {
                            action.Value();
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex, action.Key, response);
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(DelegatesActionDict<T>.delegates, action =>
                    {
                        try
                        {
                            action.Value.Item1(action.Value.Item2);
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex, action.Key, response);
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(DelegatesFuncDict<R>.delegates, func =>
                    {
                        int key = func.Key;
                        try
                        {
                            var output = func.Value.Invoke();
                            response.TryAdd(key, output);
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex, key, response);
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(DelegatesFuncDict<T, R>.delegates, func =>
                    {
                        int key = func.Key;
                        try
                        {
                            var output = func.Value.Item1(func.Value.Item2);
                            response.TryAdd(key, output);
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex, key, response);
                        }
                    });
                }
            );

            return response;
        }

        private static void HandleException(Exception ex,int key, ConcurrentDictionary<int, object> response)
        {
            var errorResp = new ErrorResponse
            {
                exception = ex.Message,
                stackTrace = ex.StackTrace
            };
            response.TryAdd(key, errorResp);
        }
        #endregion
    }
}
