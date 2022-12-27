using System;
using System.Threading.Tasks;
using Finance.Events;
using NServiceBus;

namespace Finance
{
    public class OverdueInvoicePolicy :
        Saga<OverdueInvoiceData>,
        IAmStartedByMessages<InvoiceIssued>,
        IHandleMessages<InvoicePaid>,
        IHandleTimeouts<CheckPayment>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OverdueInvoiceData> mapper)
        {
            mapper.MapSaga(d => d.InvoiceNumber)
                .ToMessage<InvoiceIssued>(m => m.InvoiceNumber)
                .ToMessage<InvoicePaid>(m => m.InvoiceNumber);
        }

        public async Task Handle(InvoiceIssued message, IMessageHandlerContext context)
        {
            Console.WriteLine(
                $"OverdueInvoicePolicy - InvoiceIssued: {message.InvoiceNumber}, DueDate {message.DueDate}");

            Data.OrderId = message.OrderId;
            Data.InvoiceNumber = message.InvoiceNumber;
            var dueDate = message.DueDate;
            if (message.CustomerCountry == "Italy")
            {
                dueDate = dueDate.AddDays(20);
            }

            await RequestTimeout<CheckPayment>(context, dueDate);
            Console.WriteLine($"OverdueInvoicePolicy - CheckPayment scheduled for: {dueDate}");
        }

        public Task Handle(InvoicePaid message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Invoice {Data.InvoiceNumber} paid. Saga complete.");
            //Comment to look at data in the db during the demos
            MarkAsComplete();
            return Task.CompletedTask;
        }

        public async Task Timeout(CheckPayment state, IMessageHandlerContext context)
        {
            //If the timeout is received it means we never received the InvoicePaid message, by definition this invoice is overdue.
            Console.WriteLine(
                $"OverdueInvoicePolicy - Invoice {Data.InvoiceNumber} is overdue, publishing InvoiceOverdue event.");
            await context.Publish(new InvoiceOverdue() {InvoiceNumber = Data.InvoiceNumber});
            //Comment to look at data in the db during the demos
            MarkAsComplete();
        }
    }

    public class CheckPayment
    {
    }

    public class OverdueInvoiceData : ContainSagaData
    {
        public string OrderId { get; set; }
        public int InvoiceNumber { get; set; }
    }
}