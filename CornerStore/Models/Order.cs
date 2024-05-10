
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int CashierId { get; set; }

    [ForeignKey("CashierId")]
    public Cashier Cashier { get; set; }

    // Not Mapped means it won't be added to SQL db
    [NotMapped]
    public decimal Total
    {
        get
        {
            decimal total = 0;
            foreach (var orderProduct in OrderProducts)
            {
                total += orderProduct.Product.Price * orderProduct.Quantity;
            }
            return total;
        }
    }

    public DateTime? PaidOnDate { get; set; }

    public List<OrderProduct> OrderProducts { get; set; }
}
    
    