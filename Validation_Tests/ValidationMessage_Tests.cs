using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations;
using Xunit;

namespace Validations_Tests
{
    public class ValidationMessage_Tests
    {
        [Fact]
        public void SettingMessageAndHavingAChid_StoresThem()
        {
            // Arrange
            var msg = new ValidationMessage("FakeParentValidator", "Message", new List<ValidationMessage>()
            {
                new ValidationMessage("FakeChildValidator", "Child Message")
            });

            //Act
            //Assert3
            msg.Message.Should().Be("Message");
            msg.Children.Count.Should().Be(1);
        }
    }
}
