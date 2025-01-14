namespace Blockbuster.Application.DTOs
{
    public class RentalDTO
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public string UserName { get; set; }
        public string MovieTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class CreateRentalDTO
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class UpdateRentalDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class RentalSearchDTO
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserName { get; set; }
        public string MovieTitle { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsOverdue { get; set; }
    }

    public class RentalStatisticsDTO
    {
        public int TotalActiveRentals { get; set; }
        public int OverdueRentals { get; set; }
        public int RentalsLast30Days { get; set; }
        public decimal AverageRentalDuration { get; set; }
        public IEnumerable<TopUserDTO> MostActiveUsers { get; set; }
        public IEnumerable<TopMovieDTO> MostRentedMovies { get; set; }
    }

    public class TopUserDTO
    {
        public string Name { get; set; }
        public int RentalCount { get; set; }
    }

    public class TopMovieDTO
    {
        public string Title { get; set; }
        public int RentalCount { get; set; }
    }
}