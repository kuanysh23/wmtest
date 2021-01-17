using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficLight.Models;

namespace TrafficLight.Controllers
{
    [Route("/[controller]")]    
    public class ClearController : ControllerBase
    {
        private MyDb _db;

        public ClearController(MyDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await _db.Dispose();

            return new ObjectResult(new
            {
                status = "ok",
                response = "ok"
            })
            { StatusCode = 200 };
        }
    }
}