using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPSInCSharp
{
    public abstract class BankAccountInheritance
    {
        protected decimal _balance;
        protected readonly string _accountNumber;
        protected readonly List<Transaction> _transactions;

        protected BankAccountInheritance(string accountNumber, decimal initialDeposit = 0)
        {
            _accountNumber = accountNumber;
            _balance = initialDeposit;
            _transactions = new List<Transaction>();
        }

        public decimal Balance => _balance;
        public string AccountNumber => _accountNumber;

        public virtual void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");

            _balance += amount;
            _transactions.Add(new Transaction("Deposit", amount, DateTime.Now));
        }

        public abstract bool Withdraw(decimal amount);

        public IReadOnlyList<Transaction> GetTransactionHistory()
            => _transactions.AsReadOnly();
    }

    public class SavingsAccount : BankAccountInheritance
    {
        private readonly decimal _interestRate;

        public SavingsAccount(string accountNumber, decimal interestRate = 0.02m)
            : base(accountNumber)
        {
            _interestRate = interestRate;
        }

        public override bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");

            if (_balance < amount)
                return false;

            _balance -= amount;
            _transactions.Add(new Transaction("Withdrawal", amount, DateTime.Now));
            return true;
        }

        public void AddInterest()
        {
            var interest = _balance * _interestRate;
            _balance += interest;
            _transactions.Add(new Transaction("Interest", interest, DateTime.Now));
        }
    }

    public class CheckingAccount : BankAccountInheritance
    {
        private readonly decimal _overdraftLimit;

        public CheckingAccount(string accountNumber, decimal overdraftLimit = 500m)
            : base(accountNumber)
        {
            _overdraftLimit = overdraftLimit;
        }

        public override bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");

            if (_balance + _overdraftLimit < amount)
                return false;

            _balance -= amount;
            _transactions.Add(new Transaction("Withdrawal", amount, DateTime.Now));
            return true;
        }

        public decimal AvailableFunds => _balance + _overdraftLimit;
    }
}
