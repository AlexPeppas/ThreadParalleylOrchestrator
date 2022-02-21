using DelegatesOrchestrator.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DelegatesOrchestrator
{
    public static class FunctionSimulations
    {
        public static void Work1InVoid(WrapperRequest request)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working"
                + Environment.NewLine +
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
            return new WrapperResponse { result = "{\"property1\":\"30\",\"property2\":\"12\"}" };
        }

        public static WrapperResponse Work1In1Out(WrapperRequest request)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working"
                + Environment.NewLine +
                " on WrapperRequest with WrapperResponse");
            Thread.Sleep(1500);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished");
            return new WrapperResponse { result = "{\"property1\":\"10\",\"property2\":\"15\"}" };
        }

        public static void WorkVoidParameterLess()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} started working.");
            Thread.Sleep(2000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished working.");
        }
    }
}
