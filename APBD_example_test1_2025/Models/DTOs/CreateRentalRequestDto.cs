namespace APBD_example_test1_2025.Models.DTOs;

public class CreateRentalRequestDto
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    public List<RentedMovieInputDto> Movies { get; set; } = new List<RentedMovieInputDto>();
}

public class RentedMovieInputDto
{
    public string Title { get; set; } = string.Empty;
    public decimal RentalPrice { get; set; }
}