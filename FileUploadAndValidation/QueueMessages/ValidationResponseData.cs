using System;
using System.Collections.Generic;
using System.Text;

namespace Qb.BillPaymentTransaction.Shared.Contracts
{
    public class ValidationResponseData
    {
        public string RequestId { get; set; }
        public string ResultLocation { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
