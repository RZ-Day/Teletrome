using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

[ApiController]
[Route("/projects")]
public class ProjectsController : ControllerBase
{
    [HttpPost]
    [Route("/create-new")]
    public async Task<IActionResult> CreateNewProjectAsync([FromBody] string projectName)
    {
        
        return null;
    }
}