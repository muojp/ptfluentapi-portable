using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using PivotalTracker.FluentAPI.Domain;
using PortableRest;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using System.Runtime.Serialization;

namespace PivotalTracker.FluentAPI.Repository
{
    /// <summary>
    /// Base class that all repositories can inherit.
    /// </summary>
    /// 
    public class PivotalTrackerRepositoryBase : RestClient, IPivotalTrackerRepository
    {
        private const int REQUEST_PAUSE = 2000; //milliseconds

        /// <summary>
        /// PivotalTrackerRepositoryBase Constructor
        /// </summary>
        /// <param name="token">Token for the Pivotal API Connection</param>
        public PivotalTrackerRepositoryBase(Token token)
        {
            if (token == null || String.IsNullOrWhiteSpace(token.ApiKey) || String.IsNullOrWhiteSpace(token.BaseUrl))
                throw new ArgumentNullException("token");

            this.Token = token;
        }

        public Token Token { get; protected set; }

        /// <summary>
        /// This method send a request to Pivotal and deserialize the XML Result into T
        /// </summary>
        /// <remarks>Sometimes Pivotal cannot answer to the request (ex: to much load). So this method do 3 retries with a pause of 2 seconds between</remarks>
        /// <typeparam name="T">The Pivotal XML Response will be deserialized into T. Must be conformed to the Pivotal API</typeparam>
        /// <param name="path">Relative URL (path) of the Pivotal API (ex:/projects/PROJECT_ID/stories/STORY_ID)</param>
        /// <param name="data">object that will be serialized to the Request stream (usefull for PUT request)</param>
        /// <param name="methodName">method used to send the requestPivotal</param>
        /// <returns>the deserialized Pivotal XML Response</returns>
        protected async Task<T> RequestPivotalAsync<T>(string path, dynamic data, string methodName = "POST")
            where T : class
        {
            //Sometimes Pivotal Fails, so let's retry several times
            int nTries = 2;

            var method = MapHttpMethod(methodName);
            while(nTries > 0)
            {
                Uri lUri = GetPivotalURI(path);
                RestRequest lRequest;

                lRequest = new RestRequest(lUri.AbsoluteUri, method, ContentTypes.Json);
                lRequest.AddHeader("X-TrackerToken", this.Token.ApiKey);
                lRequest.AddHeader("Accepts", "application/json");
                lRequest.ContentType = ContentTypes.Json;
                // string debug = "";
                if (data != null)
                {
                    lRequest.AddParameter(data);
                }

                try
                {
                    return await this.ExecuteAsync<T>(lRequest);
                }
                catch (HttpRequestException e)
                {
                    nTries--;
                    if (nTries == 0)
                    {
                        throw e;
                    }
                }
                await System.Threading.Tasks.Task.Delay(REQUEST_PAUSE);
            }

            throw new ArgumentOutOfRangeException("REQUEST_PAUSE", "must be greater than 0"); //Cannot be reached 
        }

        private static HttpMethod MapHttpMethod(string methodName)
        {
            HttpMethod method;
            switch (methodName)
            {
                case "DELETE":
                    method = HttpMethod.Delete;
                    break;
                case "HEAD":
                    method = HttpMethod.Head;
                    break;
                case "OPTIONS":
                    method = HttpMethod.Options;
                    break;
                case "POST":
                    method = HttpMethod.Post;
                    break;
                case "PUT":
                    method = HttpMethod.Put;
                    break;
                case "TRACE":
                    method = HttpMethod.Trace;
                    break;
                case "GET":
                default:
                    method = HttpMethod.Get;
                    break;
            }
            return method;
        }

        /// <summary>
        /// Transform a path into an absolute Pivotal API URL
        /// </summary>
        /// <param name="path">Relative path to the REST API</param>
        /// <returns>the absolute URL</returns>
        private Uri GetPivotalURI(string path)
        {
            if (!String.IsNullOrWhiteSpace(path) && path[0] == '/') //Manage leading slash
                path = path.Remove(0, 1);

            return new Uri(new Uri(Token.BaseUrl), path);
        }


        /// <summary>
        /// Deserialize the content stream into T
        /// </summary>
        /// <typeparam name="T">Deserialized Type</typeparam>
        /// <param name="stream">Stream that contains the serialized object</param>
        /// <returns>deserialized object of type T</returns>
        protected T Deserialize<T>(Stream stream)
        {
            var lSerializer = new XmlSerializer(typeof(T));
            try
            {
                return (T)lSerializer.Deserialize(stream);
            }
            catch (WebException e)
            {
#if DEBUG
                using (var r = new StreamReader(e.Response.GetResponseStream()))
                {
                    // Console.WriteLine(r.ReadToEnd());
                }
#endif
                throw e;
            }
            catch (Exception e)
            {
#if DEBUG
                // Console.WriteLine(e.Message);
#endif
                throw e;
            }
        }

        


        //TODO: Bug in Pivotal : do not work (http://gsfn.us/t/26f14)
        /// <summary>
        /// Download a Pivotal Attachment
        /// </summary>
        /// <remarks>Bug in Pivotal : do not work (http://gsfn.us/t/26f14)</remarks>
        /// <param name="url">absolute URL to the attachment</param>
        /// <returns>data downloaded</returns>
        protected async Task<byte[]> RequestPivotalDownload(string url)
        {
            var lReq = new RestRequest(url);
            lReq.AddHeader("X-TrackerToken", Token.ApiKey);

            // FIXME: this implementation should cause file corruption. We don't touch this because the comment says this API doesn't work. Need to test later.
            return Encoding.UTF8.GetBytes(await this.ExecuteAsync<string>(lReq));
        }

        /// <summary>
        /// Helper method to transform an url into a acceptable url (not special cars, etc...)
        /// </summary>
        /// <param name="url">Url to sanitize</param>
        /// <returns>the sanitized Url</returns>
        static string UrlSanitize(string url)
        {
            // URI normalizer is not accessible in PCL. The only user of this method was RequestPivotalUpload<T>, it's removed temporarily, so we removed this method also.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Upload a file to Pivotal and rtrive the result object
        /// </summary>
        /// <typeparam name="T">Type of the serialized object in the Pivotal XML Response</typeparam>
        /// <param name="path">Relative path to the REST API (ex: /projects/PROJECT_ID/stories/STORY_ID/attachments)</param>
        /// <param name="data">data to upload</param>
        /// <param name="filename">filename of the uploaded data (will be the name that appear in Pivotal Story)</param>
        /// <param name="contentType">data content-type</param>
        /// <returns>Deserialized object from the Pivotal XML Response</returns>
        protected T RequestPivotalUpload<T>(string path, byte[] data, string filename="upload", string contentType="application/octet-stream")
        {
            // Uploading files have several problems. Removed support at the moment.
            throw new NotImplementedException();
        }

      
    }
}
