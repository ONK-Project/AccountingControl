using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountingControl.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;

namespace AccountingControl.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SubmissionController : ControllerBase
    {
        private readonly ILogger<SubmissionController> _logger;
        private readonly ISubmissionService _submissionService;

        public SubmissionController(ILogger<SubmissionController> logger, ISubmissionService submissionService)
        {
            _logger = logger;
            _submissionService = submissionService;
        }

        [HttpPost]
        public async Task<ActionResult> Post(Submission submission)
        {
            await _submissionService.PostSubmission(submission);
            return Created("GetSubmission", submission);
        }
    }
}