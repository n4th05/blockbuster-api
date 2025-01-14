using Dapper;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using Blockbuster.Infrastructure.Data;

public class RentalRepository : IRentalRepository
{
    private readonly IDbConnectionFactory connectionFactory;

    public RentalRepository(IDbConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task<Rental> GetRentalAsync(int userId, int movieId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
           SELECT r.*, u.*, m.*
           FROM Rentals r
           JOIN Users u ON r.UserId = u.Id
           JOIN Movies m ON r.MovieId = m.Id
           WHERE r.UserId = @UserId AND r.MovieId = @MovieId";

        var rentals = await connection.QueryAsync<Rental, User, Movie, Rental>(
            sql,
            (rentalEntity, userEntity, movieEntity) =>
            {
                rentalEntity.User = userEntity;
                rentalEntity.Movie = movieEntity;
                return rentalEntity;
            },
            new { UserId = userId, MovieId = movieId },
            splitOn: "Id,Id");

        return rentals.FirstOrDefault();
    }

    public async Task<IEnumerable<Rental>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
           SELECT r.*, u.*, m.*
           FROM Rentals r
           JOIN Users u ON r.UserId = u.Id
           JOIN Movies m ON r.MovieId = m.Id";

        var rentals = await connection.QueryAsync<Rental, User, Movie, Rental>(
            sql,
            (rentalEntity, userEntity, movieEntity) =>
            {
                rentalEntity.User = userEntity;
                rentalEntity.Movie = movieEntity;
                return rentalEntity;
            },
            splitOn: "Id,Id");

        return rentals;
    }

    public async Task<IEnumerable<Rental>> GetActiveRentalsAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
           SELECT r.*, u.*, m.*
           FROM Rentals r
           JOIN Users u ON r.UserId = u.Id
           JOIN Movies m ON r.MovieId = m.Id
           WHERE r.StartDate <= @CurrentDate 
           AND r.EndDate >= @CurrentDate";

        var rentals = await connection.QueryAsync<Rental, User, Movie, Rental>(
            sql,
            (rentalEntity, userEntity, movieEntity) =>
            {
                rentalEntity.User = userEntity;
                rentalEntity.Movie = movieEntity;
                return rentalEntity;
            },
            new { CurrentDate = DateTime.UtcNow },
            splitOn: "Id,Id");

        return rentals;
    }

    public async Task<IEnumerable<Rental>> GetOverdueRentalsAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
           SELECT r.*, u.*, m.*
           FROM Rentals r
           JOIN Users u ON r.UserId = u.Id
           JOIN Movies m ON r.MovieId = m.Id
           WHERE r.EndDate < @CurrentDate";

        var rentals = await connection.QueryAsync<Rental, User, Movie, Rental>(
            sql,
            (rentalEntity, userEntity, movieEntity) =>
            {
                rentalEntity.User = userEntity;
                rentalEntity.Movie = movieEntity;
                return rentalEntity;
            },
            new { CurrentDate = DateTime.UtcNow },
            splitOn: "Id,Id");

        return rentals;
    }

    public async Task<IEnumerable<Rental>> GetUpcomingRentalsAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
           SELECT r.*, u.*, m.*
           FROM Rentals r
           JOIN Users u ON r.UserId = u.Id
           JOIN Movies m ON r.MovieId = m.Id
           WHERE r.StartDate > @CurrentDate
           ORDER BY r.StartDate";

        var rentals = await connection.QueryAsync<Rental, User, Movie, Rental>(
            sql,
            (rentalEntity, userEntity, movieEntity) =>
            {
                rentalEntity.User = userEntity;
                rentalEntity.Movie = movieEntity;
                return rentalEntity;
            },
            new { CurrentDate = DateTime.UtcNow },
            splitOn: "Id,Id");

        return rentals;
    }

    public async Task<Rental> AddAsync(Rental rental)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
           INSERT INTO Rentals (UserId, MovieId, StartDate, EndDate)
           VALUES (@UserId, @MovieId, @StartDate, @EndDate)
           RETURNING *";

        var createdRental = await connection.QuerySingleAsync<Rental>(sql, new
        {
            rental.UserId,
            rental.MovieId,
            rental.StartDate,
            rental.EndDate
        });

        return await GetRentalAsync(createdRental.UserId, createdRental.MovieId);
    }

    public async Task UpdateAsync(Rental rental)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
           UPDATE Rentals 
           SET StartDate = @StartDate,
               EndDate = @EndDate
           WHERE UserId = @UserId 
           AND MovieId = @MovieId";

        await connection.ExecuteAsync(sql, new
        {
            rental.UserId,
            rental.MovieId,
            rental.StartDate,
            rental.EndDate
        });
    }

    public async Task DeleteAsync(Rental rental)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
           DELETE FROM Rentals 
           WHERE UserId = @UserId 
           AND MovieId = @MovieId";

        await connection.ExecuteAsync(sql, new
        {
            rental.UserId,
            rental.MovieId
        });
    }
}