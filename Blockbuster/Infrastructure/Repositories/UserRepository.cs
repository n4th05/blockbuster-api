using Dapper;
using System.Data;
using Blockbuster.Domain.Entities;
using Blockbuster.Domain.Interfaces;
using Blockbuster.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User> AddAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Users (Name, Email, Phone, CreatedAt, UpdatedAt)
            VALUES (@Name, @Email, @Phone, @CreatedAt, @UpdatedAt)
            RETURNING Id";

        user.Id = await connection.QuerySingleAsync<int>(sql, new
        {
            user.Name,
            user.Email,
            user.Phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return user;
    }

    public async Task DeleteAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"DELETE FROM Users WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { user.Id });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT u.*, r.*
            FROM Users u
            LEFT JOIN Rentals r ON u.Id = r.UserId";

        var userDict = new Dictionary<int, User>();

        await connection.QueryAsync<User, Rental, User>(
            sql,
            (user, rental) =>
            {
                if (!userDict.TryGetValue(user.Id, out var userEntry))
                {
                    userEntry = user;
                    userDict.Add(user.Id, userEntry);
                }

                if (rental != null)
                {
                    userEntry.AddRental(rental);
                }

                return userEntry;
            },
            splitOn: "UserId");

        return userDict.Values;
    }

    public async Task<User> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT u.*, r.*
            FROM Users u
            LEFT JOIN Rentals r ON u.Id = r.UserId
            WHERE u.Id = @Id";

        var userDict = new Dictionary<int, User>();

        await connection.QueryAsync<User, Rental, User>(
            sql,
            (user, rental) =>
            {
                if (!userDict.TryGetValue(user.Id, out var userEntry))
                {
                    userEntry = user;
                    userDict.Add(user.Id, userEntry);
                }

                if (rental != null)
                {
                    userEntry.AddRental(rental);
                }

                return userEntry;
            },
            new { Id = id },
            splitOn: "UserId");

        return userDict.Values.FirstOrDefault();
    }

    public async Task<IEnumerable<User>> GetUsersWhoRentedMovieAsync(int movieId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT DISTINCT u.*, r.*
            FROM Users u
            INNER JOIN Rentals r ON u.Id = r.UserId
            WHERE r.MovieId = @MovieId";

        var userDict = new Dictionary<int, User>();

        await connection.QueryAsync<User, Rental, User>(
            sql,
            (user, rental) =>
            {
                if (!userDict.TryGetValue(user.Id, out var userEntry))
                {
                    userEntry = user;
                    userDict.Add(user.Id, userEntry);
                }

                if (rental != null)
                {
                    userEntry.AddRental(rental);
                }

                return userEntry;
            },
            new { MovieId = movieId },
            splitOn: "UserId");

        return userDict.Values;
    }

    public async Task<IEnumerable<User>> GetUsersWithActiveRentalsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT DISTINCT u.*, r.*
            FROM Users u
            INNER JOIN Rentals r ON u.Id = r.UserId
            WHERE r.EndDate > @CurrentDate";

        var userDict = new Dictionary<int, User>();

        await connection.QueryAsync<User, Rental, User>(
            sql,
            (user, rental) =>
            {
                if (!userDict.TryGetValue(user.Id, out var userEntry))
                {
                    userEntry = user;
                    userDict.Add(user.Id, userEntry);
                }

                if (rental != null)
                {
                    userEntry.AddRental(rental);
                }

                return userEntry;
            },
            new { CurrentDate = DateTime.UtcNow },
            splitOn: "UserId");

        return userDict.Values;
    }

    public async Task UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Users 
            SET Name = @Name,
                Email = @Email,
                Phone = @Phone,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        await connection.ExecuteAsync(sql, new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            UpdatedAt = DateTime.UtcNow
        });
    }
}