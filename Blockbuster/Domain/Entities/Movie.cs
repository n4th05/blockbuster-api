namespace Blockbuster.Domain.Entities
{
    public class Movie
    {
        private readonly List<Rental> _rentals;

        protected Movie()
        {
            _rentals = new List<Rental>();
        }

        public Movie(string title, string description, decimal value)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (value <= 0)
                throw new ArgumentException("Value must be greater than zero", nameof(value));

            Title = title;
            Description = description;
            Value = value;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _rentals = new List<Rental>();
        }

        public int Id { get;  set; }
        public string Title { get;  set; }
        public string Description { get;  set; }
        public decimal Value { get;  set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? DeletedAt { get;  set; }

        // Change from virtual to IReadOnlyCollection to enforce encapsulation
        public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

        public void Update(string title, string description, decimal value)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (value <= 0)
                throw new ArgumentException("Value must be greater than zero", nameof(value));
            if (DeletedAt.HasValue)
                throw new InvalidOperationException("Cannot update a deleted movie");

            Title = title;
            Description = description;
            Value = value;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete()
        {
            if (DeletedAt.HasValue)
                throw new InvalidOperationException("Movie is already deleted");

            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsAvailable(DateTime startDate, DateTime endDate)
        {
            if (DeletedAt.HasValue)
                return false;

            return !_rentals.Any(r =>
                r.StartDate <= endDate && r.EndDate >= startDate);
        }

        public void AddRental(Rental rental)
        {
            if (DeletedAt.HasValue)
                throw new InvalidOperationException("Cannot add rental to a deleted movie");
            if (!IsAvailable(rental.StartDate, rental.EndDate))
                throw new InvalidOperationException("Movie is not available for the specified period");

            _rentals.Add(rental);
        }
    }
}