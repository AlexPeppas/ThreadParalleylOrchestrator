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
                { 7, FunctionSimulations.WorkWithExceptionParameterless }
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
                {0, FunctionSimulations.WorkWithException }
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

            var cts = new System.Threading.CancellationTokenSource();
            Task.Factory.StartNew
            (
                () =>
                {
                    var responseWithCancel = tOrch.ExecuteParallel<WrapperRequest, WrapperResponse>(cts);
                }
            );

            Console.WriteLine("Give me a key");
            if (Console.ReadKey().KeyChar == 'c')
                cts.Cancel();
            Console.WriteLine("Jobs cancelled. Press any key to exit");
            //var responseWithCancel = tOrch.ExecuteParallelWithCancellation<WrapperRequest, WrapperResponse>(new System.Threading.CancellationTokenSource());

            var time = Stopwatch.StartNew();
            time.Start();
            var response = tOrch.ExecuteParallel<WrapperRequest, WrapperResponse>();
            time.Stop();
            
            Console.WriteLine($"33 functions with avg working time of 3 seconds that would totally cost {3*33} seconds, eventually costed {time.ElapsedMilliseconds / 1000} seconds");


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

            tOrch.Execute();

            try
            {
                var responseTOrch = tOrch.ExecuteParallelAggregation<WrapperRequest, WrapperResponse>();
            }
            catch (AggregateException exceptions)
            {
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
