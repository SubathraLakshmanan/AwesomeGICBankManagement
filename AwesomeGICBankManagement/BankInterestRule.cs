using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGICBankManagement
{
    class BankInterestRule
    {
        public DateTime Date { get; }
        public string RuleId { get; }
        public decimal Rate { get; }

        public BankInterestRule(DateTime date, string ruleId, decimal rate)
        {
            Date = date;
            RuleId = ruleId;
            Rate = rate;
        }
    }
}
