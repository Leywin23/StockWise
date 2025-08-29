using StockWise.Dtos.ProductDtos;
using StockWise.Models;

namespace StockWise.Mappers
{
    public static class ProductMapper
    {
        public static Product ToProductFromCreate(this CreateProductDto productDto, Category category)
        {
            var shoppingPrice = Money.Of(productDto.ShoppingPrice, productDto.Currency.CurrencyCode);
            var sellingPrice = Money.Of(productDto.SellingPrice, productDto.Currency.CurrencyCode);

            return new Product
            {
                ProductName = productDto.ProductName,
                EAN = productDto.EAN,
                Image = productDto.Image,
                Description = productDto.Description,
                ShoppingPrice = shoppingPrice,
                SellingPrice = sellingPrice, 
                CategoryId = category.CategoryId,
                Category = category
            };
        }
    }
}
