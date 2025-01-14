namespace Blockbuster.Domain.Entities
{
    public class User
    {
        private readonly List<Rental> _rentals;

        protected User()
        {
            _rentals = new List<Rental>();
        }

        public User(string name, string email, string phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentNullException(nameof(phone));

            Name = name;
            Email = email;
            Phone = phone;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _rentals = new List<Rental>();
        }

        public int Id { get;  set; }
        public string Name { get;  set; }
        public string Email { get;  set; }
        public string Phone { get;  set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

        public void Update(string name, string email, string phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentNullException(nameof(phone));

            Name = name;
            Email = email;
            Phone = phone;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool HasActiveRentals(DateTime currentDate)
        {
            return _rentals.Any(r => r.EndDate > currentDate);
        }

        public void AddRental(Rental rental)
        {
            _rentals.Add(rental);
        }
    }
}
