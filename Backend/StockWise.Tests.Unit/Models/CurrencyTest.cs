using FluentAssertions;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Tests.Unit.Models
{
    public class CurrencyTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Ctor_ShouldThrow_WhenEmpty(string code)
        {
            Action act = () => new Currency(code);
            act.Should().Throw<ArgumentNullException>();
        }
        [Theory]
        [InlineData("PL")]
        [InlineData("EURO")]
        [InlineData("12Z")]
        [InlineData("pl1")]
        public void Ctor_ShouldThrow_WhenNotThreeLetters(string code)
        {
            Action act = () => new Currency(code);
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("pln","PLN")]
        [InlineData("Usd","USD")]
        [InlineData(" eur","EUR")]
        public void Ctor_ShouldNormalizeCode(string input, string expected)
        {
            var c = new Currency(input);
            c.Code.Should().Be(expected);
        }

        [Fact]
        public void ToString_ShouldReturnCode()
        {
            new Currency("PLN").ToString().Should().Be("PLN");
        }
    }
}
