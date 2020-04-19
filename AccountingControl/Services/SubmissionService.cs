using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountingControl.Data;
using Models;
using MongoDB.Driver;

namespace AccountingControl.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IMongoCollection<Submission> submissions;
        public SubmissionService(IAccountControlDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            submissions = database.GetCollection<Submission>(settings.SubmissionCollectionName);
        }
        public Task PostSubmission(Submission submission)
        {
            return submissions.InsertOneAsync(submission);
        }

        public Task<IAsyncCursor<Submission>> GetSubmissions(FilterDefinition<Submission> filter)
        {
            return submissions.FindAsync(filter);
        }
    }
}
