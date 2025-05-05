namespace APBD_example_test1_2025.Models.DTOs;

public class CustomerRentalHistoryDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<RentalDetailsDto> Rentals { get; set; } = [];
}

public class RentalDetailsDto
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<RentedMovieDto> Movies { get; set; } = [];
}

public class RentedMovieDto
{
    public string Title { get; set; } = string.Empty;
    public decimal PriceAtRental { get; set; }
}