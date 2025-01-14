using Dapper;
using System.Data;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using Blockbuster.Infrastructure.Data;

namespace Blockbuster.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MovieRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Movie> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT m.*, r.*
                FROM Movies m
                LEFT JOIN Rentals r ON m.Id = r.MovieId
                WHERE m.Id = @Id";

            var movieDict = new Dictionary<int, Movie>();

            await connection.QueryAsync<Movie, Rental, Movie>(
                sql,
                (movie, rental) =>
                {
                    if (!movieDict.TryGetValue(movie.Id, out var movieEntry))
                    {
                        movieEntry = movie;
                        movieDict.Add(movie.Id, movieEntry);
                    }

                    if (rental != null)
                    {
                        movieEntry.AddRental(rental);
                    }

                    return movieEntry;
                },
                new { Id = id },
                splitOn: "UserId");

            return movieDict.Values.FirstOrDefault();
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT m.*, r.*
                FROM Movies m
                LEFT JOIN Rentals r ON m.Id = r.MovieId";

            var movieDict = new Dictionary<int, Movie>();

            await connection.QueryAsync<Movie, Rental, Movie>(
                sql,
                (movie, rental) =>
                {
                    if (!movieDict.TryGetValue(movie.Id, out var movieEntry))
                    {
                        movieEntry = movie;
                        movieDict.Add(movie.Id, movieEntry);
                    }

                    if (rental != null)
                    {
                        movieEntry.AddRental(rental);
                    }

                    return movieEntry;
                },
                splitOn: "UserId");

            return movieDict.Values;
        }

        public async Task<IEnumerable<Movie>> GetAvailableAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT DISTINCT m.*
                FROM Movies m
                WHERE m.DeletedAt IS NULL
                AND NOT EXISTS (
                    SELECT 1 FROM Rentals r
                    WHERE r.MovieId = m.Id
                    AND r.StartDate <= @EndDate
                    AND r.EndDate >= @StartDate
                )";

            return await connection.QueryAsync<Movie>(sql, new { StartDate = startDate, EndDate = endDate });
        }

        public async Task<IEnumerable<Movie>> GetTrendingAsync(int days)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT m.*, COUNT(r.MovieId) as RentalCount
                FROM Movies m
                LEFT JOIN Rentals r ON m.Id = r.MovieId 
                    AND r.StartDate >= @StartDate
                WHERE m.DeletedAt IS NULL
                GROUP BY m.Id
                ORDER BY RentalCount DESC
                LIMIT 10";

            var startDate = DateTime.UtcNow.AddDays(-days);
            return await connection.QueryAsync<Movie>(sql, new { StartDate = startDate });
        }

        public async Task<Movie> AddAsync(Movie movie)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO Movies (Title, Description, Value, CreatedAt, UpdatedAt, DeletedAt)
                VALUES (@Title, @Description, @Value, @CreatedAt, @UpdatedAt, @DeletedAt)
                RETURNING Id";

            movie.Id = await connection.QuerySingleAsync<int>(sql, movie);
            return movie;
        }

        public async Task UpdateAsync(Movie movie)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Movies 
                SET Title = @Title,
                    Description = @Description,
                    Value = @Value,
                    UpdatedAt = @UpdatedAt,
                    DeletedAt = @DeletedAt
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, movie);
        }

        public async Task DeleteAsync(Movie movie)
        {
            // Soft delete
            movie.Delete();
            await UpdateAsync(movie);
        }
    }
}