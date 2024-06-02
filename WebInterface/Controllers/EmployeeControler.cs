using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;

namespace vassago.Controllers;

[Route("[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
	[HttpGet]
	[Produces("application/json")]
	public IEnumerable<Account> Get()
	{
		return GetEmployeesDeatils();
	}

	[HttpGet("{id}")]
	[Produces("application/json")]
	public Account Get(Guid id)
	{
		return GetEmployeesDeatils().Find(e => e.Id == id);
	}

	[HttpPost]
	[Produces("application/json")]
	public Account Post([FromBody] Account employee)
	{
		// Write logic to insert employee data
		return new Account();
	}

	private List<Account> GetEmployeesDeatils()
	{
		return new List<Account>()
		{
			new Account()
		};
	}
}