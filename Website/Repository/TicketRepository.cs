using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketLibrary.Interfaces;
using TicketLibrary.Models;
using TicketLibrary.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebAPI.Repository
{
    public class TicketRepository : TicketService
    {

        private readonly TicketContext _context;
        private readonly ILogger logger;
        public TicketRepository(TicketContext context,ILogger logger)
        {
            _context = context;
            this.logger = logger;
        }

        /// <summary>
        /// Procedure which will find the best rated seats and hold them.
        /// </summary>
        /// <param name="showId"></param>
        /// <param name="numSeats"></param>
        /// <param name="customerEmail"></param>
        /// <returns>Collection of Seats</returns>
        /// <exception cref="IndexOutOfRangeException">Returns when there are not enough seats available to be held.</exception>
        public async Task<ICollection<Seat>> FindAndHoldSeats(int showId, int numSeats, string customerEmail)
        {
            var seats = await GetTopXSeatsByRating(numSeats, showId);
            if (seats.Count < numSeats)
                if(seats.Count == 1)
                    throw new IndexOutOfRangeException("There is only 1 seat still available");
                else
                    throw new IndexOutOfRangeException(string.Format("There are only {0} seats still available",seats.Count));

            seats.ForEach(x => x.HoldSeat(customerEmail));

            var saved = false;
            while(!saved)
            {
                //Continue finding new seats where 
                if (await TrySave(seats) == 0)
                    saved = true;
                else
                {
                    seats = await GetTopXSeatsByRating(numSeats, showId);
                    seats.ForEach(x => x.HoldSeat(customerEmail));
                }
            }
            return seats;
        }

        /// <summary>
        /// Returns the top X Seats ordered by rating
        /// </summary>
        /// <param name="num"></param>
        /// <param name="showId"></param>
        /// <returns>List of seats</returns>
        private async Task<List<Seat>> GetTopXSeatsByRating(int num, int showId)
        {
            return await GetAvailableSeats(showId).OrderByDescending(x => x.Rating)
                                            .Take(num).ToListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="showId"></param>
        /// <returns>IQueryable<Seat> preselected to only available seats for a show</Seat></returns>
        private IQueryable<Seat> GetAvailableSeats(int showId)
        {
            return _context.Seats.Where(x => x.ShowId == showId)
                                 .Where(x => x.Status == TicketLibrary.Enums.SeatStatus.Available || (x.Status == TicketLibrary.Enums.SeatStatus.Held && (x.HoldTime ?? DateTime.UtcNow.AddSeconds(-1)) < DateTime.UtcNow));
        }

        /// <summary>
        /// Hold a list of seats by their ID
        /// </summary>
        /// <param name="showId"></param>
        /// <param name="seatIds"></param>
        /// <param name="customerEmail"></param>
        /// <returns>Collection of seats which were recently held for the email provided</returns>
        public async Task<ICollection<Seat>> FindAndHoldSeats(int showId, List<int> seatIds, string customerEmail)
        {
            var seats = await GetAvailableSeats(showId).Where(x => seatIds.Contains(x.SeatId)).ToListAsync();

            if(seats.Count() != seatIds.Distinct().Count())
            {
                var missing = seatIds.Except(seats.Select(x => x.SeatId));

                return missing.Select(x => { return new Seat(-x, showId, 0, 0, 0); }).ToList();
            }
            seats.ForEach(x => x.HoldSeat(customerEmail));
            if (await TrySave(seats) !=0)
            {
                throw new IndexOutOfRangeException("Sorry Some of the seats requested are not available at this time.");
            }
            
            return seats;
        }

        /// <summary>
        /// Returns the number of seats available
        /// </summary>
        /// <param name="showId"></param>
        /// <returns>Number of available seats</returns>
        public async Task<int> NumSeatsAvailable(int showId)
        {
            return await _context.Seats.Where(x => x.ShowId == showId).CountAsync(x => x.Available);
        }

        /// <summary>
        /// Reserves a Colleciton of Seats if they are still being held for the specified email.
        /// </summary>
        /// <param name="showId">The Id of the show wanting to be seen</param>
        /// <param name="seatIds">List of integetrs indicating the seats you want to reserver</param>
        /// <param name="customerEmail">The email the tickets should be held under</param>
        /// <returns>A success message indicating how many seats were reserved</returns>
        /// <exception cref="ArgumentException">If not all seats are available, throws this with message inside</exception>
        public async Task<string> ReserverSeats(int showId, ICollection<int> seatIds, string customerEmail)
        {
            var seats = await _context.Seats.Where(x => seatIds.Contains(x.ShowId)).ToListAsync();
            var ret = string.Empty;
            if(seats.Where(x=>x.ReservedEmail.Equals(customerEmail)).Count() == seats.Count() &&
               seats.Where(x=>x.Status == TicketLibrary.Enums.SeatStatus.Held).Count() == seats.Count())
            {
                seats.ForEach(x => x.Status = TicketLibrary.Enums.SeatStatus.Reserved);

                if( await TrySave(seats) == 0)
                    return string.Format("You've successfully reserved {0} seats", seats.Count());
            }

            throw new ArgumentException("Not all the seats requested were still held, please request to hold the seats again and retry");
        }

        public async Task<string> ReserverSeats(int showId, string customerEmail)
        {
            var seats = await _context.Seats.Where(x => customerEmail.Equals(x.ReservedEmail)).ToListAsync();
            var ret = string.Empty;

            seats.ForEach(x => x.Status = TicketLibrary.Enums.SeatStatus.Reserved);

            if (await TrySave(seats) == 0)
                return string.Format("You've successfully reserved {0} seats", seats.Count());

            throw new ArgumentException("Not all the seats requested were still held, please request to hold the seats again and retry");
        }

        /// <summary>
        /// Saves the Context and resyncs the database values to internal values for concurrency on Status
        /// </summary>
        /// <returns>Number of rows which were out of date</returns>
        private async Task<int> TrySave(ICollection<Seat> edits)
        {
            var updated= 0;
            bool saved = false;
            while (!saved && updated == 0)
            {
                try
                {
                    //Save, ensuring our Concurrency Field (Status) is up to date and correct.
                    await _context.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is Seat)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();

                            foreach (var property in proposedValues.Properties)
                            {
                                //Resync to Database values
                                if (proposedValues[property] != databaseValues[property])
                                {

                                    if (edits.Contains((Seat)entry.Entity) && property.IsConcurrencyToken && !(proposedValues[property].Equals(TicketLibrary.Enums.SeatStatus.Available) && databaseValues[property].Equals(TicketLibrary.Enums.SeatStatus.Held)))
                                    {
                                        var p = proposedValues[property];
                                        var s = databaseValues[property];
                                        updated++;
                                    }
                                    else if (edits.Contains((Seat)entry.Entity) && property.IsConcurrencyToken && !(proposedValues[property].Equals(TicketLibrary.Enums.SeatStatus.Available) && databaseValues[property].Equals(TicketLibrary.Enums.SeatStatus.Held)))
                                    {
                                        proposedValues[property] = TicketLibrary.Enums.SeatStatus.Available;
                                    }
                                    else
                                    {
                                        proposedValues[property] = databaseValues[property];
                                    }
                                }

                            }

                            // Refresh original values to bypass next concurrency check
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                    }
                }
            }
            return updated;
        }
    }
}
