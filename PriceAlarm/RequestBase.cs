using System;
using System.IO;
using System.Net;
using System.Text;

namespace PS.AS.ASIDE
{
    /// <summary>
    /// 做HTTP请求时所用的基础类,默认http://127.0.0.1:80/api/
    /// </summary>
    public class RequestBase
    {
        private string _baseURL = "http://127.0.0.1:80/api/";
        /// <summary>
        /// 客户端的前缀连接
        /// </summary>
        public string BaseURL
        {
            get
            {
                return _baseURL;
            }

            set
            {
                _baseURL = value;
            }
        }
        private Encoding _encoder = Encoding.UTF8;
        /// <summary>
        /// 做数据解析时所用的解码器，默认UTF-8
        /// </summary>
        public Encoding Encoder
        {
            get
            {
                return _encoder;
            }

            set
            {
                _encoder = value;
            }
        }


        /// <summary>
        /// Get方法的调用
        /// </summary>
        /// <param name="uri">调用的连接，需要由外部处理增加</param>
        /// <returns></returns>
        protected string GetContent(string uri)
        {
            //Get请求中请求参数等直接拼接在url中
            WebRequest request = WebRequest.Create(uri);
            request.Headers.
            WebResponse resp = null;
            try
            {
                //返回对Internet请求的响应
                resp = request.GetResponse();
            }
            catch(WebException ex)
            {
                return null;
            }
            //从网络资源中返回数据流
            Stream stream = resp.GetResponseStream();

            StreamReader sr = new StreamReader(stream, _encoder);

            //将数据流转换文字符串
            string result = sr.ReadToEnd();

            //关闭流数据
            stream.Close();
            sr.Close();

            return result;
        }

        protected string GetContentSample(string method)
        {
            string uri = _baseURL + method;
            return GetContent(uri);
        }

        /// <summary>
        /// 采用全路径Web地址的Post方法调用
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        protected string PostData(string uri, string json)
        {
            //Get请求中请求参数等直接拼接在url中
            WebRequest request = WebRequest.Create(uri);
            //request.Headers.Add("Content-Type:application/json");
            request.ContentType = "application/json;charset=utf-8";
            request.Method = "POST";
            byte[] code = _encoder.GetBytes(json);
            request.ContentLength = code.Length;
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(code, 0, code.Length);

            //返回对Internet请求的响应
            WebResponse resp = request.GetResponse();

            //从网络资源中返回数据流
            Stream stream = resp.GetResponseStream();

            StreamReader sr = new StreamReader(stream, _encoder);

            //将数据流转换文字符串
            string result = sr.ReadToEnd();

            reqStream.Close();
            //关闭流数据
            stream.Close();
            sr.Close();

            return result;
        }

        /// <summary>
        /// 采用默认Web地址的Post方法调用
        /// </summary>
        /// <param name="method"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        protected string PostDataSample(string method, string json)
        {
            string uri = _baseURL + method;
            return PostData(uri, json);
        }
    }
}
