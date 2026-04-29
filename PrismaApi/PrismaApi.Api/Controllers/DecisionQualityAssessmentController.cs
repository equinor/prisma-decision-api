using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Extensions;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class DecisionQualityAssessmentController : PrismaBaseEntityController
    {
        private readonly IUserService _userService;
        private readonly IDecisionQualityAssessmentService _DecisionQualityAssessmentService;

        public DecisionQualityAssessmentController(
            AppDbContext dbContext,
            IUserService userService,
            IDecisionQualityAssessmentService DecisionQualityAssessmentService) : base(dbContext)
        {
            _userService = userService;
            _DecisionQualityAssessmentService = DecisionQualityAssessmentService;
        }

        [HttpGet("dq_assessments/{id}")]
        public async Task<ActionResult<DecisionQualityAssessmentOutgoingDto>> GetDecisionQualityAssessment(Guid id, CancellationToken ct = default)
        {
            UserOutgoingDto user = HttpContext.GetLoadedUser();
            var result = await _DecisionQualityAssessmentService.GetAsync(id, user, ct);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("dq_assessments")]
        public async Task<ActionResult<List<DecisionQualityAssessmentOutgoingDto>>> GetAllDecisionQualityAssessments(CancellationToken ct = default)
        {
            UserOutgoingDto user = HttpContext.GetLoadedUser();
            var result = await _DecisionQualityAssessmentService.GetAllAsync(user, ct);
            return Ok(result);
        }

        [HttpPost("dq_assessments")]
        public async Task<ActionResult<List<DecisionQualityAssessmentOutgoingDto>>> CreateDecisionQualityAssessment([FromBody] List<DecisionQualityAssessmentIncomingDto> dtos, CancellationToken ct = default)
        {
            UserOutgoingDto user = HttpContext.GetLoadedUser();
            await BeginTransactionAsync(ct);
            try
            {
                var result = await _DecisionQualityAssessmentService.CreateAsync(dtos, user, ct);
                await CommitTransactionAsync(ct);
                return result != null && result.Count > 0 ? Ok(result) : NotFound();
            }
            catch
            {
                await RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
        }

        [HttpPut("dq_assessments")]
        public async Task<ActionResult<List<DecisionQualityAssessmentOutgoingDto>>> UpdateDecisionQualityAssessment([FromBody] List<DecisionQualityAssessmentIncomingDto> dtos, CancellationToken ct = default)
        {
            UserOutgoingDto user = HttpContext.GetLoadedUser();
            await BeginTransactionAsync(ct);
            try
            {
                await _DecisionQualityAssessmentService.UpdateAsync(dtos, user, ct);
                await CommitTransactionAsync(ct);
                return NoContent();
            }
            catch
            {
                await RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
        }

        [HttpDelete("dq_assessments/{id:guid}")]
        public async Task<ActionResult> DeleteDecisionQualityAssessment(Guid id, CancellationToken ct = default)
        {
            UserOutgoingDto user = HttpContext.GetLoadedUser();
            await BeginTransactionAsync(ct);
            try
            {
                await _DecisionQualityAssessmentService.DeleteAsync(new List<Guid> { id }, user, ct);
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
