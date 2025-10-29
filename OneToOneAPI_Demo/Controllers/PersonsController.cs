using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneToOneAPI_Demo.Data;
using OneToOneAPI_Demo.Models;

namespace OneToOneAPI_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        AppDbContext _db;
        public PersonsController(AppDbContext db) {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetPeople()
        {
            return await _db.Persons.Include(p => p.Passport).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPeopleById(int id)
        {
            var person = await _db.Persons.Include(p => p.Passport).FirstOrDefaultAsync(s => s.PersonId == id);
            if (person == null)
            {
                return NotFound();
            }
            return person;
        }

        [HttpPost]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            return CreatedAtAction("GetPeopleById", new { id = person.PersonId }, person);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Person>> UpdatePerson(int id,[FromBody] Person person)
        {
            if(id != person.PersonId)
            {
                return BadRequest("Id not matched");
            }

            var existing = await _db.Persons.Include(s => s.Passport).FirstOrDefaultAsync(s => s.PersonId == id);
            if (existing == null)
            {
                return BadRequest("Person not Present");
            }
            existing.Name = person.Name;

            if (person.Passport != null)
            {
                if (existing.Passport != null)
                {
                    existing.Passport.PassportNumber = person.Passport.PassportNumber;
                    existing.Passport.IssueDate = person.Passport.IssueDate;
                    existing.Passport.ExpiryDate = person.Passport.ExpiryDate;
                }
                else
                {
                    existing.Passport = new Passport
                    {
                        PersonId = id,
                        PassportNumber = person.Passport.PassportNumber,
                        IssueDate = person.Passport.IssueDate,
                        ExpiryDate = person.Passport.ExpiryDate
                    };
                }

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }

                }
                return existing;
            }
            return null;
        }

        private bool PersonExists(int id)
        {
            return _db.Persons.Any(p => p.PersonId == id);
        }


        [HttpDelete]
        public async Task<ActionResult<Person>> DeletePerson(int id)
        {
            var persons = await _db.Persons.Include(p => p.Passport).FirstOrDefaultAsync(p => p.PersonId == id);
            if (persons == null)
            {
                return BadRequest();
            }
            if (persons.Passport != null)
            {
                _db.Passports.Remove(persons.Passport);
            }
            _db.Persons.Remove(persons);
            await _db.SaveChangesAsync();
            return Ok(persons);
        }

        [HttpPost("AddPassport/{personId}")]
        public async Task<ActionResult<Passport>> AddPassport(int personId, [FromBody] Passport passport)
        {
            // Step 1: Find the person
            var person = await _db.Persons
                .Include(p => p.Passport)
                .FirstOrDefaultAsync(p => p.PersonId == personId);

            if (person == null)
            {
                return NotFound($"Person with ID {personId} not found.");
            }

            // Step 2: Check if person already has a passport (since it's one-to-one)
            if (person.Passport != null)
            {
                return BadRequest("This person already has a passport assigned.");
            }

            // Step 3: Create and assign passport
            passport.PersonId = personId;
            _db.Passports.Add(passport);
            await _db.SaveChangesAsync();

            // Step 4: Return created passport
            return CreatedAtAction(nameof(GetPeopleById), new { id = personId }, passport);
        }
    }
}
