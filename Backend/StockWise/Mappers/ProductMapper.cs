using StockWise.Dtos;
using StockWise.Models;

namespace StockWise.Mappers
{
    public static class ProductMapper
    {
        public static Product ToProductFromCreate(this CreateProductDto productDto, Category category)
        {
            return new Product
            {
                ProductName = productDto.ProductName,
                EAN = productDto.EAN,
                Image = productDto.Image,
                Description = productDto.Description,
                ShoppingPrice = productDto.ShoppingPrice,
                SellingPrice = productDto.SellingPrice,
                CategoryId = category.CategoryId
            };
        }
    }
}
