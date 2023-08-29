using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGICBankManagement
{
    public class BankTransaction
    {
        public DateTime Date { get; }
        public string TransactionId { get; }
        public string Type { get; }
        public decimal Amount { get; }
        public decimal Balance { get; }

        public BankTransaction(DateTime date, string transactionId, string type, decimal amount)
        {
            Date = date;
            TransactionId = transactionId;
            Type = type;
            Balance += type == "D" ? amount : -amount;
            Amount = amount;
        }
    }

}
