using Microsoft.AspNetCore.Mvc;
using UserApi.Models;
using UserApi.Services;

namespace UserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<User>> GetAll() => Ok(_service.GetAll());

    [HttpGet("{id}")]
    public ActionResult<User> GetById(int id)
    {
        try
        {
            var user = _service.GetById(id);
            return user is null ? NotFound() : Ok(user);
        }
        catch (Exception ex)
        {
            // Log the exception (e.g., to a file or monitoring system)
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost]
    public IActionResult Create(User user)
    {
        if (!ModelState.IsValid)
        return BadRequest(ModelState);

        _service.Create(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);

    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, User user)
    {
        if (!ModelState.IsValid)
        return BadRequest(ModelState);


        if (_service.GetById(id) is null) return NotFound();
        user.Id = id;
        _service.Update(user);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (_service.GetById(id) is null) return NotFound();
        _service.Delete(id);
        return NoContent();
    }

    [HttpGet("throw")]
    public IActionResult Throw() => throw new Exception("Simulated failure");

}