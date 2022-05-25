using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DelegatesExecutionBooster
{
    public interface IThreadOrchestrator
    {
        void AddDelegates(List<ThreadOrchestrator.Actions> input);
        void AddDelegates<R>(List<ThreadOrchestrator.Funcs<R>> input);
        void AddDelegates<T, R>(List<Tuple<ThreadOrchestrator.Funcs<T, R>, T>> input);
        void AddDelegates<T>(List<Tuple<ThreadOrchestrator.Actions<T>, T>> input);
        void AddIndexedDelegates(Dictionary<int, ThreadOrchestrator.Actions> input);
        void AddIndexedDelegates<R>(Dictionary<int, ThreadOrchestrator.Funcs<R>> input);
        void AddIndexedDelegates<T, R>(Dictionary<int, Tuple<ThreadOrchestrator.Funcs<T, R>, T>> input);
        void AddIndexedDelegates<T>(Dictionary<int, Tuple<ThreadOrchestrator.Actions<T>, T>> input);
        ConcurrentDictionary<string, object> ExecuteParallel<T, R>();
        ConcurrentDictionary<string, object> ExecuteParallel<T, R>(CancellationTokenSource cts);
        ConcurrentDictionary<string, R> ExecuteParallelAggregation<T, R>();
    }
}