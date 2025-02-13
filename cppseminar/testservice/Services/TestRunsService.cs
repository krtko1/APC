using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using testservice.Models;

namespace testservice.Services;

public class TestRunsService
{
    private readonly IMongoCollection<TestRun> _testRuns;

    public TestRunsService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(
            databaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            databaseSettings.Value.DatabaseName);

        _testRuns = mongoDatabase.GetCollection<TestRun>("testRuns");
    }

    // only version with both parameters set is covered by index
    public async Task<List<TestRun>> GetAsync(string userEmail, string submissionId, int count)
    {
        var filter = Builders<TestRun>.Filter.Empty;

        if (userEmail != null)
            filter &= Builders<TestRun>.Filter.Eq(x => x.CreatedBy, userEmail);

        if (submissionId != null)
            filter &= Builders<TestRun>.Filter.Eq(x => x.SubmissionId, submissionId);

        return await _testRuns.Find(filter).SortBy(x => x.CreatedAt).Limit(count).ToListAsync();
    }

    public async Task<TestRun> GetAsync(string id) =>
        await _testRuns.Find(x => x.Id == id).SingleAsync();

    public async Task<long> GetCountAsync(string userEmail, string testCaseId) =>
        await _testRuns.Find(x => x.TestCaseId == testCaseId && x.CreatedBy == userEmail).CountDocumentsAsync();

    public async Task CreateAsync(TestRun newTask) =>
        await _testRuns.InsertOneAsync(newTask);

    public async Task UpdateAsync(TestRun updatedTask) =>
        await _testRuns.ReplaceOneAsync(x => x.Id == updatedTask.Id, updatedTask);
}
