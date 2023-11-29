namespace AllUpHomeWork.Areas.Admin.Models
{
    public class ProductImageModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public ProductModel Product { get; set; }

        public string? ImagePath { get; set; }
    }
}
