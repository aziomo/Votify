﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Votify
{
    static public class Controller
    {
        static readonly HttpClient httpClient = new HttpClient();

        // do kontaktu z api
        /* logowanie:
                METHOD: POST
                URL: /api/authenticate
                BODY: login*, password*

            eventy:
                HEADER: Auth; z tokenem JWT dla poniższych:
                pobieranie z przedziału:
                    METHOD: GET
                    URL: /api/auth/events?from=<int>&to=<int>
    
                pobieranie z dzisiaj:
                    METHOD: GET
                    URL: /api/auth/events
        */
        public static bool TestNet()
        {
            try
            {
                var body = new Dictionary<string, string> { };
                var content = new FormUrlEncodedContent(body);
                var response = httpClient.PostAsync("http://127.0.0.1/api/authenticate", content).Result;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<string> SendLoginRequest(string login, string password)
        {
            var body = new Dictionary<string, string>
            {
                { "login", login },
                { "password", password }
            };

            var content = new FormUrlEncodedContent(body);

            var response = httpClient.PostAsync("http://127.0.0.1/api/authenticate", content).Result;

            var responseString = response.Content.ReadAsStringAsync();

            return await responseString;
        }


        public static Models.DataFromAuthorization Login(string login, string password)
        {
            var Request = SendLoginRequest(login, password);

            JObject parsedJSON = JObject.Parse(Request.Result);

            if (parsedJSON["message"].ToString() == "Logged in") //Login successful
            {
                string _Token = parsedJSON["token"].ToString();
                Models.User _User = new Models.User(int.Parse(parsedJSON["user"]["UserId"].ToString()), parsedJSON["user"]["UserLogin"].ToString(), parsedJSON["user"]["UserEmail"].ToString(), parsedJSON["user"]["UserName"].ToString());
                Models.DataFromAuthorization _DataFromAuthorization = new Models.DataFromAuthorization(_Token, _User);

                return _DataFromAuthorization;
            }
            else
            {
                return null;
            }




        }
        private static async Task<string> CreateEventsResponse(string Token, string Name, string Desc, string startDate, string endDate)
        {
            if (!httpClient.DefaultRequestHeaders.Contains("Auth"))
                httpClient.DefaultRequestHeaders.Add("Auth", Token);

            var body = new Dictionary<string, string>
            {
                { "name", Name },
                { "desc", Desc },
                { "startDate", startDate },
                { "endDate", endDate }
            };

            var content = new FormUrlEncodedContent(body);

            var response = httpClient.PostAsync("http://127.0.0.1/api/auth/events", content).Result;

            var responseString = response.Content.ReadAsStringAsync();

            return await responseString;
        }


        public static bool CreateEvents(string Token, string Name, string Desc, string startDate, string endDate)
        {
            var Request = CreateEventsResponse(Token, Name, Desc, startDate, endDate);

            if (Request.Status == TaskStatus.Created)
                return true;
            else
                return false;

        }
        private static async Task<string> SendResponseEvent(string Token)
        {
            if (!httpClient.DefaultRequestHeaders.Contains("Auth"))
                httpClient.DefaultRequestHeaders.Add("Auth", Token);

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,

                RequestUri = new Uri("http://127.0.0.1/api/auth/events")
            };

            var response = httpClient.SendAsync(requestMessage).Result;
            var responsecode = response.StatusCode;

            var test = requestMessage.Headers;
            var responseString = response.Content.ReadAsStringAsync();


            return await responseString;

        }

        public static List<Event> GetEventFromResponse(string Token)
        {
            Task<string> EventsResponse = SendResponseEvent(Token);
            JObject parsedJSON = JObject.Parse(EventsResponse.Result);
            EventsResponse.Dispose();
            List<Event> ListEvents = new List<Event>();

            try
            {
                foreach (var element in parsedJSON["events"])
                {
                    Event _Temp;
                    if (element["id"] != null)
                        _Temp = new Event((string)element["date"]["start"], (string)element["date"]["end"], (string)element["title"], (string)element["description"], (int)element["id"]);
                    else _Temp = new Event((string)element["date"]["start"], (string)element["date"]["end"], (string)element["title"], (string)element["description"], 100);
                    ListEvents.Add(_Temp);
                }
            }
            catch
            {
                return null;
            }

            return ListEvents;
        }
    }
}