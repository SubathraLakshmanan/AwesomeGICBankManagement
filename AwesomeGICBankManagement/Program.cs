using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Lifetime;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGICBankManagement
{
    public class Program
    {

        static Dictionary<string, BankAccount> accounts = new Dictionary<string, BankAccount>();
        static List<BankTransaction> transactions = new List<BankTransaction>();
        static List<BankInterestRule> interestRules = new List<BankInterestRule>();

        static void Main(string[] args)
        {
            // *** uncommend below lines for testing ***//

            //Transactions("20230505|AC001|D|100.00");
            //Transactions("20230601|AC001|D|150.00");
            //Transactions("20230626|AC001|W|20.00");
            //Transactions("20230626|AC001|W|100.00");

            //InterestRules("20230101|RULE01|1.95");
            //InterestRules("20230520|RULE02|1.90");
            //InterestRules("20230615|RULE03|2.20");

            //PrintTransaction("AC001|6");

            Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
            Console.WriteLine("[I]nput transactions");
            Console.WriteLine("[D]efine interest rules");
            Console.WriteLine("[P]rint statement");
            Console.WriteLine("[Q]uit");

            while (true)
            {
                Console.WriteLine(">");
                string input = Console.ReadLine().ToUpper();

                switch (input)
                {
                    case "I":
                        Transactions();
                        break;
                    case "D":
                        InterestRules();
                        break;
                    case "P":
                        PrintTransaction();
                        break;
                    case "Q":
                        Quit();
                        return;
                    default:
                        Console.WriteLine("Invalid input. Please try again.");
                        break;
                }
            }
        }

        #region Transactions
        static void Transactions(string input = null)
        {

            if (input == null)
            {
                Console.WriteLine("Please enter transaction details in <Date>|<Account>|<Type>|<Amount> format");
                Console.WriteLine("(or enter blank to go back to the main menu):");
                Console.WriteLine(">");
                input = Console.ReadLine();
            }

            if (string.IsNullOrWhiteSpace(input))
                return;

            string[] transactionDetails = input.Split('|');

            if (transactionDetails.Length != 4)
            {
                Console.WriteLine("Invalid transaction format. Please try again.");
                return;
            }

            string dateStr = transactionDetails[0];
            string accountNumber = transactionDetails[1];
            string typeStr = transactionDetails[2].ToUpper();
            decimal amount;

            if (!DateTime.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                Console.WriteLine("Invalid date format. Please try again.");
                return;
            }

            if (!decimal.TryParse(transactionDetails[3], out amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount. Please try again.");
                return;
            }

            if (!accounts.ContainsKey(accountNumber))
            {
                if (typeStr == "W")
                {
                    Console.WriteLine("First transaction for an account cannot be a withdrawal. Please try again.");
                    return;
                }

                accounts[accountNumber] = new BankAccount(accountNumber);
            }

            BankAccount account = accounts[accountNumber];

            if (typeStr == "W" && account.Balance - amount < 0)
            {
                Console.WriteLine("Withdrawal amount exceeds account balance. Please try again.");
                return;
            }

            string transactionId = $"{date:yyyyMMdd}-{account.BankTransactions.Count + 1:00}";
            BankTransaction transaction = new BankTransaction(date, transactionId, typeStr, amount);
            account.BankTransactions.Add(transaction);
            transactions.Add(transaction);

            Console.WriteLine("Transaction added successfully.");

            PrintTransactionStatement(account);
        }

        #endregion Transactions

        #region InterestRules
        static void InterestRules(string input = null)
        {

            if (input == null)
            {
                Console.WriteLine("Please enter interest rules details in <Date>|<RuleId>|<Rate in %> format");
                Console.WriteLine("(or enter blank to go back to the main menu):");
                Console.WriteLine(">");
                input = Console.ReadLine();
            }


            if (string.IsNullOrWhiteSpace(input))
                return;

            string[] ruleDetails = input.Split('|');

            if (ruleDetails.Length != 3)
            {
                Console.WriteLine("Invalid interest rule format. Please try again.");
                return;
            }

            string dateStr = ruleDetails[0];
            string ruleId = ruleDetails[1];
            decimal rate;

            if (!DateTime.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                Console.WriteLine("Invalid date format. Please try again.");
                return;
            }

            if (!decimal.TryParse(ruleDetails[2], out rate) || rate <= 0 || rate >= 100)
            {
                Console.WriteLine("Invalid interest rate. Please try again.");
                return;
            }

            BankInterestRule rule = new BankInterestRule(date, ruleId, rate);
            interestRules.RemoveAll(r => r.Date == date);
            interestRules.Add(rule);
            interestRules.Sort((r1, r2) => r1.Date.CompareTo(r2.Date));

            Console.WriteLine("Interest rule added successfully.");

            PrintInterestRules();
        }
        static void PrintInterestRules()
        {
            Console.WriteLine("Interest rules:");
            Console.WriteLine("Date     | RuleId | Rate (%) |");

            foreach (BankInterestRule rule in interestRules)
            {
                Console.WriteLine($"{rule.Date:yyyyMMdd} | {rule.RuleId} | {rule.Rate,8:0.00} |");
            }

            Console.WriteLine();
        }

        #endregion InterestRules

        #region PrintTransaction 


        static void PrintTransaction(string input=null)
        {
            if (input == null)
            {
                Console.WriteLine("Please enter account and month to generate the statement <Account>|<Month>");
                Console.WriteLine("(or enter blank to go back to the main menu):");
                Console.WriteLine(">");
                input = Console.ReadLine();
            } 

            if (string.IsNullOrWhiteSpace(input))
                return;

            string[] statementDetails = input.Split('|');

            if (statementDetails.Length != 2)
            {
                Console.WriteLine("Invalid statement format. Please try again.");
                return;
            }

            string accountNumber = statementDetails[0];
            string monthStr = statementDetails[1];

            if (!accounts.ContainsKey(accountNumber))
            {
                Console.WriteLine("Account not found. Please try again.");
                return;
            }

            if (!int.TryParse(monthStr, out int month) || month < 1 || month > 12)
            {
                Console.WriteLine("Invalid month. Please try again.");
                return;
            }

            BankAccount account = accounts[accountNumber];
            List<BankTransaction> accountTransactions = account.BankTransactions;

            Console.WriteLine($"Account: {account.AccountNumber}");
            Console.WriteLine("Date     | Txn Id      | Type | Amount | Balance |");

            decimal balance = 0;
            decimal Previousbalance = (from tot in account.BankTransactions where tot.Date.Month< month
                                    select tot).Sum(e => e.Amount);
            foreach (BankTransaction transaction in accountTransactions)
            {
                if (balance == 0)
                { 
                    balance = Previousbalance;
                }
                if (transaction.Date.Month == month)
                {
                    balance += transaction.Type == "D" ? transaction.Amount : -transaction.Amount; 
                    Console.WriteLine($"{transaction.Date:yyyyMMdd} | {transaction.TransactionId} | {transaction.Type}    | {transaction.Amount,6:0.00} | {balance,7:0.00} |");
                }
            }

            decimal interest = CalculateInterest(account, month);
            balance += interest;
            DateTime startDate = new DateTime(DateTime.Today.Year, month, 1);
            Console.WriteLine($"{startDate.AddMonths(1).AddDays(-1):yyyyMMdd} |             | I    | {interest,6:0.00} | {balance,7:0.00} |");

        }

        static void PrintTransactionStatement(BankAccount account)
        {
            Console.WriteLine($"Account: {account.AccountNumber}");
            Console.WriteLine("Date     | Txn Id      | Type | Amount |");

            foreach (BankTransaction transaction in account.BankTransactions)
            {
                Console.WriteLine($"{transaction.Date:yyyyMMdd} | {transaction.TransactionId} | {transaction.Type}    | {transaction.Amount,4:0.00} |");
            }

            Console.WriteLine();
        }

        static decimal CalculateInterest(BankAccount account, int month)
        {
            decimal interest = 0;
            decimal balance = 0;
            decimal Previousbalance = 0;

            DateTime startDate = new DateTime(DateTime.Today.Year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            int daysInPeriod = DateTime.DaysInMonth(DateTime.Today.Year, month);

            Previousbalance = (from tot in account.BankTransactions
                                    where tot.Date.Month < month
                                    select tot).Sum(e => e.Amount);
 
            while (startDate <= endDate)
            {
                var ApplicableRule = interestRules.FirstOrDefault(x => x.Date <= startDate && x.Date.Month == month);

                if (ApplicableRule == null)
                    ApplicableRule = interestRules.LastOrDefault(x => x.Date <= startDate && x.Date.Month < month);

                if (ApplicableRule == null)
                    continue;

                var accountTransactions = account.BankTransactions.Where(x => x.Date <= startDate && x.Date.Month == month).ToList();
                balance = Previousbalance;
                foreach (var transaction in accountTransactions)
                {
                    balance += transaction.Type == "D" ? transaction.Amount : -transaction.Amount;
                }

                interest += balance * (ApplicableRule.Rate / 100);
                //Console.WriteLine($"{balance} | {interest} | {ApplicableRule.Rate,4:0.00} |");  
                startDate = startDate.AddDays(1);
            }

            return interest / 365;
        }

        #endregion PrintTransaction

        #region Quit 
        static void Quit()
        {
            Console.WriteLine("Thank you for banking with AwesomeGIC Bank.");
            Console.WriteLine("Have a nice day!");
        }

        #endregion Quit 


    }
}
