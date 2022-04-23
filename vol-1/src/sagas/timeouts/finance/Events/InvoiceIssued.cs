using System;

namespace Finance.Events
{
    public class InvoiceIssued
    {
        public long InvoiceNumber { get; set; }
        public DateTime DueDate { get; set; }
        public string CustomerCountry { get; set; }
        public string OrderId { get; set; }
    }
}