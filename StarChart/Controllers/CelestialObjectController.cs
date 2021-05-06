using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using StarChart.Data;
using StarChart.Models;

namespace StarChart.Controllers
{
    [Route("")]
    [ApiController]
    public class CelestialObjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CelestialObjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}", Name = "GetById")]
        public IActionResult GetById(int id)
        {
            var celestialObject = _context.CelestialObjects.FirstOrDefault(o => o.Id == id);
            
            if (celestialObject == null)
                return NotFound();

            celestialObject.Satellites = GetSatellitesOfCelestialObject(celestialObject.Id);

            return Ok(celestialObject);
        }

        
        [HttpGet("{name}", Name = "GetByName")]
        public IActionResult GetByName(string name)
        {
            var celestialObjects = _context.CelestialObjects.Where(o => o.Name == name).ToList();

            if (celestialObjects.Count == 0)
                return NotFound();

            foreach (var celestialObject in celestialObjects)
            {
                celestialObject.Satellites = GetSatellitesOfCelestialObject(celestialObject.Id);
            }

            return Ok(celestialObjects);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var celestialObjects = _context.CelestialObjects.ToList();

            foreach (var celestialObject in celestialObjects)
            {
                celestialObject.Satellites = celestialObjects
                    .Where(o => o.OrbitedObjectId.HasValue 
                                && o.OrbitedObjectId == celestialObject.Id)
                    .ToList();
            }

            return Ok(celestialObjects);
        }

        private List<CelestialObject> GetSatellitesOfCelestialObject(int celestialObjectId)
        {
            return _context.CelestialObjects
                .Where(o => o.OrbitedObjectId.HasValue 
                            && o.OrbitedObjectId == celestialObjectId)
                .ToList();
        }
    }
}
