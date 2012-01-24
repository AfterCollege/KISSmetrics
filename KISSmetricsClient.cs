using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Policy;
using System.Net;
using log4net;
using System.Net.Cache;

namespace AfterCollege.KISSmetrics
{
    public class KISSmetricsException : Exception{
        public KISSmetricsException(string message) : base(message) { }
        public KISSmetricsException() : base() { }
        public KISSmetricsException(Exception ex):base(ex.Message, ex){}
        public KISSmetricsException(string message, Exception ex) : base(message, ex) { }
    }

    public class MissingIdentityException : KISSmetricsException
    {

    }        

    public class KISSmetricsClient
    {
        private string apiKey;
        private string identity;
        private ILog log = LogManager.GetLogger(typeof(KISSmetricsClient));

        public KISSmetricsClient(string apiKey)
        {
            this.apiKey = apiKey;
            log.Info("KISS metrics client created successfully.");
        }

        public KISSmetricsClient()
        {
            this.apiKey = AfterCollege.KISSmetrics.Properties.Settings.Default.ApiKey;
            if (String.IsNullOrEmpty(this.apiKey))
            {
                log.Warn("API Key not found.");
                log.Debug("Modify API Key in App.config");
                throw new KISSmetricsException("API Key not found.");
            }
            log.Info("KISS metrics client created successfully.");
        }

        public void SetIdentity(string identity)
        {
            log.Info("Setting identiy");
            log.Debug(identity);
            this.identity = identity;
        }

        public void Alias(string identity1 , string identity2){
            Dictionary<String, String> data = new Dictionary<String, String>();
            data.Add("_p", identity1);
            data.Add("_n", identity2);

            try {                
                string url = BuildUrl("a", data);
                log.Debug(url);
                SendRequest(url);
            } catch (Exception ex) {            
                throw new KISSmetricsException(ex);
            }
        }

        public void Record(string name)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            Record(name, data);
        }

        public void Record(string name, Dictionary<string, string> data)
        {
            log.Info("Record is triggered");

            if (identity == null)
                throw new MissingIdentityException();

            data.Add("_p", identity);
            data.Add("_n", name);

            try
            {
                string url = BuildUrl("e", data);
                log.Debug(url);
                SendRequest(url);
            }
            catch (Exception ex)
            {
                log.Warn(ex);
                throw new KISSmetricsException(ex);

            }
        }

        private string BuildUrl(string action, Dictionary<string, string> data){
            
            bool isFirst = true;
            
            StringBuilder sb = new StringBuilder();
            foreach (string key in data.Keys){
                log.Debug(String.Format("{0} {1}", key, data[key]));

                if(!isFirst)
                    sb.Append("&");

                sb.Append(System.Web.HttpUtility.UrlEncode(key) + "=" + System.Web.HttpUtility.UrlEncode(data[key]));               
                
                isFirst = false;
            }
            string retVal = "http://trk.kissmetrics.com/" + action +"?_k=" + apiKey + "&" + sb.ToString();
            log.Debug(retVal);
            return retVal;            
        }

        /// <summary>
        /// set method lets you set properties on a person without recording an event.
        /// </summary>
        /// <param name="data"></param>
        public void Set(Dictionary<string, string> data){
            if(identity == null){
                throw new MissingIdentityException();
            }

            data.Add("_p", identity);

            try
            {
                string url = BuildUrl("s", data);
                log.Debug(url);
                SendRequest(url);
            }
            catch (Exception ex)
            {
                throw new KISSmetricsException(ex);
            }
        }

        private void SendRequest(string urlForAction){
            log.Debug("Sending a request");
            WebClient client = new WebClient();
            client.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.BypassCache);
            client.DownloadData(urlForAction);           

            log.Debug("Finished sending the request");
        }
    }
}
