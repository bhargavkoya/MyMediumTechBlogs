using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwarePrinciplesInDotNET.RealWorldUsecases
{
    //Base class with virtual methods for proper substitution
    public class BankAccount
    {
        public string AccountNumber { get; set; }
        public decimal Balance { get; protected set; }

        public BankAccount(string accountNumber, decimal initialBalance)
        {
            AccountNumber = accountNumber;
            Balance = initialBalance;
        }

        public virtual void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");

            Balance += amount;
            Console.WriteLine($"Deposited {amount:C}. New balance: {Balance:C}");
        }

        public virtual bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");

            if (amount <= Balance)
            {
                Balance -= amount;
                Console.WriteLine($"Withdrew {amount:C}. New balance: {Balance:C}");
                return true;
            }

            Console.WriteLine("Insufficient funds");
            return false;
        }

        public virtual decimal GetAvailableBalance()
        {
            return Balance;
        }
    }

    //SavingsAccount extends behavior without changing base expectations
    public class SavingsAccount : BankAccount
    {
        public decimal InterestRate { get; set; }

        public SavingsAccount(string accountNumber, decimal initialBalance, decimal interestRate)
            : base(accountNumber, initialBalance)
        {
            InterestRate = interestRate;
        }

        public void ApplyInterest()
        {
            decimal interest = Balance * InterestRate;
            Deposit(interest);
            Console.WriteLine($"Applied interest: {interest:C}");
        }

        //Maintains base class behavior while adding restrictions
        public override bool Withdraw(decimal amount)
        {
            if (amount > 500) //Savings account daily limit
            {
                Console.WriteLine("Daily withdrawal limit exceeded for savings account");
                return false;
            }

            return base.Withdraw(amount);
        }
    }

    //CurrentAccount allows overdraft without breaking base behavior
    public class CurrentAccount : BankAccount
    {
        public decimal OverdraftLimit { get; set; }

        public CurrentAccount(string accountNumber, decimal initialBalance, decimal overdraftLimit)
            : base(accountNumber, initialBalance)
        {
            OverdraftLimit = overdraftLimit;
        }

        public override bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");

            if (amount <= Balance + OverdraftLimit)
            {
                Balance -= amount;
                Console.WriteLine($"Withdrew {amount:C}. New balance: {Balance:C}");
                return true;
            }

            Console.WriteLine("Exceeded overdraft limit");
            return false;
        }

        public override decimal GetAvailableBalance()
        {
            return Balance + OverdraftLimit;
        }
    }

    //LSP allows seamless substitution
    public class BankingService
    {
        public void ProcessAccounts(List<BankAccount> accounts)
        {
            foreach (BankAccount account in accounts)
            {
                //Works with any BankAccount subtype
                Console.WriteLine($"Account {account.AccountNumber}: Available Balance = {account.GetAvailableBalance():C}");

                account.Deposit(100);
                account.Withdraw(50);
            }
        }
    }
}
