using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using TrafficLight.Models;

namespace TrafficLight.Controllers
{
    public class SequenceController : ControllerBase
    {
        private MyDb _db;

        public SequenceController(MyDb db)
        {
            _db = db;
        }


        // POST sequence/create
        [HttpPost]
        public async Task <IActionResult> Create()
        {
            Guid sequence = Guid.NewGuid();

            SeqData sd = new SeqData();

            await _db.Set(sequence, sd);

            return new ObjectResult(new
            {
                status = "ok",
                response = new { sequence = sequence }
            })
            { StatusCode = 201 };
        }
    }
}
