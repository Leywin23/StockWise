using FluentAssertions;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Tests.Unit.Models
{
    public class MoneyTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Ctor_ShouldThrow_WhenAmountIsNotPositive(decimal amount)
        {
            Action act = () => new Money(amount, new Currency("PLN"));
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Ctor_ShouldSetProperties_WhenValid()
        {
            var c = new Currency("USD");
            var m = new Money(10.5m, c);
            m.Amount.Should().Be(10.5m);
            m.Currency.Should().BeSameAs(c);
        }

        [Fact]
        public void Ctor_ShouldCreateFromCode()
        {
            var m = Money.Of(99.99m, "eur");

            m.Amount.Should().Be(99.99m);
            m.Currency.ToString().Should().Be("EUR");
        }

        [Fact]
        public void ToString_ShouldContainAmountAndCurrency()
        {
            var m = new Money(7m, new Currency("PLN"));
            m.ToString().Should().Be("7, PLN");
        }
    }
}
