using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multithreading
{
    public class OrchestratorM
    {
        /// <summary>
        /// T is going to be a request Wrapper Class ex. T:{payload,headers} and 
        /// R is going to be a response Wrapper Class ex. R:{result,metadata}
        /// </summary>
        
        //fundamentals
        public delegate void ActionVoids(); //parameterless void action
        public delegate void Action1Input<T>(T item1); //generic payload void
        public delegate R Func1Out<out R>(); //parameterless func
        public delegate R func1In1Out<T, R>(T item1); //generic payload func
        //-----

        //extras
        public delegate void Action2Inputs<T, R>(T item1, R item2);
        public delegate void Action3Inputs<T, R, J>(T item1, R item2, J item3);
        public delegate T Func2Input1Out<R, J, out T>(R item1, J item2);
        //-----

        //initializing global lists with all delegates from different Types
        #region global lists initialization
        internal static class GlobalArrays<T,R> where T : class 
                                                where R : class
        {
            internal static List<Action<T, R>> actions2Input = null;
        }

        internal static class GlobalArrays<T> where T : class
        {
            internal static List<Action<T>> actions1Input = null;
        }

        internal static List<Action> actions = new List<Action>();
        #endregion

        //initializing global dictionaries with all delegates from different Types
        #region global dictionaries initialization
        internal static class GlobalDicts<T, R>
        {
            internal static Dictionary<Action<T, R>, Tuple<T, R>> actions2Input = new Dictionary<Action<T, R>, Tuple<T, R>>();
        }

        internal static class GlobalDicts<T>
        {
            internal static Dictionary<Action<T>, T> actions1Input = new Dictionary<Action<T>, T>();
        }

        internal static class GlobalFuncDicts<T>
        {
            internal static Dictionary<Func<T>, T> func1Out = new Dictionary<Func<T>, T>();
        }

        internal static class GlobalFuncDicts<T,R>
        {
            internal static Dictionary<Func<T, R>, Tuple<T, R>> func1In1Out = new Dictionary<Func<T, R>, Tuple<T, R>>();
        }
        #endregion
        /*
        * Building Generic Delegates of 0,1,2 inputs void functions to be compatible with orchestrators actions
        */
        #region Build Generic Delegates Lists 0,1,2 Inputs
        public static void AddGenericDelegates(List<ActionVoids> actionsVoid)
        {
            foreach (var action in actionsVoid)
            {
                actions.Add(new Action(action));
            }
        }

        public static void AddGenericDelegates<T>(List<Action1Input<T>> actions1) where T:class
        {
            
            foreach (var action in actions1)
            {
                GlobalArrays<T>.actions1Input.Add(new Action<T>(action));
            }
        }

        public static void AddGenericDelegates<R, J>(List<Action2Inputs<R, J>> actions2) where R:class
                                                                                         where J:class
        {
            foreach (var action in actions2)
            {
                GlobalArrays<R,J>.actions2Input.Add(new Action<R, J>(action));
            }
        }
        #endregion

        #region Build Generic Delegates Dictionaries 0,1,2 Inputs
        public  void AddDelegates(List<ActionVoids> actionsVoid)
        {
            foreach (var action in actionsVoid)
            {
                actions.Add(new Action(action));
            }
        }

        public void AddDelegates<T>(Dictionary<Func1Out<T>,T> func1)
        {
            foreach (var func in func1)
            {
                GlobalFuncDicts<T>.func1Out.Add(new Func<T>(func.Key), default(T));
            }
        }

        public void AddDelegates<T>(Dictionary<Action1Input<T>,T> actions1)
        {

            foreach (var action in actions1)
            {
                GlobalDicts<T>.actions1Input.Add(new Action<T>(action.Key),action.Value);
            }
        }

        public  void AddDelegates<R, J>(Dictionary<Action2Inputs<R, J>,Tuple<R,J>> actions2)
        {
            foreach (var action in actions2)
            {
                GlobalDicts<R, J>.actions2Input.Add(new Action<R, J>(action.Key),new Tuple<R, J>(action.Value.Item1,action.Value.Item2));
            }
        }
        #endregion

        //dynamic execution
        public void Execute()
        {
            int workerThreads;
            int completionPortThreads;

            var voidActionsNum = actions.Count;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            //ThreadPool.QueueUserWorkItem(new WaitCallback())
        }

        public void Execute<T,R,J>()
        {
            Parallel.Invoke(() =>
            {
                foreach (var item in actions)
                {
                    item.Invoke();
                }
            }
            ,
            () => {
                foreach (var item in GlobalDicts<T>.actions1Input)
                {
                    item.Key(item.Value);
                }
            },
            () =>
            {
                foreach (var item in GlobalDicts<R,J>.actions2Input)
                {
                    item.Key(item.Value.Item1, item.Value.Item2);
                }
            }
            );
        }

        //dummy execution
        public static void Execute<T, R, J>(List<ActionVoids> actionsVoid, List<Action1Input<T>> actions1, T inpT, List<Action2Inputs<R, J>> actions2, R inpR, J inpJ)
            where T : class
            where R : class
            where J : class
        {
            
            Parallel.Invoke(() =>
            {
                foreach (var item in actions)
                {
                    item.Invoke();
                }
            }
            ,
            () => {
                foreach (var item in  GlobalArrays<T>.actions1Input)
                {
                    item(inpT);
                }
            },
            () =>
            {
                foreach (var item in GlobalArrays<R,J>.actions2Input)
                {
                    item(inpR, inpJ);
                }
            }
            );
        }
    }
}
