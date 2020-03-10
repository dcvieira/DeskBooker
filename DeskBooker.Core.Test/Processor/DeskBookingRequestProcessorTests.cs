using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequestProcessor _processor;
        private readonly DeskBookingRequest _request;
        private readonly List<Desk> _availableDesks;
        private readonly Mock<IDeskBookingRepository> _deskBookingRepositoryMock;
        private readonly Mock<IDeskRepository> _deskRepositoryMock;

        public DeskBookingRequestProcessorTests()
        {

            _request = new DeskBookingRequest
            {
                FirstName = "Diego",
                LastName = "Martins",
                Email = "dvieira@email.com",
                Date = new DateTime(2020, 1, 28)
            };

            _availableDesks = new List<Desk> { new Desk { Id = 7 } };
            _deskBookingRepositoryMock = new Mock<IDeskBookingRepository>();
            _deskRepositoryMock = new Mock<IDeskRepository>();
            _deskRepositoryMock.Setup(x => x.GetAvailableDesks(_request.Date))
                .Returns(_availableDesks);

            _processor = new DeskBookingRequestProcessor(_deskBookingRepositoryMock.Object, _deskRepositoryMock.Object);

        }

        [Fact]
        public void ShouldReturnDeskBookingResultWithRequestValues()
        {

            // Act
            DeskBookingResult result = _processor.BookDesk(_request);

            // Assert

            Assert.NotNull(result);
            Assert.Equal(
                (_request.FirstName, _request.LastName, _request.Email, _request.Date),
                (_request.FirstName, result.LastName, result.Email, result.Date));
        }

        [Fact]
        public void ShouldThrowExceptionIfRequestIsNull()
        {
            // Arrange
            DeskBookingRequest request = null;

            // Act
            Action action = () => _processor.BookDesk(request);

            // Assert 
            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        public void ShouldSaveDeskBooking()
        {
            DeskBooking savedDeskBooking = null;
            _deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                .Callback<DeskBooking>(deskBooking =>
                {
                    savedDeskBooking = deskBooking;
                });

            _processor.BookDesk(_request);

            _deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Once);

            Assert.NotNull(savedDeskBooking);
            Assert.Equal(_request.FirstName, savedDeskBooking.FirstName);
            Assert.Equal(_request.LastName, savedDeskBooking.LastName);
            Assert.Equal(_request.Email, savedDeskBooking.Email);
            Assert.Equal(_request.Date, savedDeskBooking.Date);
            Assert.Equal(_availableDesks.FirstOrDefault().Id, savedDeskBooking.DeskId);
        }

        [Fact]
        public void ShouldNotSaveDesBookingIfNoDeskIsAvailable()
        {
            _availableDesks.Clear();
            _processor.BookDesk(_request);

            _deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Never);

        }

        [Theory]
        [InlineData(DeskBookingResultCode.Success, true)]
        [InlineData(DeskBookingResultCode.NoDeskAvailable, false)]
        public void ShouldReturnExpectedResultCode(DeskBookingResultCode code, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                _availableDesks.Clear();
            }
            var result = _processor.BookDesk(_request);

            Assert.Equal(code, result.Code);

        }

        [Theory]
        [InlineData(5, true)]
        [InlineData(null, false)]
        public void ShouldReturnExpectedDeskBookingId(int? expectedDeskBookingId, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                _availableDesks.Clear();
            }
            else
            {
                _deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                    .Callback<DeskBooking>(deskBooking =>
                    {
                        deskBooking.Id = expectedDeskBookingId.Value;
                    });
                    
            }
            var result = _processor.BookDesk(_request);

            Assert.Equal(expectedDeskBookingId, result.DeskBookingId);

        }
    }
}
