using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using MongoDB.Driver;

namespace AccountingControl.Services
{
    public interface ISubmissionService
    {
        Task PostSubmission(Submission submission);
        Task<IAsyncCursor<Submission>> GetSubmissions(FilterDefinition<Submission> filter);
    }
}
