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

        [HttpPost]
        public IActionResult Create([FromBody] CelestialObject celestialObject)
        {
            _context.CelestialObjects.Add(celestialObject);
            _context.SaveChanges();

            return CreatedAtRoute("GetById", new {id = celestialObject.Id});
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] CelestialObject celestialObject)
        {
            var celestialObjectFromDb = _context.CelestialObjects.FirstOrDefault(o => o.Id == id);

            if (celestialObjectFromDb == null)
                return NotFound();

            celestialObjectFromDb.Name = celestialObject.Name;
            celestialObjectFromDb.OrbitalPeriod = celestialObject.OrbitalPeriod;
            celestialObjectFromDb.OrbitedObjectId = celestialObject.OrbitedObjectId;

            _context.CelestialObjects.Update(celestialObjectFromDb);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPatch("{id}/{name}")]
        public IActionResult RenameObject(int id, string name)
        {
            var celestialObjectFromDb = _context.CelestialObjects.FirstOrDefault(o => o.Id == id);

            if (celestialObjectFromDb == null)
                return NotFound();

            celestialObjectFromDb.Name = name;

            _context.CelestialObjects.Update(celestialObjectFromDb);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var celestialObjectsFromDb = _context.CelestialObjects
                .Where(o => o.Id == id || o.OrbitedObjectId == id).ToList();

            if (celestialObjectsFromDb.Count == 0)
                return NotFound();

            _context.CelestialObjects.RemoveRange(celestialObjectsFromDb);
            _context.SaveChanges();

            return NoContent();
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
