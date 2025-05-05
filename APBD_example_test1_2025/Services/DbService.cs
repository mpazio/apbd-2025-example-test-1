using System.Data.Common;
using APBD_example_test1_2025.Exceptions;
using APBD_example_test1_2025.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD_example_test1_2025.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    
    public async Task<CustomerRentalHistoryDto> GetRentalsForCustomerByIdAsync(int customerId)
    {
        var query =
            @"SELECT first_name, last_name, r.rental_id, rental_date, return_date, s.name, ri.price_at_rental, m.title
            FROM Rental r
            JOIN Customer c ON r.customer_id = c.customer_id
            JOIN Status s ON r.status_id = s.status_id
            JOIN Rental_Item ri ON ri.rental_id = r.rental_id
            JOIN Movie m ON m.movie_id = ri.movie_id
            WHERE r.customer_id = @customerId;";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@customerId", customerId);
        var reader = await command.ExecuteReaderAsync();
        
        CustomerRentalHistoryDto? rentals = null;
        
        while (await reader.ReadAsync())
        {
            if (rentals is null)
            {
                rentals = new CustomerRentalHistoryDto
                {
                    FirstName = reader.GetString(0),
                    LastName = reader.GetString(1),
                    Rentals = new List<RentalDetailsDto>()
                };
            }
            
            int rentalId = reader.GetInt32(2);
            
            var rental = rentals.Rentals.FirstOrDefault(e => e.Id.Equals(rentalId));
            if (rental is null)
            {
                rental = new RentalDetailsDto()
                {
                    Id = rentalId,
                    RentalDate = reader.GetDateTime(3),
                    ReturnDate = await reader.IsDBNullAsync(4) ? null : reader.GetDateTime(4),
                    Status = reader.GetString(5),
                    Movies = new List<RentedMovieDto>()
                };
                rentals.Rentals.Add(rental);
            }
            rental.Movies.Add(new RentedMovieDto()
            {
                Title = reader.GetString(7),
                PriceAtRental = reader.GetDecimal(6),
            });
            
        }       
        
        if (rentals is null)
        {
            throw new NotFoundException("No rentals found for the specified customer.");
        }
        
        return rentals;
    }

    public async Task AddNewRentalAsync(int customerId, CreateRentalRequestDto rentalRequest)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Customer WHERE customer_id = @IdCustomer;";
            command.Parameters.AddWithValue("@IdCustomer", customerId);
                
            var customerIdRes = await command.ExecuteScalarAsync();
            if(customerIdRes is null)
                throw new NotFoundException($"Customer with ID - {customerId} - not found.");
            
            command.Parameters.Clear();
            command.CommandText = 
                @"INSERT INTO Rental
            VALUES(@IdRental, @RentalDate, @ReturnDate, @CustomerId, @StatusId);";
        
            command.Parameters.AddWithValue("@IdRental", rentalRequest.Id);
            command.Parameters.AddWithValue("@RentalDate", rentalRequest.RentalDate);
            command.Parameters.AddWithValue("@ReturnDate", DBNull.Value);
            command.Parameters.AddWithValue("@CustomerId", customerId);
            command.Parameters.AddWithValue("@StatusId", 1);

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw new ConflictException("A rental with the same ID already exists.");
            }
            

            foreach (var movie in rentalRequest.Movies)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT movie_id FROM Movie WHERE Title = @MovieTitle;";
                command.Parameters.AddWithValue("@MovieTitle", movie.Title);
                
                var movieId = await command.ExecuteScalarAsync();
                if(movieId is null)
                    throw new NotFoundException($"Movie - {movie.Title} - not found.");
                
                command.Parameters.Clear();
                command.CommandText = 
                    @"INSERT INTO Rental_Item
                        VALUES(@IdRental, @MovieId, @RentalPrice);";
        
                command.Parameters.AddWithValue("@IdRental", rentalRequest.Id);
                command.Parameters.AddWithValue("@MovieId", movieId);
                command.Parameters.AddWithValue("@RentalPrice", movie.RentalPrice);
                
                await command.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
        

    }
}