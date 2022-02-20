using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelegatesOrchestrator.Types;
using Multithreading.DelegatesOrchestrator;
using static Multithreading.DelegatesOrchestrator.ThreadOrchestrator;

namespace DelegatesOrchestrator
{
    public class MasterThread
    {
        public void Main()
        {
            var tOrch = new ThreadOrchestrator();

            var request = new WrapperRequest { payload = "Stringified object" };
            var response = new WrapperResponse { result = "Stringified response" };

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
            
        }

    }

}
