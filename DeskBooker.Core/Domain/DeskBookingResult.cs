using System;
using System.Collections.Generic;

namespace DeskBooker.Core.Domain
{
    public class DeskBookingResult : DeskBookingBase
    {
        public int? DeskBookingId { get; set; }
        public DeskBookingResultCode Code { get; set; }
       
    }
}