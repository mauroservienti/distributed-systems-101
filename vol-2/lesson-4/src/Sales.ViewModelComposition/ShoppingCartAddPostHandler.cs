using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NServiceBus;
using Sales.ViewModelComposition.Events;
using Sales.ViewModelComposition.Messages;
using ServiceComposer.AspNetCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Sales.ViewModelComposition
{
    class ShoppingCartAddPostHandler : ICompositionRequestsHandler
    {
        IMessageSession messageSession;

        public ShoppingCartAddPostHandler(IMessageSession messageSession)
        {
            this.messageSession = messageSession;
        }

        [HttpPost("shoppingcart/add/{id}")]
        public async Task Handle(HttpRequest request)
        {
            if (request.Cookies["cart-id"] == null)
            {
                throw new ArgumentException("Missing cart-id cookie.");
            }
            
            var requestData = new Dictionary<string, string>()
            {
                { "sales-product-id", (string)request.HttpContext.GetRouteValue("id") },
                { "sales-quantity", request.Form["quantity"][0] },
            };
            var vm = request.GetComposedResponseModel();
            var ctx = request.GetCompositionContext();
            await vm.RaiseEvent(new AddItemToCartRequested()
            {
                CartId = request.Cookies["cart-id"],
                RequestId = ctx.RequestId,
                RequestData = requestData
            });

            await messageSession.SendLocal(new AddToCartRequest()
            {
                RequestId = ctx.RequestId,
                CartId = new Guid(request.Cookies["cart-id"]!),
                RequestData = requestData });
        }
    }
}
