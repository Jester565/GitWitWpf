using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitWitWpf.Services
{
    public class GitService
    {
        //Workaround for no statuscode in the exception
        private static HttpResponseMessage EnsureSuccessStatusCodeWithStatus(HttpResponseMessage httpResponseMessage)
        {
            try
            {
                return httpResponseMessage.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                ex.Data["StatusCode"] = httpResponseMessage.StatusCode;
                throw;
            }
        }
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
            _client.BaseAddress = new Uri("https://api.github.com");
            _client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("GitWit", "1.0"));
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetHttpClient(string accessToken)
        {
            if (accessToken != null)
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", accessToken);
            }
        }

        public async Task<string> GetUsername(CancellationToken ct)
        {
            HttpResponseMessage response = await _client.GetAsync("user", ct);
            EnsureSuccessStatusCodeWithStatus(response);
            string userStr = await response.Content.ReadAsStringAsync();
            dynamic user = JsonConvert.DeserializeObject(userStr, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
            return user.login;
        }

        private DateTime GetDayOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public async Task<List<CommitData>> GetRecentCommits(string username, DateTime startDateTime, CancellationToken ct)
        {
            List<CommitData> commits = new List<CommitData>();
            HttpResponseMessage response = await _client.GetAsync($"users/{username}/events", ct);
            EnsureSuccessStatusCodeWithStatus(response);
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
                        try
                        {
                            CommitData commitData = await GetCommit((string)evt.repo.url, (string)commit.sha, ct);
                            commits.Add(commitData);
                        } catch (HttpRequestException ex)
                        {
                            Console.WriteLine("HTTP REQUEST FAILED " + ex);
                            commits.Add(new CommitData {
                                Author = commit.author.name,
                                DateTime = DateTime.Parse((string)evt.created_at).ToLocalTime(),
                                RepoUrl = evt.repo.url
                            });
                        }
                    }
                }
            }
            return commits;
        }

        private async Task<CommitData> GetCommit(string repoUrl, string sha, CancellationToken ct)
        {
            HttpResponseMessage response = await _client.GetAsync($"{repoUrl}/commits/{sha}", ct);
            EnsureSuccessStatusCodeWithStatus(response);
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
