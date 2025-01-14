namespace Blockbuster.Application.DTOs
{
    public class MovieDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateMovieDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
    }

    public class UpdateMovieDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
    }

    public class MovieSearchDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public bool? IsAvailable { get; set; }
    }

    public class MovieStatisticsDTO
    {
        public int TotalMovies { get; set; }
        public int AvailableMovies { get; set; }
        public int RentedMovies { get; set; }
        public decimal AverageRentalDuration { get; set; }
        public IEnumerable<MovieRentalStatDTO> TopRentedMovies { get; set; }
    }

    public class MovieRentalStatDTO
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public int RentalCount { get; set; }
    }
}