using DelegatesOrchestrator.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            //core's max threads
            //cancellation token to cancel every running delegate ? 
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


        /// <summary>
        /// Execute parameterless Actions - No Wrapper Class
        /// <hint>To retire</hint>
        /// </summary>
        /// <returns></returns>
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
        /// Executes parallely every delegate that has been added in lists, 
        /// Every exception occurs enqueues in a ConcurrentQueue
        /// Enqueued exceptions are being aggregated
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, R> ExecuteParallelAggregation<T, R>()
        {
            var response = new ConcurrentDictionary<string, R>();
            var exceptionsQueue = new ConcurrentQueue<Exception>();

            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.Invoke
            (
                () =>
                { 
                    Parallel.ForEach(actions, options, action =>
                    {
                        try
                        { 
                            action();
                        }
                        catch (Exception ex)
                        {
                            exceptionsQueue.Enqueue(ex);
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(ActionsList<T>.actions, options, action =>
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
                    Parallel.ForEach(FuncList<R>.funcs, options, func =>
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
                    Parallel.ForEach(FuncList<T, R>.funcs, options, func =>
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
        /// Executes parallely every delegate that has been added in dictionaries,
        /// Responses and Exceptions are being added in a concurrentDictionary
        /// with the index of each delegate + a Suffix (_Func,_Action,_FuncParameterless,_ActionParameterless) as Key
        /// and type either WrapperResponse or ErrorResponse Dict<string,object> as Value
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string,object> ExecuteParallel<T, R>()
        {
            var response = new ConcurrentDictionary<string, object>();

            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.Invoke
            (
                () =>
                {
                    Parallel.ForEach(delegates,options, action =>
                    {
                        try
                        {
                            action.Value();
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex, action.Key, response, "_ActionParameterless");
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(DelegatesActionDict<T>.delegates, options, action =>
                    {
                        try
                        {
                            action.Value.Item1(action.Value.Item2);
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex, action.Key, response, "_Action");
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(DelegatesFuncDict<R>.delegates, options, func =>
                    {
                        int key = func.Key;
                        try
                        {
                            var output = func.Value.Invoke();

                            HandleResponse<R>(output,key,response, "_FuncParameterless");
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex, key, response, "_FuncParameterless");
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(DelegatesFuncDict<T, R>.delegates, options, func =>
                    {
                        int key = func.Key;
                        try
                        {
                            var output = func.Value.Item1(func.Value.Item2);

                            HandleResponse<R>(output, key, response, "_Func");
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex, key, response, "_Func");
                        }
                    });
                }
            );
            
            return response;
        }

        /// <summary>
        /// Executes parallely every delegate that has been added in dictionaries,
        /// Responses and Exceptions are being added in a concurrentDictionary
        /// with the index of each delegate + a Suffix (_Func,_Action,_FuncParameterless,_ActionParameterless) as Key
        /// and type either WrapperResponse or ErrorResponse Dict<string,object> as Value
        /// Accepts a CancellationTokenSource so the execution can be stopped by another thread
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, object> ExecuteParallel<T, R>(CancellationTokenSource cts)
        {
            var response = new ConcurrentDictionary<string, object>();

            ParallelOptions options = new ParallelOptions
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            try
            {
                Parallel.Invoke
                (
                    () =>
                    {
                        Parallel.ForEach(delegates,options, action =>
                        {
                            
                            try
                            {
                                if (options.CancellationToken.IsCancellationRequested) 
                                    options.CancellationToken.ThrowIfCancellationRequested();

                                action.Value();
                            }
                            catch (OperationCanceledException cancelEx)
                            {
                                HandleException(cancelEx, action.Key, response, "_ActionParameterless");
                                
                            }
                            catch (Exception ex)
                            {
                                HandleException(ex, action.Key, response, "_ActionParameterless");
                            }
                            finally
                            {
                                cts.Dispose();
                            }
                        });
                    },
                    () =>
                    {
                        Parallel.ForEach(DelegatesActionDict<T>.delegates, options, action =>
                        {
                            
                            try
                            {
                                if (options.CancellationToken.IsCancellationRequested)
                                    options.CancellationToken.ThrowIfCancellationRequested();

                                action.Value.Item1(action.Value.Item2);
                            }
                            catch (OperationCanceledException cancelEx)
                            {
                                HandleException(cancelEx, action.Key, response, "_Action");
                                
                            }
                            catch (Exception ex)
                            {
                                HandleException(ex, action.Key, response, "_Action");
                            }
                            finally
                            {
                                cts.Dispose();
                            }
                        });
                    },
                    () =>
                    {
                        Parallel.ForEach(DelegatesFuncDict<R>.delegates, options, func =>
                        {
                            
                            int key = func.Key;
                            try
                            {
                                if (options.CancellationToken.IsCancellationRequested)
                                    options.CancellationToken.ThrowIfCancellationRequested();

                                var output = func.Value.Invoke();

                                HandleResponse<R>(output, key, response, "_FuncParameterless");
                            }
                            catch (OperationCanceledException cancelEx)
                            {
                                HandleException(cancelEx, key, response, "_FuncParameterless");
                                
                            }
                            catch (Exception ex)
                            {
                                HandleException(ex, key, response, "_FuncParameterless");
                            }
                            finally
                            {
                                cts.Dispose();
                            }
                        });
                    },
                    () =>
                    {
                        Parallel.ForEach(DelegatesFuncDict<T, R>.delegates, options, func =>
                        {
                            
                            int key = func.Key;
                            try
                            {
                                if (options.CancellationToken.IsCancellationRequested)
                                    options.CancellationToken.ThrowIfCancellationRequested();

                                var output = func.Value.Item1(func.Value.Item2);

                                HandleResponse<R>(output, key, response, "_Func");
                            }
                            catch (OperationCanceledException cancelEx)
                            {
                                HandleException(cancelEx, key, response, "_Func");
                                
                            }
                            catch (Exception ex)
                            {
                                HandleException(ex, key, response, "_Func");
                            }
                            finally
                            {
                                cts.Dispose();
                            }
                        });
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            return response;
        }

        private static void HandleException(OperationCanceledException ex,int key, ConcurrentDictionary<string, object> response,string suffix)
        {
            var errorResp = new ErrorResponse
            {
                exception = "Operation Canceled!",
                stackTrace = null
            };
                
            response.TryAdd(key + suffix, errorResp);
            
        }

        private static void HandleException(Exception ex, int key, ConcurrentDictionary<string, object> response, string suffix)
        {
            var errorResp = new ErrorResponse
            {
                exception = ex.Message,
                stackTrace = ex.StackTrace
            };

            response.TryAdd(key + suffix, errorResp);

        }

        private static void HandleResponse<R>(R output,int key, ConcurrentDictionary<string, object> response, string suffix)
        {
            response.TryAdd(key + suffix, output);
        }
        #endregion


    }

    

}
