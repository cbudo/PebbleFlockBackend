using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using SimpleOAuth;
using Twilio;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace pebblewebapp.Controllers
{
    class YelpAPIClient
    {
        private const string CONSUMER_KEY = "uL6xCtuJTiEFCVrsf6ubbQ";
        /// <summary>
        /// Consumer secret used for OAuth authentication.
        /// This must be set by the user.
        /// </summary>
        private const string CONSUMER_SECRET = "xfBEGe2ZFJJmyWnWpBhiPA4cMDo";

        /// <summary>
        /// Token used for OAuth authentication.
        /// This must be set by the user.
        /// </summary>
        private const string TOKEN = "AnpMbjAY-5MIm8yCT5oZ_AXMx2rEGocU";

        /// <summary>
        /// Token secret used for OAuth authentication.
        /// This must be set by the user.
        /// </summary>
        private const string TOKEN_SECRET = "wbYbOmAZflFnxLDNjH9vJAyqUGg";

        /// <summary>
        /// Host of the API.
        /// </summary>
        private const string API_HOST = "http://api.yelp.com";

        /// <summary>
        /// Relative path for the Search API.
        /// </summary>
        private const string SEARCH_PATH = "/v2/search/";

        /// <summary>
        /// Relative path for the Business API.
        /// </summary>
        private const string BUSINESS_PATH = "/v2/business/";

        /// <summary>
        /// Search limit that dictates the number of businesses returned.
        /// </summary>
        private const int SEARCH_LIMIT = 7;

        /// <summary>
        /// Prepares OAuth authentication and sends the request to the API.
        /// </summary>
        /// <param name="baseURL">The base URL of the API.</param>
        /// <param name="queryParams">The set of query parameters.</param>
        /// <returns>The JSON response from the API.</returns>
        /// <exception>Throws WebException if there is an error from the HTTP request.</exception>
        private JObject PerformRequest(string baseURL, Dictionary<string, string> queryParams = null)
        {
            var query = System.Web.HttpUtility.ParseQueryString(String.Empty);

            if (queryParams == null)
            {
                queryParams = new Dictionary<string, string>();
            }

            foreach (var queryParam in queryParams)
            {
                query[queryParam.Key] = queryParam.Value;
            }

            var uriBuilder = new UriBuilder(baseURL);
            uriBuilder.Query = query.ToString();
            var uribuilderstring = uriBuilder.ToString().Replace("%2c", ",");
            var request = WebRequest.Create(uribuilderstring);
            request.Method = "GET";

            request.SignRequest(
                new Tokens
                {
                    ConsumerKey = CONSUMER_KEY,
                    ConsumerSecret = CONSUMER_SECRET,
                    AccessToken = TOKEN,
                    AccessTokenSecret = TOKEN_SECRET
                }
            ).WithEncryption(EncryptionMethod.HMACSHA1).InHeader();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            return JObject.Parse(stream.ReadToEnd());
        }

        /// <summary>
        /// Query the Search API by a search term and location.
        /// </summary>
        /// <param name="term">The search term passed to the API.</param>
        /// <param name="location">The search location passed to the API.</param>
        /// <returns>The JSON response from the API.</returns>
        public JObject Search(string term, double latitude, double longitude)
        {
            string baseURL = API_HOST + SEARCH_PATH;
            var queryParams = new Dictionary<string, string>()
            {
                { "term", term },
                { "ll", latitude+","+longitude},
                { "limit", SEARCH_LIMIT.ToString() }
            };
            return PerformRequest(baseURL, queryParams);
        }

        /// <summary>
        /// Query the Business API by a business ID.
        /// </summary>
        /// <param name="business_id">The ID of the business to query.</param>
        /// <returns>The JSON response from the API.</returns>
        public JObject GetBusiness(string business_id)
        {
            string baseURL = API_HOST + BUSINESS_PATH + business_id;
            return PerformRequest(baseURL);
        }
    }
    public class apicalls
    {
        public bool twillioJoin(string fromNum, string toNum, string business)
        {

            try
            {
                YelpAPIClient yelpClient = new YelpAPIClient();
                var buisness = yelpClient.GetBusiness(business);
                var buisnessURl = buisness.GetValue("url");


                string AccountSid = "ACcdeba5687e73b2f0018fe8b7004e6fc8";
                string AuthToken = "1e389c71315f88b00945ed9499dea847";
                var twilio = new TwilioRestClient(AccountSid, AuthToken);

                var message = twilio.SendMessage("+15714512210", "+1" + toNum, "Your friend thinks you should join them at " + buisnessURl);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool twillioRec(string fromNum, string toNum, string business)
        {
            try
            {
                YelpAPIClient yelpClient = new YelpAPIClient();
                var buisness = yelpClient.GetBusiness(business);
                var buisnessURl = buisness.GetValue("url");


                string AccountSid = "ACcdeba5687e73b2f0018fe8b7004e6fc8";
                string AuthToken = "1e389c71315f88b00945ed9499dea847";
                var twilio = new TwilioRestClient(AccountSid, AuthToken);

                var message = twilio.SendMessage("+15714512210", "+1" + toNum, "Your friend thinks you would like this restaurant " + buisnessURl);
                return true;
            }
            catch
            {
                return false;
            }

        }
        public bool MailJetJoin(string toMail, string fromMail, string business)
        {
            string APIKey = "633afd3eb0cf804cdb516339f60daca3";
            string SecretKey = "e70e0805566054df533c996b3f67b96b";
            string from = fromMail;
            string to = toMail;
            YelpAPIClient yelpClient = new YelpAPIClient();
            var buisness = yelpClient.GetBusiness(business);

            try
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["from"] = "budocf@rose-hulman.edu";
                    values["to"] = toMail;
                    int hour = System.DateTime.Now.Hour;
                    string meal = "a meal";
                    if (hour < 11)
                    {
                        meal = "breakfast";
                    }
                    else if (hour < 15)
                    {
                        meal = "lunch";
                    }
                    else
                    {
                        meal = "dinner";
                    }
                    values["subject"] = "Melissa wants you to join her for " + meal + "!";
                    values["text"] = "Melissa is currently at " + buisness.GetValue("name") + "\nAnd wants you to join her!\n" + buisness.GetValue("url");
                    NetworkCredential myCreds = new NetworkCredential(APIKey, SecretKey);
                    client.Credentials = myCreds;

                    var response = client.UploadValues("http://api.mailjet.com/v3/send/message", values);

                    var responseString = Encoding.Default.GetString(response);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool MailJetSuggest(string toMail, string fromMail, string business)
        {
            string APIKey = "633afd3eb0cf804cdb516339f60daca3";
            string SecretKey = "e70e0805566054df533c996b3f67b96b";
            string from = fromMail;
            string to = toMail;
            YelpAPIClient yelpClient = new YelpAPIClient();
            var buisness = yelpClient.GetBusiness(business);

            try
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["from"] = "budocf@rose-hulman.edu";
                    values["to"] = toMail;
                    int hour = System.DateTime.Now.Hour;
                    string meal = "a meal";
                    if (hour < 11)
                    {
                        meal = "breakfast";
                    }
                    else if (hour < 15)
                    {
                        meal = "lunch";
                    }
                    else
                    {
                        meal = "dinner";
                    }
                    values["subject"] = "Melissa was thinking of you!";
                    values["text"] = "Melissa is currently at " + buisness.GetValue("name") + "\nAnd thinks that you would enjoy it!\n" + buisness.GetValue("url");
                    NetworkCredential myCreds = new NetworkCredential(APIKey, SecretKey);
                    client.Credentials = myCreds;

                    var response = client.UploadValues("http://api.mailjet.com/v3/send/message", values);

                    var responseString = Encoding.Default.GetString(response);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        internal bool MailJetMessage(string toMail, string business, string message)
        {
            string APIKey = "633afd3eb0cf804cdb516339f60daca3";
            string SecretKey = "e70e0805566054df533c996b3f67b96b";
            YelpAPIClient yelpClient = new YelpAPIClient();
            var buisness = yelpClient.GetBusiness(business);

            try
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["from"] = "budocf@rose-hulman.edu";
                    values["to"] = toMail;
                    values["subject"] = "Let's go to " + buisness.GetValue("name") + "!";
                    values["text"] = "Let's go to " + buisness.GetValue("name") + "\n\n" + message + "\n\n" + buisness.GetValue("url");
                    NetworkCredential myCreds = new NetworkCredential(APIKey, SecretKey);
                    client.Credentials = myCreds;

                    var response = client.UploadValues("http://api.mailjet.com/v3/send/message", values);

                    var responseString = Encoding.Default.GetString(response);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    public class apisController : Controller
    {
        apicalls calls = new apicalls();
        public static void QueryAPIAndPrintResult(string term, double latitude, double longitude)
        {
            var client = new YelpAPIClient();


            JObject response = client.Search(term, latitude, longitude);

            JArray businesses = (JArray)response.GetValue("businesses");

            if (businesses.Count == 0)
            {
                //Console.WriteLine("No businesses for {0} in {1} found.", term, location);
                return;
            }

            string business_id = (string)businesses[0]["id"];

            Console.WriteLine(
                "{0} businesses found, querying business info for the top result \"{1}\"...",
                businesses.Count,
                business_id
            );

            response = client.GetBusiness(business_id);

            Console.WriteLine(String.Format("Result for business {0} found:", business_id));
            Console.WriteLine(response.ToString());
        }
        // GET: API
        public JObject NearbyRestaurants(double latitude, double longitude)
        {
            YelpAPIClient client = new YelpAPIClient();

            return client.Search("food", latitude, longitude);
        }
        public JObject GetBusiness(string business)
        {
            YelpAPIClient client = new YelpAPIClient();

            return client.GetBusiness(business);
        }
        public ActionResult Loginfo()
        {
            return View();
        }
        public void setInfo(string username, string password)
        {
            return;
        }
        public bool TwilJoin(string fromNum, string toNum, string business)
        {
            return calls.twillioJoin(fromNum, toNum, business);
        }
        public bool TwilSuggest(string fromNum, string toNum, string business)
        {
            return calls.twillioRec(fromNum, toNum, business);
        }
        public bool JetJoin(string toMail, string fromMail, string business)
        {
            return calls.MailJetJoin(toMail, fromMail, business);
        }
        public bool JetSuggest(string toMail, string fromMail, string business)
        {
            return calls.MailJetSuggest(toMail, fromMail, business);
        }
        public bool JetMessage(string toMail, string business, string message)
        {
            return calls.MailJetMessage(toMail, business, message);
        }
    }
}