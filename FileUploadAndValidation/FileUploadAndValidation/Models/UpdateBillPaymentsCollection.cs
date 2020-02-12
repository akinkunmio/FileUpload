using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class UpdateBillPaymentsCollection
    {
        public string BatchId { get; set; }

        public string UserName { get; set; }

        public string Status { get; set; }

        public int NumOfValidRecords { get; set; }

        public string ModifiedDate { get; set; }

        public string NasValidatedFileName { get; set; }

        public string NasAuthorizedFileName { get; set; }

        public string NasConfirmedFileName { get; set; }

        public List<BillPayment> BillPayments { get; set; }
    }
}
