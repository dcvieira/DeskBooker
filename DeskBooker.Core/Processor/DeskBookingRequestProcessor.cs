using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using System;
using System.Linq;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessor
    {
        private IDeskBookingRepository _deskBookingRepository;
        private IDeskRepository _deskRepository;

        public DeskBookingRequestProcessor(IDeskBookingRepository deskBookingRepository, IDeskRepository deskRepository)
        {
            _deskBookingRepository = deskBookingRepository;
            _deskRepository = deskRepository;
        }

        public DeskBookingResult BookDesk(DeskBookingRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var result = Create<DeskBookingResult>(request);

            var availableDesks = _deskRepository.GetAvailableDesks(request.Date);

            if(availableDesks.FirstOrDefault() is Desk availableDesk)
            {
                var deskBooking = Create<DeskBooking>(request);
                deskBooking.DeskId = availableDesk.Id;

                _deskBookingRepository.Save(deskBooking);

                result.Code = DeskBookingResultCode.Success;
                result.DeskBookingId = deskBooking.Id;
                return result;
            }
            else
            {
                result.Code = DeskBookingResultCode.NoDeskAvailable;
                return result;
            }

        }


        private T Create<T>(DeskBookingBase deskBooking) where T : DeskBookingBase, new()
        {
            return new T()
            {
                FirstName = deskBooking.FirstName,
                LastName = deskBooking.LastName,
                Email = deskBooking.Email,
                Date = deskBooking.Date
            };
        }

    }
}