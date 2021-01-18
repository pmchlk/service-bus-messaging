using System;

namespace Messaging.Models
{
    public class Subscription
    {
        public Type HandlerType { get; private set; }

        public Subscription(Type handlerType)
        {
            HandlerType = handlerType;
        }
    }
}