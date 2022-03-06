Delegates orchestratos should be used to optimize system/application performance
by decreasing runtime execution using parallelization.

You can add your delegates (Action, Parameterless Action, Func, Parameterless Func)
and by using the ExecuteParallel functions your delegates are going to operate in parallel.

To avoid adding multiple inputs (and handling multiple outputs for the Func types
delegates) each delegate should use a WrapperRequest { string payload, string headers}
where you can serialize your input and ?headers so different type of payloads
won't make the orchestrator to run those types consecutive. 
Similarly, your functions that the Func delegates are pointing at should return
a WrapperResponse type based on the same philosophy {string output, string metadata}.

It is suggested not to modify your prototype functions but rather create 
wrapper functions for this specific handling, 

Ex. 

public WrapperResponse WrapperFunctionOfSleeperFunction (WrapperRequest request)
{
	T payload = Json.DeserializeObject<T>(request.payload);
	
	R output = SleeperFunction(payload);

	WrapperResponse output = new WrapperResponse{result = Json.SerializeObject(output)};
	
	return output
}