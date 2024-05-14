namespace CornerStore.Models.DTOs;

public class CreateOrderDTO
{
    public int CashierId { get; set;}

    public List<OrderProductDTO> OrderProducts {get; set;}
}