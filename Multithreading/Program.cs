using DelegatesOrchestrator;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace Multithreading
{
    class Program
    {
        public delegate void ActionVoids();
        public delegate void Action1Input<T>(T item1);
        public delegate void Action2Inputs<T, R>(T item1, R item2);
        public delegate void Action3Inputs<T, R, J>(T item1, R item2, J item3);
        public delegate T Func2Input1Out<R, J,out T>(R item1, J item2);

        static void Main(string[] args)
        {
            var DelegatesOrch = new MasterThread();
            try { DelegatesOrch.Main(); }
            catch (AggregateException ex)
            {
                foreach (var item in ex.Flatten().InnerExceptions) 
                {
                    Console.WriteLine(item.Message + " , " + item.StackTrace);
                }
            }
            
            //
            //Chain of Functions (Dependency on the previous)
            Task<WrapperResponse>.Factory.StartNew(() =>
            Work1In1Out(new WrapperRequest { })).
            ContinueWith(prevTask => Work1InVoid(new WrapperRequest { }));
            //

            var tOrch = new ThreadOrchestrator();

            var request = new WrapperRequest { payload = "Stringified object" };
            var response = new WrapperResponse { result = "Stringified response"};
            
            #region build ThreadOrchestrator dictionaries
            tOrch.AddDelegates(new List<ThreadOrchestrator.Actions> 
            { 
                DoSomethingParameterLess,
                DoSomethingOtherParameterLess
            });

            tOrch.AddDelegates<WrapperRequest>(new Dictionary<ThreadOrchestrator.Actions<WrapperRequest>, WrapperRequest>
            {
                {Work1InVoid,request }
            });

            tOrch.AddDelegates<WrapperResponse>(new List<Func<WrapperResponse>>
            {
                Work1OutParameterless
            });

            tOrch.AddDelegates<WrapperRequest,WrapperResponse>(new Dictionary<Func<WrapperRequest, WrapperResponse>, WrapperRequest>
            {
                {Work1In1Out,request }
            });
            #endregion

            tOrch.Execute();
            var responseTOrch = tOrch.Execute<WrapperRequest, WrapperResponse>();

            /*var orch = new OrchestratorM();
            orch.AddDelegates(new List<OrchestratorM.ActionVoids> { DoSomethingParameterLess });
            

            orch.AddDelegates(new Dictionary<OrchestratorM.Action1Input<int>, int> { {DoSomething1Input, 5 } });
            orch.AddDelegates(new Dictionary<OrchestratorM.Action1Input<string>, string> { { DoSomething1Input, "5" } }); // how will this execute when we have different types ?
            orch.AddDelegates(new Dictionary<OrchestratorM.Action2Inputs<int, int>, Tuple<int, int>> { { DoSomething2Input,new Tuple<int,int>(10,12 )} });
            orch.Execute<int,int,int>(); // how will this execute when we have different types ? example lines 22-23
            orch.Execute<string, string, string>();

            BuildGenericDelegates();
            ConcurrencyGenericTaskFactory(DoSomethingParameterLess); //everything starts concurrent
            ConcurrencyGenericTaskFactory(new Dictionary<int, List<Action>> //thread by thread
            {
                { 1,new List<Action>{ DoSomethingParameterLess, DoSomethingParameterLess} },
                { 2,new List<Action>{ DoSomethingParameterLess} }
            });
            ConcurrencyGenericParallel(new Dictionary<int, List<Action>> //thread by thread
            {
                { 1,new List<Action>{ DoSomethingParameterLess, DoSomethingOtherParameterLess } }
            });
            Concurrency();

            Orchestrator(new List<ActionVoids> { DoSomethingParameterLess },
                new List<Action1Input<int>> { DoSomething1Input }, 5,
                new List<Action2Inputs<int, int>> { DoSomething2Input }, 10, 12);*/
        }

        //orchestrator receives lists of delegates (0,1,2 inputs voids) formats them with BuildGenericDelegates function and then execute them in parallel.
        public static void Orchestrator<T, R, J>(List<ActionVoids> actionsVoid, List<Action1Input<T>> actions1, T inpT, List<Action2Inputs<R, J>> actions2, R inpR, J inpJ)
        {
            var actionsVoidArray = BuildGenericDelegates(actionsVoid);
            var actions1Array = BuildGenericDelegates(actions1); //T
            var actions2Array = BuildGenericDelegates(actions2); //R,J

            //Parallel.Invoke(actionsVoidArray);
            Parallel.Invoke(() =>
            {
                foreach (var item in actionsVoidArray)
                {
                    item.Invoke();
                }
            }
            ,
            () =>{
                foreach (var item in actions1Array)
                {
                    item(inpT);
                }
            },
            () =>
            {
                foreach (var item in actions2Array)
                {
                    item(inpR, inpJ);
                }
            }
            );
        }
        /*
         * Building Generic Delegates of 0,1,2 inputs void functions to be compatible with orchestrators actions
         */
        #region Build Generic Delegates 0,1,2 Inputs
        public static Action[] BuildGenericDelegates(List<ActionVoids> actionsVoid)
        {
            var actionsArray = new Action[actionsVoid.Count];
            int index = 0;
            foreach ( var action in actionsVoid)
            {
                actionsArray[index++] = new Action(action);
            }
            return actionsArray;
        }

        public static Action<T>[] BuildGenericDelegates<T>(List<Action1Input<T>> actions1)
        {
            var actionsArray = new Action<T>[actions1.Count];
            int index = 0;
            foreach (var action in actions1)
            {
                actionsArray[index++] = new Action<T>(action);
            }
            return actionsArray;
        }

        public static Action<R,J>[] BuildGenericDelegates<R,J>(List<Action2Inputs<R, J>> actions2)
        {
            var actionsArray = new Action<R,J>[actions2.Count];
            int index = 0;
            foreach (var action in actions2)
            {
                actionsArray[index++] = new Action<R,J>(action);
            }
            return actionsArray;
        }

        public static void BuildGenericDelegates()
        {
            Action3Inputs<int, int, CancellationToken> doSmthThree = new Action3Inputs<int, int, CancellationToken>(DoSomething);
            Action<int, int, CancellationToken> action = new Action<int, int, CancellationToken>(doSmthThree);
            CancellationTokenSource source = new CancellationTokenSource();
            /*var actions = new Action<int, int, CancellationToken>[1];
            actions[0] = action;*/
            Parallel.Invoke(
                ()=> action(1, 1500, source.Token)
                );
        }
        #endregion

        #region Concurrency Approaches
        //Concurrent execution of list of Actions 
        public static void ConcurrencyGenericParallel(List<Action> dictOfFuncs)
        {
            Parallel.Invoke(dictOfFuncs.ToArray());
        }

        //To Fix Task.Factory for dynamic concurrent execution
        public static void ConcurrencyGenericTaskFactory(Dictionary<int, List<Action>> dictOfFuncs)
        {
            
            foreach (var thread in dictOfFuncs)
            {
                Console.WriteLine($"thread : {thread.Key} started working ");
                var listOfActions = thread.Value;
                var tasks = new Task[listOfActions.Count];
                tasks[0] = Task.Factory.StartNew(listOfActions[0]);
                for (int i=1;i<listOfActions.Count;i++)
                {
                    tasks[i] = tasks[i - 1].ContinueWith((prevTask) => listOfActions[i]);
                }
                Task.WaitAll(tasks);
            }
         
        }

        //dummy parallel execution with Task.Factory
        public static void ConcurrencyGenericTaskFactory(Action doSmth)
        {
            var awaitList = new Task[2];
            awaitList[0] = Task.Factory.StartNew(doSmth);
            awaitList[1] = Task.Factory.StartNew(doSmth);
            Task.WaitAll(awaitList);
        }

        //thread executes list of actions, then next thread of dictionary starts
        public static void ConcurrencyGenericParallel(Dictionary<int, List<Action>> dictOfFuncs)
        {
            foreach (var actionList in dictOfFuncs)
            {
                Console.WriteLine($" Task {actionList.Key} started!");
                Parallel.Invoke(actionList.Value.ToArray());
            }
        }

        //dummy static concurrency
        public static void Concurrency()
        {

            CancellationTokenSource source = new CancellationTokenSource();
            try
            {
                var t1 = Task.Factory.StartNew(() => DoSomething(1, 2000, source.Token)).ContinueWith((previousTask) => DoSomethingMore(1, 1500, source.Token));
                var t2 = Task.Factory.StartNew(() => DoSomething(2, 2000, source.Token)).ContinueWith((previousTask) => DoSomethingMore(2, 1500, source.Token));
                var t3 = Task.Factory.StartNew(() => DoSomething(3, 2000, source.Token)).ContinueWith((previousTask) => DoSomethingMore(3, 1500, source.Token));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.GetType());
            }
            //List<Task> taskList = new List<Task> { t1, t2, t3 };

            //Task.WaitAll(taskList.ToArray());

            Console.WriteLine("Press 'C' to cancel the running jobs");
            var key = Console.ReadKey().KeyChar;
            if (key=='C' || key=='c')
            {
                source.Cancel();
                Console.WriteLine("Cancellation Requested!");
            }
            else
                Console.WriteLine("All tasks finished successfully!");
        }
        #endregion

        #region Thread Job Simulation

        public static void Work1InVoid (WrapperRequest request)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working"
                + Environment.NewLine+
                " on WrapperRequest with void output");
            Thread.Sleep(1500);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished");
        }

        public static WrapperResponse Work1OutParameterless()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working"
                + Environment.NewLine +
                " parameterless with WrapperResponse");
            Thread.Sleep(1500);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished");
            return new WrapperResponse { result = $"result_of_Td_{Thread.CurrentThread.ManagedThreadId}" };
        }

        public static WrapperResponse Work1In1Out(WrapperRequest request)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working"
                + Environment.NewLine +
                " on WrapperRequest with WrapperResponse");
            Thread.Sleep(1500);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished");
            return new WrapperResponse { result = $"result_of_Td_{Thread.CurrentThread.ManagedThreadId}" };
        }

        public static void WorkVoidParameterLess()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working.");
            Thread.Sleep(2000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished working.");
        }

        public static void DoSomethingParameterLess()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working.");
            Thread.Sleep(2000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished working.");
        }

        public static void DoSomething1Input(int num)
        {
            Console.WriteLine($"thread {Thread.CurrentThread.ManagedThreadId} received number : {num}");
            Thread.Sleep(1500);
        }

        public static void DoSomething1Input(string num)
        {
            Console.WriteLine($"thread {Thread.CurrentThread.ManagedThreadId} received string : {num}");
            Thread.Sleep(1500);
        }

        public static void DoSomething2Input(int num, int num2)
        {
            Console.WriteLine($"thread {Thread.CurrentThread.ManagedThreadId} received numbers : {num} & {num2}");
            Thread.Sleep(1500);
        }

        public static void DoSomethingOtherParameterLess()
        {
            Console.WriteLine($"Thread started more working.");
            Thread.Sleep(1500);
            Console.WriteLine($"Thread finished more working.");
        }

        public static void DoSomething(int threadId, int timeToSleep,CancellationToken cancellationToken )
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Cancellation has been requested");
                //throw new OperationCanceledException();
                cancellationToken.ThrowIfCancellationRequested();
            }
            Console.WriteLine($"Thread {threadId} started working.");
            Thread.Sleep(timeToSleep);
            Console.WriteLine($"Thread {threadId} finished working.");
        }

        public static void DoSomethingMore(int threadId, int timeToSleep, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Cancellation has been requested");
                //throw new OperationCanceledException();
                cancellationToken.ThrowIfCancellationRequested();
            }
            Console.WriteLine($"Thread {threadId} started more work.");
            Thread.Sleep(timeToSleep);
            Console.WriteLine($"Thread {threadId} finally finished.");
        }
        #endregion
    }

    public class ObjectSimulation
    {
        public string  prop1 {get;set;}
        public string prop2 {get;set;}
    }

    public class WrapperRequest
    {
        public string payload { get; set; }
    }

    public class WrapperResponse
    {
        public string result { get; set; }
    }
}

