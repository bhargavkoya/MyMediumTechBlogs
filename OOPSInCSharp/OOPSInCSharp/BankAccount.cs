
namespace OOPSInCSharp
{
    //encapsulation
    public class BankAccount
    {
        private decimal _balance;
        private readonly string _accountNumber;
        private readonly List<Transaction> _transactions;

        public BankAccount(string accountNumber, decimal initialDeposit = 0)
        {
            _accountNumber = accountNumber;
            _balance = initialDeposit;
            _transactions = new List<Transaction>();
        }

        public decimal Balance => _balance;
        public string AccountNumber => _accountNumber;

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");

            _balance += amount;
            _transactions.Add(new Transaction("Deposit", amount, DateTime.Now));
            Console.WriteLine($"Deposited ${amount}. New balance: ${_balance}");
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");

            if (_balance < amount)
            {
                Console.WriteLine("Insufficient funds");
                return false;
            }

            _balance -= amount;
            _transactions.Add(new Transaction("Withdrawal", amount, DateTime.Now));
            Console.WriteLine($"Withdrew ${amount}. New balance: ${_balance}");
            return true;
        }

        public IReadOnlyList<Transaction> GetTransactionHistory()
        {
            return _transactions.AsReadOnly();
        }
    }

    public record Transaction(string Type, decimal Amount, DateTime Date);
}
