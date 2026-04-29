using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Extensions;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class AssessmentController : PrismaBaseEntityController
    {
        private readonly IUserService _userService;
        private readonly IAssessmentService _assessmentService;

        public AssessmentController(AppDbContext dbContext, IUserService userService, IAssessmentService assessmentService) : base(dbContext)
        {
            _userService = userService;
            _assessmentService = assessmentService;
        }
        [HttpGet("assessments/{id}")]
        public async Task<ActionResult<AssessmentOutgoingDto>> GetAssessments(Guid id, CancellationToken ct = default)
        {
            UserOutgoingDto user = HttpContext.GetLoadedUser();
            var result = await _assessmentService.GetAsync(new List<Guid> { id }, user, ct);
            return result != null && result.Count > 0 ? Ok(result[0]) : NotFound();
        }
        [HttpGet("assessments")]
        public async Task<ActionResult<List<AssessmentOutgoingDto>>> GetAllAssessments(CancellationToken ct = default)
        {
            UserOutgoingDto user = HttpContext.GetLoadedUser();
            var result = await _assessmentService.GetAllAsync(user, ct);
            return Ok(result);
        }

        [HttpPost("assessments")]
        public async Task<ActionResult<List<AssessmentOutgoingDto>>> CreateAssessment([FromBody] List<AssessmentIncomingDto> dtos, CancellationToken ct = default)
        {
            UserOutgoingDto user = HttpContext.GetLoadedUser();
            await BeginTransactionAsync(ct);
            try
            {
                var result = await _assessmentService.CreateAsync(dtos, user, ct);
                await CommitTransactionAsync(ct);
                return result != null && result.Count > 0 ? Ok(result) : NotFound();
            }
            catch
            {
                await RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
        }
        [HttpPut("assessments")]
        public async Task<ActionResult<List<AssessmentOutgoingDto>>> UpdateAssessment([FromBody] List<AssessmentIncomingDto> dtos, CancellationToken ct = default)
        {
            UserOutgoingDto user = HttpContext.GetLoadedUser();
            await BeginTransactionAsync(ct);
            try
            {
                await _assessmentService.UpdateRangeAsync(dtos, user, ct);
                await CommitTransactionAsync(ct);
                return NoContent();
            }
            catch
            {
                await RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
        }
        [HttpDelete("assessments/{id:guid}")]
        public async Task<ActionResult<List<AssessmentOutgoingDto>>> DeleteAssessment(Guid id, CancellationToken ct = default)
        {
            UserOutgoingDto user = HttpContext.GetLoadedUser();
            await BeginTransactionAsync(ct);
            try
            {
                await _assessmentService.DeleteAsync(id, user, ct);
                await CommitTransactionAsync(ct);
                return NoContent();
            }
            catch
            {
                await RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
        }
    }


}
