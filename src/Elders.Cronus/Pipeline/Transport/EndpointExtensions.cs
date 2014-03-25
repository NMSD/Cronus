using System;
using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus.Pipeline.Transport
{
    public static class EndpointExtensions
    {
        public static EndpointMessage BlockDequeue(this IEndpoint endpoint, IBatchScope scope)
        {
            EndpointMessage message = null;
            if (scope.Size == 1)
                message = endpoint.BlockDequeue();
            else if (scope.Size > 1)
                endpoint.BlockDequeue(30, out message);
            else
                throw new InvalidOperationException("The batch scope size cannot be smaller than 1");
            return message;
        }
    }
}