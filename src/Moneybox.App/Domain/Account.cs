using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }


        public void UpdateBalanceAfterDebit(decimal amount)
        {
            Balance -= amount;
            Withdrawn += amount;
        }


        public void UpdateBalanceAfterCredit(decimal amount)
        {
            Balance += amount;
            PaidIn += amount;
        }
    }
}
