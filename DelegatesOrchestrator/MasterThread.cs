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

            #region build ThreadOrchestrator dictionaries
            tOrch.AddDelegates(new List<ThreadOrchestrator.Actions>
            {
                FunctionSimulations.WorkVoidParameterLess,
                FunctionSimulations.WorkVoidParameterLess
            });

            tOrch.AddDelegates<WrapperRequest>(new List<Tuple<ThreadOrchestrator.Actions<WrapperRequest>, WrapperRequest>>
            {
                new Tuple<ThreadOrchestrator.Actions<WrapperRequest>, WrapperRequest> (FunctionSimulations.Work1InVoid,request),
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

            //tOrch.Execute();
            
            //var responseTOrch = tOrch.Execute<WrapperRequest, WrapperResponse>();
            
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
