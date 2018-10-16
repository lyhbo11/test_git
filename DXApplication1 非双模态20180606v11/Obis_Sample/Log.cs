using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Obis_Sample.Nets;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Obis_New
{
    public partial class Log : DevExpress.XtraEditors.XtraForm
    {

        public string SERVER_URL = "http://192.168.0.139:8001/users/";
        public Boolean isUserExist = false;
        public string user_id;
        public string password;
        public Net Netinfo = new Net();

        public Log()
        {
            InitializeComponent();
        }

        public bool IsNumber(String strNumber)
        {
            Regex objNotNumberPattern = new Regex("[^0-9.-]");
            Regex objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
            Regex objTwoMinusPattern = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
            String strValidRealPattern = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
            String strValidIntegerPattern = "^([-]|[0-9])[0-9]*$";
            Regex objNumberPattern = new Regex("(" + strValidRealPattern + ")|(" + strValidIntegerPattern + ")");

            return !objNotNumberPattern.IsMatch(strNumber) &&
                !objTwoDotPattern.IsMatch(strNumber) &&
                !objTwoMinusPattern.IsMatch(strNumber) &&
                objNumberPattern.IsMatch(strNumber);
        }

        private string HttpRequestProcess(string user_id, string password, string URL)
        {
            string Reqtest = "";
            URL = URL + '?' + "user_id=" + user_id + '&' + "password=" + password + '&' + "Version=2.0";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "GET";
            request.KeepAlive = false;
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;
            Encoding encoding = Encoding.UTF8;
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, encoding);
                Reqtest = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
            finally
            {
                request = null;
            }
            return Reqtest;
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.user_id = textEdit1.Text;
            this.password = textEdit2.Text;
            Netinfo.user_id = user_id;
            Netinfo.password = password;
            //MessageBox.Show(SERVER_URL);
            String SERVER_URL_TEMP = SERVER_URL + "nurse_auth/";
            string Response = HttpRequestProcess(user_id, password, SERVER_URL_TEMP);
            if (!IsNumber(Response))
            {
                if (Response.Length != 0)
                {
                    MessageBox.Show("登陆成功");
                    isUserExist = true;
                    this.DialogResult = DialogResult.OK;
                    Netinfo.rec_result = Response;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("网络故障");
                    isUserExist = false;
                }
            }
            else
            {
                MessageBox.Show("登陆失败");
                isUserExist = false;
                textEdit1.Text = "";
                textEdit2.Text = "";
            }
            //if (checkEdit1.Checked==true)
            //{
            //    using (StreamWriter sr = new StreamWriter("ip.json"))
            //    {
            //        string js= JsonConvert.SerializeObject(Netinfo);
            //        sr.WriteLine(js);
            //    }
            //}
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void checkEdit1_CheckedChanged(object sender, EventArgs e)
        {
        }
        private void checkEdit2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkEdit2.Checked==true)
                if ((textEdit1.Text=="")&(textEdit2.Text==""))
                {
                textEdit1.Text = user_id;
                textEdit2.Text = password;
                }
        }

        private void Log_Load(object sender, EventArgs e)
        {
            string json_ip;
            using (StreamReader sr = new StreamReader("ip.json"))
            {
                json_ip = sr.ReadToEnd();
            }
            JObject IP_Json = (JObject)JsonConvert.DeserializeObject(json_ip);
            this.SERVER_URL = IP_Json["Connect_IP"].ToString();
            if (IP_Json["user_id"].ToString().Length!=0)
            {
                user_id = IP_Json["user_id"].ToString();
                password = IP_Json["password"].ToString();
            }
            Netinfo.user_id = user_id;
            Netinfo.password = password;
            Netinfo.Connect_IP = SERVER_URL;
        }
    }
}