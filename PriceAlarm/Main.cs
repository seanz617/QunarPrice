using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows.Forms;

using Newtonsoft.Json;

namespace PriceAlarm
{
    public partial class Main : Form
    {
        private string root=AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 参数
        /// </summary>
        private Options option=null;

        /// <summary>
        /// 票的缓存信息
        /// </summary>
        private Dictionary<string,Piao> dic=new Dictionary<string, Piao>();

        /// <summary>
        /// 小时增量
        /// </summary>
        private Dictionary<string,int> increase=new Dictionary<string, int>();

        public Main()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            //加载配置
            string json=File.ReadAllText(root+"option.json");
            option = JsonConvert.DeserializeObject<Options>(json);
            //加载缓存
            json = File.ReadAllText(root + "cache.json");
            dic = JsonConvert.DeserializeObject<Dictionary<string,Piao>>(json);
            increase.Clear();
            foreach(string key in dic.Keys)
            {
                increase.Add(key, 0);
            }
            //同步票的数据
            foreach(string str in option.TicketNames)
            {
                if(dic.ContainsKey(str))
                {
                    continue;
                }
                Piao p=new Piao();
                p.Name = str;
                p.Price = 0;
                p.Count = 0;
                p.Time = DateTime.Now;
                dic.Add(str, p);
                increase.Add(str, 0);
            }

            SaveCache();

            ShowData(false);

            tmr_Ticket.Enabled = true;
        }












        /// <summary>
        /// 获取Json数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        private string GetJson(string url, string referer)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Referer = referer;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Method = "POST";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //从网络资源中返回数据流
            Stream stream = response.GetResponseStream();

            StreamReader sr = new StreamReader(stream, Encoding.UTF8);

            //将数据流转换文字符串
            string result = sr.ReadToEnd();

            //关闭流数据
            stream.Close();
            sr.Close();

            return result;
        }

        /// <summary>
        /// 取得一页的票信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        private QunarResult GetPage(string url, int page)
        {
            int pageSize=50;
            string par="anonymous/shop.json?keyword=&method=queryProductList&pageNo={0}&perPageSize="+pageSize;
            string all=String.Format(url+par,page);
            string json =GetJson(all,url);
            QunarResult qr0=JsonConvert.DeserializeObject<QunarResult>(json);
            if(qr0.data.products.Length == pageSize)
            {
                QunarResult qr1=GetPage(url, page+1);
                List<Product> list  =new List<Product>();
                foreach(Product pro in qr0.data.products)
                {
                    list.Add(pro);
                }
                foreach(Product pro in qr1.data.products)
                {
                    list.Add(pro);
                }
                qr0.data.products = list.ToArray();
            }
            return qr0;
        }

        private void ShowData(bool update)
        {
            //添加行
            if(update==false)
            {
                dgv_Piao.Rows.Clear();
                foreach(Piao p in dic.Values)
                {
                    int index=dgv_Piao.Rows.Add();
                    dgv_Piao.Rows[index].Cells[0].Value = p.Name;
                    dgv_Piao.Rows[index].Tag = p;
                }
            }

            //添加数据
            foreach(DataGridViewRow dgvr in dgv_Piao.Rows)
            {
                Piao p=dgvr.Tag as Piao;
                if(p==null)
                {
                    dgvr.Cells[0].Style.BackColor = Color.Red;
                    continue;
                }
                dgvr.Cells[1].Value = p.Price.ToString("0.00");
                dgvr.Cells[2].Value = p.Count.ToString();
                dgvr.Cells[3].Value = increase[p.Name].ToString();
            }
            
        }

        private void SendMessage(string msg)
        {
            Console.WriteLine(msg);
        }



        private void tmr_Ticket_Tick(object sender, EventArgs e)
        {
            DateTime dt=DateTime.Now;
            if(!(dt.Second==0&&dt.Minute%3==0))
            {
                return;
            }
            QunarResult qr=GetPage(option.DianUrl,1);
            foreach(Product p in qr.data.products)
            {
                if(!dic.ContainsKey(p.productName))
                {
                    continue;
                }

                if(p.qunarPrice > dic[p.productName].Price)
                {
                    SendMessage(Zhang(dic[p.productName], p.qunarPrice));
                }
                if(p.qunarPrice<dic[p.productName].Price)
                {
                    SendMessage(Jiang(dic[p.productName], p.qunarPrice));
                }
                dic[p.productName].Price = p.qunarPrice;
                dic[p.productName].Count = p.sellCount;
            }
            SaveCache();
            ShowData(true);
        }

        private string Zhang(Piao p, float price)
        {
            return p.Name + "涨价了 " + price.ToString("0.00");
        }

        private string Jiang(Piao p, float price)
        {
            return p.Name + "降价了 " + price.ToString("0.00");

        }

        private void SaveCache()
        {
            string json = JsonConvert.SerializeObject(dic);
            File.WriteAllText(root + "cache.json", json, Encoding.UTF8);
        }
    }
}
