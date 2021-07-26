using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PriceAlarm
{
    public class QunarResult
    {
        public bool ret;
        public Data data;
    }

    public class Data
    {
        public int totalRows;
        public string[] containsCites;
        public string[] containsThemes;
        public Product[] products;
    }

    public class Product
    {
        public string productId;
        public string city;
        public string sightName;
        public string productName;
        public int sellCount;
        public float qunarPrice;
        public float marketPrice;
        public string productImg;
        public string[] themes;
        public bool tuan;
        public bool cashBack;
        public bool passGuarantee;
    }
}
