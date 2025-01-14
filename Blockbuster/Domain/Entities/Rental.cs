namespace Blockbuster.Domain.Entities
{
    public class Rental
    {
        protected Rental() { }

        public Rental(int userId, int movieId, DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                throw new ArgumentException("End date must be after start date");

            UserId = userId;
            MovieId = movieId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public int UserId { get;  set; }
        public int MovieId { get;  set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        // Navigation properties
        public User User { get;  set; }
        public Movie Movie { get;  set; }

        public bool IsActive(DateTime currentDate)
        {
            return StartDate <= currentDate && EndDate >= currentDate;
        }

        public bool IsOverdue(DateTime currentDate)
        {
            return EndDate < currentDate;
        }

        public void UpdateDates(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                throw new ArgumentException("End date must be after start date");

            StartDate = startDate;
            EndDate = endDate;
        }

        public int GetRentalDurationInDays()
        {
            return (EndDate - StartDate).Days;
        }
    }
}