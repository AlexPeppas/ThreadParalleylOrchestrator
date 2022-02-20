using System;
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
        //fundamentals
        public delegate void Actions(); //parameterless void action
        public delegate void Actions<T>(T item1); //generic payload void
        public delegate R Funcs<out R>(); //parameterless func
        public delegate R Funcs<T, R>(T item1); //generic payload func
        //-----

        #region Data Holders (Structures)
        internal static List<Action> actions = new List<Action>();

        internal static class ActionDictionary<T>
        {
            internal static Dictionary<Action<T>, T> actions = new Dictionary<Action<T>, T>();
        }
       
        internal static class FuncDictionary<T>
        {
            internal static Dictionary<Func<T>, T> funcs = new Dictionary<Func<T>, T>();
        }
        //simplified the upper FuncDictionary - no need for Output initialization
        internal static class FuncList<R>
        {
            internal static List<Func<R>> funcs = new List<Func<R>>();
        }

        internal static class FuncDictionary<T, R>
        {
            internal static Dictionary<Func<T, R>, Tuple<T, R>> funcs = new Dictionary<Func<T, R>, Tuple<T, R>>();
        }
        //simplified the upper FuncDictionary - no need for Output initialization
        internal static class FuncList<T,R>
        {
            internal static Dictionary<Func<T, R>, T> funcs = new Dictionary<Func<T, R>, T>();
        }
        #endregion

        #region Initialize & Build Data Holders
        public void AddDelegates(List<Actions> input)
        {
            foreach (var action in input)
            {
                actions.Add(new Action(action));
            }
        }

        public void AddDelegates<T>(Dictionary<Actions<T>, T> input)
        {

            foreach (var action in input)
            {
                ActionDictionary<T>.actions.Add(new Action<T>(action.Key), action.Value);
            }
        }

        /*public void AddDelegates<R>(List<Func<R>> input)
        {
            foreach (var func in input)
            {
                FuncDictionary<R>.funcs.Add(func, default(R));
            }
        }*/
        //simplified the upper AddDelegates - no need for Output initialization in Tuple
        public void AddDelegates<R>(List<Func<R>> input)
        {
            foreach (var func in input)
            {
                FuncList<R>.funcs.Add(func);
            }
        }

        /*public void AddDelegates<T,R>(Dictionary<Func<T,R>,Tuple<T,R>> input)
        {
            foreach (var func in input)
            {
                FuncDictionary<T, R>.funcs.Add(new Func<T, R>(func.Key),new Tuple<T, R>(func.Value.Item1,default(R)));
            }
        }*/
        //simplified the upper AddDelegates - no need for Output initialization in Tuple
        public void AddDelegates<T, R>(Dictionary<Func<T, R>, T> input)
        {
            foreach (var func in input)
            {
                FuncList<T, R>.funcs.Add(new Func<T, R>(func.Key), func.Value);
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
            Parallel.Invoke(actions.ToArray());
        }

        public Dictionary<string, R> Execute<T,R>()
        {
            var response = new Dictionary<string, R>();

            //parallel invoke for all data holders (optimize it with custom task factory
            Parallel.Invoke(
            //parallely execute the parameterless voids
            () =>
                {
                    Parallel.Invoke(actions.ToArray());
                },
            //parallely execute voids with their input wrapper class
            () =>
                {
                    foreach (var item in ActionDictionary<T>.actions)
                    {
                        item.Key(item.Value);
                    }
                },
            //parallely execute the parameterless Funcs and store their result in the initial dictionary
            ()=>
                {
                    foreach (var item in FuncList<R>.funcs)
                    {
                        string key = item.Method.Name;
                        var output = item.Invoke();
                        response.Add(key, output) ;
                    }
                },
            //parallely execute the Funcs and update the dict value to hold the new Tuple with the new Output
            () =>
                {
                    foreach (var item in FuncList<T,R>.funcs)
                    {
                        var output = item.Key(item.Value);                        
                        response.Add(item.Key.Method.Name, output);
                    }
                }
            );

            return response;
        }

        #endregion
    }
}
