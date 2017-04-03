namespace ServiceFabric.Logging
{
    public enum ServiceFabricEvent
    {
        Trace = 1000,
        ApiRequest = 1001,
        ServiceRequest = 1002,
        Exception = 1003,
        Metric = 1004,
        Dependency = 1005,
        ServiceListening = 1006,
        ServiceInitializationFailed = 1007,
    }
}


