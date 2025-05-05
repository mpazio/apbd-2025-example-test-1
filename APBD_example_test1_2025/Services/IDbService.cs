using APBD_example_test1_2025.Models.DTOs;

namespace APBD_example_test1_2025.Services;

public interface IDbService
{
    Task<CustomerRentalHistoryDto> GetRentalsForCustomerByIdAsync(int customerId);
    Task AddNewRentalAsync(int customerId, CreateRentalRequestDto rentalRequest);
}