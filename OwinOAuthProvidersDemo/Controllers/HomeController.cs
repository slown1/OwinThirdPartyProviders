﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using EmailUtilsRazor;
using Newtonsoft.Json;
using Owin.Security.Providers;
using OwinOAuthProvidersDemo.Models;

namespace OwinOAuthProvidersDemo.Controllers
{
    public partial class HomeController : Controller
    {
        String token = "147a9066d2f30d590ce1217ff90fbf887eec7c0f";
        private HttpClient client = null;
        List<String> currentTrackTeamCombinations = new List<string>();
        public async virtual Task<ActionResult> Index()
        {

            await GetGithubReposStatistics();
            //List<CreateRepoModel> reposToCreate = new List<CreateRepoModel>();
            ////TODO: check if list name is unique -> if equal (1)
            //var lines = FileReaderBusinessLogic.ReadFiles(Server.MapPath("/Input/Input.txt"));
            //if (lines != null && lines.Length > 1)
            //{
            //    string currentLine = null;
            //    for (int i = 0; i < lines.Length; i++)
            //    {
            //        try
            //        {
            //            currentLine = lines[i];
            //            if (i == 0)
            //            {
            //                continue;
            //            }
            //            string[] partsOfCurrentLines = currentLine.Split(new string[] { "," }, StringSplitOptions.None);
            //            if (partsOfCurrentLines != null && partsOfCurrentLines.Length > 0)
            //            {
            //                string currentUserGithubUsername = null;
            //                string currentUserEmail = null;
            //                string currentUserName = null;
            //                CreateRepoModel currentRepoToCreate = new CreateRepoModel();
            //                List<UserToSendEmailTo> usersToSendEmailTo = new List<UserToSendEmailTo>();
            //                // track name  
            //                currentRepoToCreate.TrackName = partsOfCurrentLines[1].Trim();
            //                //team name
            //                currentRepoToCreate.TeamName = partsOfCurrentLines[3].Trim();
            //                //mentor name
            //                currentRepoToCreate.MentorName = partsOfCurrentLines[2].Trim();

            //                List<string> collaboratorsEntry = partsOfCurrentLines.ToList();
            //                for (int l = 0; l < 4; l++)
            //                {
            //                    collaboratorsEntry.RemoveAt(0);
            //                }

            //                for (int j = 0; j < collaboratorsEntry.ToArray().Length; j++)
            //                {
            //                    if (j % 4 == 0)
            //                    {
            //                        try
            //                        {
            //                            currentUserEmail = collaboratorsEntry.ElementAt(j + 1);
            //                            currentUserGithubUsername = collaboratorsEntry.ElementAt(j + 2);
            //                            if (String.IsNullOrWhiteSpace(currentUserGithubUsername) == false)
            //                            {
            //                                try
            //                                {
            //                                    if (currentRepoToCreate.Collaboratos.ContainsKey(currentUserGithubUsername) == false)
            //                                    {
            //                                        currentRepoToCreate.Collaboratos.Add(currentUserGithubUsername, currentUserEmail);
            //                                    }
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    ex.ToString();
            //                                }
            //                            }
            //                            else
            //                            {
            //                                if (String.IsNullOrWhiteSpace(currentUserEmail) == false)
            //                                {
            //                                    usersToSendEmailTo.Add(new UserToSendEmailTo()
            //                                    {
            //                                        Email = currentUserEmail,
            //                                        Name = currentUserName
            //                                    });
            //                                }
            //                            }
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            ex.ToString();
            //                        }
            //                    }
            //                }
            //                String trackAndTeamName = String.Format("{0}-{1}", currentRepoToCreate.TrackName, currentRepoToCreate.TeamName);
            //                if (currentTrackTeamCombinations.Contains(trackAndTeamName) == true)
            //                {
            //                    currentRepoToCreate.TeamName = trackAndTeamName = String.Format("{0}-{1}", currentRepoToCreate.TeamName, RandomStringGenerator.RandomString(10));
            //                }
            //                currentTrackTeamCombinations.Add(trackAndTeamName);

            //                reposToCreate.Add(currentRepoToCreate);
            //                await CreateRepoAndAddCollaborators(currentRepoToCreate, usersToSendEmailTo);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            ex.ToString();
            //        }
            //    }
            //}
            return View();
        }

        private async Task GetGithubReposStatistics()
        {
            try
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "https://api.github.com/meta");
                var result = await client.GetAsync(
                    String.Format("https://api.github.com/users/hacktm/repos?access_token={0}&direction=desc", token));

                int sum = 0;
                var repos = JsonConvert.DeserializeObject<List<GithubCreateRepositoryResponseRepoDetails>>(await result.Content.ReadAsStringAsync());
                if( repos != null)
                {
                    foreach (var repo in repos)
                    {
                        sum += await GetGithubRepoCommits(repo.Name);
                    }
                    
                }
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private async Task<int> GetGithubRepoCommits(string repoName)
        {
            try
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "https://api.github.com/meta");
                var result = await client.GetAsync(String.Format("https://api.github.com/repos/hacktm/{0}/commits?access_token={1}", repoName, token));
                try
                {
                    var commits = JsonConvert.DeserializeObject<List<CommitModel>>(await result.Content.ReadAsStringAsync());
                    if (commits != null)
                    {
                        return commits.Count;
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return 0;
        }


        #region createRepos
        private void SendEmailToUserWithoutGithubAccount(UserToSendEmailTo userInfo)
        {
            try
            {
                bool result = EmailHelper.SendHtmlTemplatedEmailFromPath(userInfo.Email, "HackTM Notice", "Views/Templates/Emails/UserSendEmailNoGitAccount.cshtml", true, userInfo);
                if (result == false)
                {
                    int x = 0;
                    x++;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
        private async Task CreateRepoAndAddCollaborators(CreateRepoModel currentRepoToCreate, List<UserToSendEmailTo> usersToSendEmailTo)
        {
            try
            {
                if (currentRepoToCreate != null && String.IsNullOrWhiteSpace(currentRepoToCreate.TrackName) == false
                    && String.IsNullOrWhiteSpace(currentRepoToCreate.TeamName) == false)
                {
                    client = new HttpClient();
                    GithubCreateRepositoryRequest model = new GithubCreateRepositoryRequest()
                    {
                        Name = String.Format("{0}-{1}", currentRepoToCreate.TrackName, currentRepoToCreate.TeamName),
                        Description = String.Format("Mentor-{0}", currentRepoToCreate.MentorName),
                        Homepage = "https://github.com",
                        Private = false,
                        HasIssues = true,
                        HasWiki = true,
                        HasDownloads = true
                    };

                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "https://api.github.com/meta");

                    try
                    {
                        var result = await client.PostAsync(new Uri(String.Format("https://api.github.com/user/repos?access_token={0}", token)),
                                     new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
                        result.EnsureSuccessStatusCode();
                        string content = await result.Content.ReadAsStringAsync();
                        var parsedObject = JsonConvert.DeserializeObject<GithubCreateRepositoryResponseRepoDetails>(content);
                        if (currentRepoToCreate.Collaboratos != null && currentRepoToCreate.Collaboratos.Count > 0)
                        {
                            foreach (var collaboratorsEntry in currentRepoToCreate.Collaboratos)
                            {
                                try
                                {
                                    result = await client.PutAsync(new Uri(
                                     String.Format("https://api.github.com/repos/{0}/collaborators/{1}?access_token={2}", parsedObject.FullName, collaboratorsEntry.Key.Trim(), token)), null);
                                    result.EnsureSuccessStatusCode();
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                }
                            }
                        }

                        if (usersToSendEmailTo != null && usersToSendEmailTo.Count > 0)
                        {
                            foreach (var userToSendEmailToEntry in usersToSendEmailTo)
                            {
                                userToSendEmailToEntry.RepoLink = parsedObject.RepoUrl;
                                SendEmailToUserWithoutGithubAccount(userToSendEmailToEntry);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        #endregion

        public virtual ActionResult SigninGithub()
        {
            return RedirectToAction("Index");
        }

        public virtual ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public virtual ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}