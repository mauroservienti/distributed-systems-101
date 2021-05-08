using System;

namespace Sales.Events
{
    public class OrderPlaced
    {
        // with a bit of work this can be an interface with a get-only property
        // https://github.com/mauroservienti/immutable-message-samples
        // C# being statically typed doesn't help in this case.
        public string OrderId { get; set; }
    }
}