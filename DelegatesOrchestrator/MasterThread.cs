using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelegatesOrchestrator.Types;
using Multithreading.DelegatesOrchestrator;
using System.Runtime.Serialization;
using static Multithreading.DelegatesOrchestrator.ThreadOrchestrator;


namespace DelegatesOrchestrator
{
    public class MasterThread
    {
        public void Main()
        {
            var tOrch = new ThreadOrchestrator();
            WrapperRequest request = new WrapperRequest();
            request.payload = "2010";

            #region Build Orchestrators Dictionaries
            tOrch.AddIndexedDelegates(new Dictionary<int, Actions>
            {
                { 0,FunctionSimulations.WorkWithExceptionParameterless},
                { 1, FunctionSimulations.WorkVoidParameterLess },
                { 2, FunctionSimulations.WorkVoidParameterLess }
            });
            
            tOrch.AddIndexedDelegates(new Dictionary<int, Tuple<Actions<WrapperRequest>, WrapperRequest>>
            {
                {3, new Tuple<Actions<WrapperRequest>, WrapperRequest> (FunctionSimulations.Work1InVoid,request) },
                {4, new Tuple<Actions<WrapperRequest>,WrapperRequest>(FunctionSimulations.WorkWithException,request) }
            });

            tOrch.AddIndexedDelegates(new Dictionary<int,Funcs<WrapperResponse>>
            {
                {5, FunctionSimulations.Work1OutParameterless },
                {6, FunctionSimulations.WorkWithException }
            });

            tOrch.AddIndexedDelegates(new Dictionary<int,Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>>
            {
                {7, new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request) },
                {8, new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request) }
            });
            #endregion

            var response = tOrch.ExecuteParallelWithExceptions<WrapperRequest, WrapperResponse>();

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
                var responseTOrch = tOrch.ExecuteParallel<WrapperRequest, WrapperResponse>();
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
