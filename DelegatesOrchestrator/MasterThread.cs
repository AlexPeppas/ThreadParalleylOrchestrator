using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelegatesOrchestrator.Types;
using Multithreading.DelegatesOrchestrator;
using System.Runtime.Serialization;
using static Multithreading.DelegatesOrchestrator.ThreadOrchestrator;
using System.Text.Json;

namespace DelegatesOrchestrator
{
    public class MasterThread
    {
        public void Main()
        {
            var tOrch = new ThreadOrchestrator();
            WrapperRequest request = new WrapperRequest();
            request.payload = JsonSerializer.Serialize(new DummyRequest { property1 = "10", property2 = 10 });

            #region build ThreadOrchestrator dictionaries
            tOrch.AddDelegates(new List<ThreadOrchestrator.Actions>
            {
                FunctionSimulations.WorkVoidParameterLess,
                FunctionSimulations.WorkVoidParameterLess
            });

            tOrch.AddDelegates<WrapperRequest>(new List<Tuple<ThreadOrchestrator.Actions<WrapperRequest>, WrapperRequest>>
            { 
                new Tuple<ThreadOrchestrator.Actions<WrapperRequest>, WrapperRequest> (FunctionSimulations.Work1InVoid,request)
            });

            tOrch.AddDelegates<WrapperResponse>(new List<Funcs<WrapperResponse>>
            {
                FunctionSimulations.Work1OutParameterless
            });

            tOrch.AddDelegates<WrapperRequest, WrapperResponse>(new List<Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>>
            {
                new Tuple<Funcs<WrapperRequest, WrapperResponse>, WrapperRequest>(FunctionSimulations.Work1In1Out,request)
            });
            #endregion

            tOrch.Execute();
            var responseTOrch = tOrch.Execute<WrapperRequest, WrapperResponse>();

            try
            {
                var response2 = tOrch.ExecuteParallel<WrapperRequest, WrapperResponse>();
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
            
            foreach (var item in responseTOrch)
            {
                Console.WriteLine($"Function with Name {item.Key} returned the object with name : {item.Value.GetType().Name} and assembly : {item.Value.GetType().Assembly}");
                var props = JsonSerializer.Deserialize<DummyResponse>(item.Value.result);
                Console.WriteLine($"With props res1 : {0} , res2 : {1}", props.property1, props.property2);
            }
        }

    }

}
