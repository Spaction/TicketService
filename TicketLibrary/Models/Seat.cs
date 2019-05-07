using System;
using System.Collections.Generic;
using System.Text;
using TicketLibrary.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketLibrary.Models
{
    public class Seat:IComparable<Seat>
    {
        public Seat(int Id, Location loc)
        {
            Status = 0;
            SeatId = Id;
            Loc = loc;
        }
        public Seat()
        {
        }

        public Seat(int Id, int showId, int x, int y, decimal score)
        {
            SeatId = Id;
            ShowId = showId;
            Rating = score;
            Loc = new Location(x, y);
        }

        public Show Show { get; set; }

        [Column("Id")]
        public int SeatId { get; set; }
        public int ShowId { get; set; }

        public SeatStatus _status;

        [ConcurrencyCheck]
        public SeatStatus Status
        {
            get
            {
                switch (_status)
                {
                    case SeatStatus.Held:
                        return (HoldTime??DateTime.UtcNow.AddSeconds(-1)) < DateTime.UtcNow ? SeatStatus.Available : SeatStatus.Held;
                    default:
                        return _status;

                }
            }
            set
            {
                _status = value;
            }
        }

        public int LocationId { get; set; }

        public Location Loc { get; set; }

        public DateTime? HoldTime { get; set; }

        [MaxLength(255)]
        public string ReservedEmail { get; set; }

        public decimal Rating { get; set; }

        [NotMapped]
        public bool Available
        {
            get
            {
                return Status == SeatStatus.Available;
            }
        }

        public int CompareTo(Seat other)
        {
            var val = other.Rating-this.Rating;
            if (val == 0)
                return 0;
            else if (val < 0)
                return -1;
            else
                return 1;
        }

        public void HoldSeat(string CustomerEmail)
        {
            Status = SeatStatus.Held;
            HoldTime = DateTime.UtcNow.AddSeconds(15);
            ReservedEmail = CustomerEmail;
        }


    }
}
