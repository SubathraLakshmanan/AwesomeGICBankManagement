using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGICBankManagement
{
   public class BankAccount
    {
        public string AccountNumber { get; }
        public List<BankTransaction> BankTransactions { get; }

        public decimal Balance
        {
            get
            {
                decimal balance = 0;

                foreach (BankTransaction transaction in BankTransactions)
                {
                    balance += transaction.Type == "D" ? transaction.Amount : -transaction.Amount;
                }

                return balance;
            }
        }

        public BankAccount(string accountNumber)
        {
            AccountNumber = accountNumber;
            BankTransactions = new List<BankTransaction>();
        }
    }

}
