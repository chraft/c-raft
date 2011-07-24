using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Chraft.Utils
{
    public static class Http
    {
        public static string GetHttpResponse(Uri requestUrl)
        {
            return GetHttpResponse(requestUrl, null);
        }

        public static string GetHttpResponse(Uri requestUrl, byte[] postData)
        {
            // declare objects
            string responseData = String.Empty;
            HttpWebRequest req = null;
            HttpWebResponse resp = null;
            StreamReader strmReader = null;

            try
            {
                req = (HttpWebRequest)HttpWebRequest.Create(requestUrl);


                // set HttpWebRequest properties here (Method, ContentType, etc)
                // some code

                // in case of POST you need to post data
                if ((postData != null) && (postData.Length > 0))
                {
                    using (Stream strm = req.GetRequestStream())
                    {
                        strm.Write(postData, 0, postData.Length);
                    }
                }

                resp = (HttpWebResponse)req.GetResponse();
                strmReader = new StreamReader(resp.GetResponseStream());
                responseData = strmReader.ReadToEnd().Trim();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (req != null)
                {
                    req = null;
                }

                if (resp != null)
                {
                    resp.Close();
                    resp = null;
                }
            }

            return responseData;
        }
    }
}
