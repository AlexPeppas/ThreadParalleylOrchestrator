using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelegatesOrchestrator.Types;
using Multithreading.DelegatesOrchestrator;
using System.Runtime.Serialization;
using static Multithreading.DelegatesOrchestrator.ThreadOrchestrator;
using System.Diagnostics;
using System.Threading;

namespace DelegatesOrchestrator
{
    public class MasterThread
    {
        public void Main()
        {
            var tOrch = new ThreadOrchestrator();
            WrapperRequest request = new WrapperRequest();
            request.payload = "2022";

            #region Build Orchestrators Dictionaries
            tOrch.AddIndexedDelegates(new Dictionary<int, Actions>
            {
                { 0,FunctionSimulations.WorkWithExceptionParameterless},
                { 1, FunctionSimulations.WorkVoidParameterLess },
                { 2, FunctionSimulations.WorkVoidParameterLess },
                { 3, FunctionSimulations.WorkWithExceptionParameterless },
                { 4,FunctionSimulations.WorkWithExceptionParameterless},
                { 5, FunctionSimulations.WorkVoidParameterLess },
                { 6, FunctionSimulations.WorkVoidParameterLess },
                { 7, FunctionSimulations.WorkWithExceptionParameterless },
                { 8, FunctionSimulations.DbDummyFetch },
                { 10,FunctionSimulations.WorkWithExceptionParameterless},
                { 11, FunctionSimulations.WorkVoidParameterLess },
                { 12, FunctionSimulations.WorkVoidParameterLess },
                { 13, FunctionSimulations.WorkWithExceptionParameterless },
                { 14,FunctionSimulations.WorkWithExceptionParameterless},
                { 15, FunctionSimulations.WorkVoidParameterLess },
                { 16, FunctionSimulations.WorkVoidParameterLess },
                { 17, FunctionSimulations.WorkWithExceptionParameterless },
                { 18, FunctionSimulations.DbDummyFetch }
            });
            
            tOrch.AddIndexedDelegates(new Dictionary<int, Tuple<Actions<WrapperRequest>, WrapperRequest>>
            {
                {3, new Tuple<Actions<WrapperRequest>, WrapperRequest> (FunctionSimulations.Work1InVoid,request) },
                {4, new Tuple<Actions<WrapperRequest>,WrapperRequest>(FunctionSimulations.WorkWithException,request) },
                {5, new Tuple<Actions<WrapperRequest>,WrapperRequest>(FunctionSimulations.WorkWithException,request) },
                {1, new Tuple<Actions<WrapperRequest>, WrapperRequest> (FunctionSimulations.Work1InVoid,request) },
                {2, new Tuple<Actions<WrapperRequest>,WrapperRequest>(FunctionSimulations.WorkWithException,request) },
                {0, new Tuple<Actions<WrapperRequest>,WrapperRequest>(FunctionSimulations.WorkWithException,request) },
                {6, new Tuple<Actions<WrapperRequest>, WrapperRequest> (FunctionSimulations.Work1InVoid,request) },
                {7, new Tuple<Actions<WrapperRequest>,WrapperRequest>(FunctionSimulations.WorkWithException,request) },
                {8, new Tuple<Actions<WrapperRequest>,WrapperRequest>(FunctionSimulations.WorkWithException,request) }
            });

            tOrch.AddIndexedDelegates(new Dictionary<int,Funcs<WrapperResponse>>
            {
                {1, FunctionSimulations.Work1OutParameterless },
                {2, FunctionSimulations.WorkWithException },
                {3, FunctionSimulations.Work1OutParameterless },
                {4, FunctionSimulations.WorkWithException },
                {5, FunctionSimulations.Work1OutParameterless },
                {6, FunctionSimulations.WorkWithException },
                {7, FunctionSimulations.Work1OutParameterless },
                {0, FunctionSimulations.WorkWithException },
                {11, FunctionSimulations.Work1OutParameterless },
                {12, FunctionSimulations.WorkWithException },
                {13, FunctionSimulations.Work1OutParameterless },
                {14, FunctionSimulations.WorkWithException },
                {15, FunctionSimulations.Work1OutParameterless },
                {16, FunctionSimulations.WorkWithException },
                {17, FunctionSimulations.Work1OutParameterless },
                {10, FunctionSimulations.WorkWithException }
            });

            tOrch.AddIndexedDelegates(new Dictionary<int,Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>>
            {
                {1, new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request) },
                {2, new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request) },
                {7, new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request) },
                {8, new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request) },
                {3, new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request) },
                {0, new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request) },
                {10, new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request) },
                {15, new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request) }
            });
            #endregion

            /*var time = Stopwatch.StartNew();
            time.Start();
            var cts = new System.Threading.CancellationTokenSource();
            Task.Factory.StartNew
            (
                () =>
                {
                    tOrch.ExecuteParallel<WrapperRequest, WrapperResponse>(cts);
                }
            );
            //tOrch.TerminateExecution(cts);
            Console.WriteLine("Press 'C' to cancel the running jobs");
            var key = Console.ReadKey().KeyChar;
            if (key == 'C' || key == 'c')
            {
                cts.Cancel();
                Console.WriteLine("Cancellation Requested!");
            }
            time.Stop();*/
            var cts = new System.Threading.CancellationTokenSource();
            Task.Run(() =>
            {
                Console.WriteLine("Press 'C' to cancel the running jobs");
                var key = Console.ReadKey().KeyChar;
                if (key == 'C' || key == 'c')
                {
                    Console.WriteLine("Cancellation Requested!");
                    cts.Cancel();
                    Console.WriteLine("Operations Cancelled!");
                    cts.Dispose();
                }
            });

            var time = Stopwatch.StartNew();
            time.Start();
            
            var responseWithCancel = tOrch.ExecuteParallel<WrapperRequest, WrapperResponse>(cts);
            
            time.Stop();
            
            Console.WriteLine($"50 functions with avg working time of 3 seconds that would totally cost {3 * 50} seconds, eventually costed {time.ElapsedMilliseconds / 1000} seconds");

            time = Stopwatch.StartNew();
            time.Start();
            var response = tOrch.ExecuteParallel<WrapperRequest, WrapperResponse>();
            time.Stop();
            
            Console.WriteLine($"34 functions with avg working time of 3 seconds that would totally cost {3*34} seconds, eventually costed {time.ElapsedMilliseconds / 1000} seconds");


            #region Build ThreadOrchestrator Lists
            tOrch.AddDelegates(new List<Actions>
            {
                FunctionSimulations.WorkWithExceptionParameterless,
                FunctionSimulations.WorkVoidParameterLess
            });

            tOrch.AddDelegates<WrapperRequest>(new List<Tuple<Actions<WrapperRequest>, WrapperRequest>>
            {
                new Tuple<Actions<WrapperRequest>, WrapperRequest> (FunctionSimulations.Work1InVoid,request),
                new Tuple<Actions<WrapperRequest>,WrapperRequest>(FunctionSimulations.WorkWithException,request)
            });

            tOrch.AddDelegates<WrapperResponse>(new List<Funcs<WrapperResponse>>
            {
                FunctionSimulations.Work1OutParameterless,
                FunctionSimulations.WorkWithException
            });
            
            tOrch.AddDelegates<WrapperRequest, WrapperResponse>(new List<Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>>
            {
                new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request)
            });
            #endregion

            time.Start();
            tOrch.Execute();
            time.Stop();

            try
            {
                time.Start();
                var responseTOrch = tOrch.ExecuteParallelAggregation<WrapperRequest, WrapperResponse>();
                time.Stop();
            }
            catch (AggregateException exceptions)
            {
                time.Stop();
                var ignoredExceptions = new List<Exception>();
                foreach (var ex in exceptions.Flatten().InnerExceptions)
                {
                    if (ex is ArgumentException)
                        Console.WriteLine(ex.Message);
                    else
                        ignoredExceptions.Add(ex);
                }
                if (ignoredExceptions.Count > 0) throw new AggregateException(ignoredExceptions);
            }
        }

    }

}
