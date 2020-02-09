using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GitWitWpf.Services
{
    public class GitService
    {
        public struct CommitData
        {
            public string Author;
            public DateTime DateTime;
            public string RepoUrl;
            public string Msg;
        }

        private HttpClient _client = new HttpClient();
        public GitService()
        {
        }

        public void SetHttpClient(string accessToken)
        {
            _client.BaseAddress = new Uri("https://api.github.com");
            _client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("GitWit", "1.0"));
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            if (accessToken != null)
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", accessToken);
            }
        }

        private DateTime GetDayOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public async Task<List<CommitData>> GetRecentCommits(string username, DateTime startDateTime)
        {
            List<CommitData> commits = new List<CommitData>();
            HttpResponseMessage response = await _client.GetAsync($"users/{username}/events");
            response.EnsureSuccessStatusCode();
            string eventsStr = await response.Content.ReadAsStringAsync();
            dynamic events = JsonConvert.DeserializeObject(eventsStr, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });

            foreach (dynamic evt in events)
            {
                DateTime evtDateTime = DateTime.Parse((string)evt.created_at).ToLocalTime();
                if (evtDateTime.Date < startDateTime.Date)
                {
                    break;
                }
                //Check if event was a push by this user
                if (evt.type == "PushEvent" && evt.actor.login == username)
                {
                    foreach (dynamic commit in evt.payload.commits)
                    {
                        commits.Add(await GetCommit((string)evt.repo.url, (string)commit.sha));
                    }
                }
            }
            return commits;
        }

        private async Task<CommitData> GetCommit(string repoUrl, string sha)
        {
            HttpResponseMessage response = await _client.GetAsync($"{repoUrl}/commits/{sha}");
            response.EnsureSuccessStatusCode();
            string commitStr = await response.Content.ReadAsStringAsync();
            dynamic commit = ((dynamic)JsonConvert.DeserializeObject(commitStr));
            return new CommitData
            {
                Author = commit.commit.author.name,
                DateTime = DateTime.Parse((string)commit.commit.author.date).ToLocalTime(),
                Msg = commit.commit.message,
                RepoUrl = repoUrl
            };
        }
    }
}
