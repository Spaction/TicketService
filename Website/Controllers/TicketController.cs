using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TicketLibrary.Context;
using TicketLibrary.Models;
using WebAPI.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly TicketContext _context;
        private readonly TicketRepository ticketRepository;
        private ILogger<TicketController> logger;

        public TicketController(TicketContext context, ILogger<TicketController> logger)
        {
            _context = context;
            ticketRepository = new TicketRepository(context,logger);
            this.logger = logger;
        }

        [HttpGet]
        public Task<List<Show>> GetAll()
        {
            return _context.Shows.Include(x=>x.Seats).Select(x=> new Show(){ AvailableSeats = x.AvailableSeats,ShowId= x.ShowId, Title= x.Title,Description= x.Description, Seats=x.Seats }).ToListAsync();
        }

        [HttpGet,Route("{ShowId}")]
        public async Task<Show> GetShowSeats(int ShowId)
        {
            return await _context.Shows.Where(x => x.ShowId == ShowId).Include(y => y.Seats).ThenInclude(z=>z.Loc).FirstOrDefaultAsync();
        }

        [HttpGet,Route("SeatsAvailable/{ShowId}")]
        public async Task<int> GetAvailableSeatsAsync(int ShowId)
        {
            return await ticketRepository.NumSeatsAvailable(ShowId);
        }

        [HttpGet,Route("Hold/{showId}/{numSeats}/{customerEmail}")]
        public async Task<IActionResult> HoldSeatsAsync(int showId,int numSeats,string customerEmail)
        {
            var obj = new JObject()
            {
                new JProperty("ShowId",showId),
                new JProperty("customerEmail",customerEmail),
                new JProperty("numSeats",numSeats)
            };

            return await HoldSeatsAsync(obj);
        }

        [HttpPost,Route("Hold")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> HoldSeatsAsync([FromBody]JObject obj)
        {
            try
            {
                var ShowId = obj.Value<int>("ShowId");
                var customerEmail = obj.Value<string>("customerEmail");
                if(string.IsNullOrEmpty(customerEmail))
                {
                    return BadRequest("customerEmail is a required parameter");
                }
                ICollection<Seat> tickets;
                if (obj.ContainsKey("seats"))
                {
                    var seats = obj.Value<JArray>("seats").ToObject<List<int>>();
                    tickets = await ticketRepository.FindAndHoldSeats(ShowId, seats, customerEmail);
                    //Negative SeatId indicates the list contains seats which are taken.
                    if (tickets != null && tickets.Count > 0 && tickets.First().SeatId < 0)
                        return new ContentResult()
                        {
                            StatusCode = 400,
                            Content = JsonConvert.SerializeObject(tickets),
                            ContentType = "application/json"
                        };
                }
                else
                {
                    var numSeats = obj.Value<int>("numSeats");
                    tickets = await ticketRepository.FindAndHoldSeats(ShowId, numSeats, customerEmail);
                }

                return new ObjectResult(tickets);
            }
            catch (IndexOutOfRangeException e)
            {
                return BadRequest(e.Message);
            }
            catch(Exception e)
            {
                logger.LogError(e.ToString());
                return StatusCode(500);
            }

        }

        [HttpPost,Route("ReserveSeats")]
        public async Task<IActionResult> ReserverSeats([FromBody] JObject obj)
        {
            try
            {
                var ShowId = obj.Value<int>("ShowId");
                var customerEmail = obj.Value<string>("CustomerEmail");

                if(obj.ContainsKey("seats"))
                {
                    var Seats = obj.Value<JArray>("seats").ToObject<List<int>>();
                    return new ObjectResult(await ticketRepository.ReserverSeats(ShowId, Seats, customerEmail));
                }
                else
                {
                    return new ObjectResult(await ticketRepository.ReserverSeats(ShowId, customerEmail));
                }
                
                
            }
            catch (ArgumentException e)
            {
                logger.LogDebug(e.Message);
                return new BadRequestObjectResult(e.Message);
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return StatusCode(500);
            }
        }
    }
}
