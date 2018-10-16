using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Obis_Sample.Nets;
using Obis_Sample.Patients;

namespace Obis_Sample.Function
{
    public class NetFunction
    {
        public Net Net_info;
        public List<Patient> Patient_List;
        public Queue<Patient> UploadList;
        public List<Patient> Patiented_List;
        public int Upload_Signal = 0;
        public int Upload = 0;
        public string LogAddress = Environment.GetEnvironmentVariable("USERPROFILE") + "\\Documents\\orbis\\Log\\" + DateTime.Now.Month + '_' + DateTime.Now.Day + "_OrbisLog.log";

        private string HttpRequestProcess(string user_id, string password, string URL)
        {
            string Reqtest = "";
            URL = URL + '?' + "user_id=" + user_id + '&' + "password=" + password;
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

        public void List_Refresh()
        {
            List<Patient> List = new List<Patient>();
            try
            {
                System.Net.WebRequest wReq = System.Net.WebRequest.Create(Net_info.Connect_IP + "get_patient_list/?user_id=" + Net_info.user_id);
                wReq.Timeout = 2000;
                System.Net.WebResponse wResp = wReq.GetResponse();
                System.IO.Stream respStream = wResp.GetResponseStream();
                using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream, Encoding.GetEncoding("utf-8")))
                {
                    Net_info.rec_result = reader.ReadToEnd();
                    //MessageBox.Show("通信成功: " + rec_result, "错误提示");
                }
                JArray jf = (JArray)JsonConvert.DeserializeObject(Net_info.rec_result);
                bool Signal = true;
                for (int i = 0; i < jf.Count; i++)
                {
                    Patient Pi = new Patient();
                    Pi.Patient_name = jf[i]["patient_name"].ToString();
                    Pi.Patient_ID = jf[i]["patient_ID"].ToString();
                    Pi.Patient_sex = jf[i]["patient_sex"].ToString();
                    if ((Pi.Patient_sex == "0")||(Pi.Patient_sex == "男"))
                        Pi.Patient_sex = "男";
                    else
                        Pi.Patient_sex = "女";
                    Pi.Patient_age = jf[i]["patient_age"].ToString();
                    Signal = true;
                    foreach(Patient P in Patiented_List)
                    {
                        if (P.Patient_ID.Equals(Pi.Patient_ID))
                        {
                            Signal = false;
                            break;
                        }
                    }
                    if (Signal)
                        List.Add(Pi);
                }
                this.Patient_List = List;
            }
            catch(Exception ex)
            {
                MessageBox.Show("刷新失败，请检查网络", "错误提示");
                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                {
                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                    fs.WriteLine("异常类型：获取列表失败");
                    fs.WriteLine("异常信息:" + ex.Message);
                    fs.WriteLine("异常对象：" + ex.Source);
                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                    fs.WriteLine("触发方法：" + ex.TargetSite);
                    fs.WriteLine();
                    fs.Close();
                }
                return;
            }
        }

        public PatientsDetial getPatientsDetial(Patient Patient_Now)
        {
            PatientsDetial patientsDetial_Now = new PatientsDetial();
            try
            {
                PatientForJson patientForJson_Now = new PatientForJson();
                patientForJson_Now.patient_ID = Patient_Now.Patient_ID;
                patientForJson_Now.patient_name = Patient_Now.Patient_name;
                patientForJson_Now.patient_sex = Patient_Now.Patient_sex;
                patientForJson_Now.patient_age = Convert.ToDouble(Patient_Now.Patient_age);
                string json = JsonConvert.SerializeObject(patientForJson_Now);
                string result = "";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Net_info.Connect_IP + "get_patient_details/?user_id=" + Net_info.user_id);
                req.Method = "POST";
                req.ContentType = "text/html";

                #region 添加Post 参数  
                byte[] data = Encoding.UTF8.GetBytes(json);
                req.ContentLength = data.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
                #endregion

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                //获取响应内容  
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    //result = reader.ReadToEnd();
                    ////MessageBox.Show("通信成功: " + result, "错误提示");
                    //JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                    ////MessageBox.Show("通信成功: " + jo["internal_diseases"][0]["Is_Diagnosed"].ToString(), "错误提示");
                    //if (jo["personal_info"][0]["patient_name"].ToString() != null)
                    //    patientsDetial_Now.Patient_name = jo["personal_info"][0]["patient_name"].ToString();
                    //else
                    //    patientsDetial_Now.Patient_name = " ";

                    //if (jo["personal_info"][0]["patient_sex"].ToString() != null)
                    //    patientsDetial_Now.Patient_sex = jo["personal_info"][0]["patient_sex"].ToString();
                    //else
                    //    patientsDetial_Now.Patient_sex = " ";

                    //if (jo["personal_info"][0]["patient_age"].ToString() != null)
                    //    patientsDetial_Now.Patient_age = jo["personal_info"][0]["patient_age"].ToString();
                    //else
                    //    patientsDetial_Now.Patient_age = " ";
                    //jo["internal_diseases"][0]["TimeToGet_diabetes"] = "123";
                    //patientsDetial_Now.Patient_RightSight = jo["internal_diseases"][0]["Right_eyesight"].ToString();
                    //patientsDetial_Now.Patient_LeftSight = jo["internal_diseases"][0]["Left_eyesight"].ToString();
                    //patientsDetial_Now.Patient_Record = jo["internal_diseases"][0]["Recorder"].ToString();
                    //patientsDetial_Now.Patient_OtherDis = jo["internal_diseases"][0]["other_internal_diseases"].ToString();
                    //patientsDetial_Now.Timetoget = jo["internal_diseases"][0]["Recoding_date"].ToString();
                    //patientsDetial_Now.Patient_ID = Patient_Now.Patient_ID;
                    result = reader.ReadToEnd();
                    //MessageBox.Show("通信成功: " + result, "错误提示");
                    JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                    //MessageBox.Show("通信成功: " + jo["internal_diseases"][0]["Is_Diagnosed"].ToString(), "错误提示");
                    if (jo["personal_info"]["patient_name"].ToString() != null)
                        patientsDetial_Now.Patient_name = jo["personal_info"]["patient_name"].ToString();
                    else
                        patientsDetial_Now.Patient_name = " ";

                    if (jo["personal_info"]["patient_sex"].ToString() != null)
                    {
                        if (jo["personal_info"]["patient_sex"].ToString() == "0")
                            patientsDetial_Now.Patient_sex = "男";
                        else
                            patientsDetial_Now.Patient_sex = "女";
                    }
                    else
                        patientsDetial_Now.Patient_sex = " ";

                    if (jo["personal_info"]["patient_age"].ToString() != null)
                        patientsDetial_Now.Patient_age = jo["personal_info"]["patient_age"].ToString();
                    else
                        patientsDetial_Now.Patient_age = " ";
                    patientsDetial_Now.Patient_LeftSight = jo["internal_diseases"][0]["zyz"].ToString() + '/' + jo["internal_diseases"][0]["zyy"].ToString();
                    patientsDetial_Now.Patient_RightSight = jo["internal_diseases"][0]["yyz"].ToString() + '/' + jo["internal_diseases"][0]["yyy"].ToString();
                    patientsDetial_Now.Patient_Record = jo["internal_diseases"][0]["Recorder"].ToString();
                    patientsDetial_Now.Patient_OtherDis = jo["internal_diseases"][0]["other_internal_diseases"].ToString();
                    patientsDetial_Now.Timetoget = jo["internal_diseases"][0]["applydate"].ToString();
                    patientsDetial_Now.Patient_ID = Patient_Now.Patient_ID;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("通信失败", "错误提示");
                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                {
                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                    fs.WriteLine("异常类型：获取详细信息异常");
                    fs.WriteLine("异常信息:" + ex.Message);
                    fs.WriteLine("异常对象：" + ex.Source);
                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                    fs.WriteLine("触发方法：" + ex.TargetSite);
                    fs.WriteLine();
                    fs.Close();
                }
                return patientsDetial_Now;
            }

            return patientsDetial_Now;
        }

        public static int HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            int ERRORUPLOAD = 0;
            //log.Debug(string.Format("Uploading {0} to {1}", file, url));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                string re = reader2.ReadToEnd();

                if (uint.Parse(re) == 1)
                {
                    //MessageBox.Show(re, "上传成功");
                }
            }
            catch (Exception ex)
            {
                //log.Error("Error uploading file", ex);
                ERRORUPLOAD = 1;
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                string LogAddress = Environment.GetEnvironmentVariable("USERPROFILE") + "\\Documents\\orbis\\Log\\" + DateTime.Now.Month + '_' + DateTime.Now.Day + "_OrbisLog.log";
                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                {
                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                    fs.WriteLine("异常类型：图片上传时网络连接异常");
                    fs.WriteLine("异常信息:" + ex.Message);
                    fs.WriteLine("异常对象：" + ex.Source);
                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                    fs.WriteLine("触发方法：" + ex.TargetSite);
                    fs.WriteLine();
                    fs.Close();
                }
            }
            finally
            {
                wr = null;
            }
            return ERRORUPLOAD;
        }

        class MyException : ApplicationException
        {
            public MyException(string message) : base(message) { }
            public override string Message
            {
                get
                {
                    return base.Message;
                }
            }
        }

        public static bool PingIpOrDomainName(string strIpOrDName)
        {
            try
            {
                Ping objPingSender = new Ping();
                PingOptions objPinOptions = new PingOptions();
                objPinOptions.DontFragment = true;
                string data = "";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int intTimeout = 120;
                PingReply objPinReply = objPingSender.Send(strIpOrDName, intTimeout, buffer, objPinOptions);
                string strInfo = objPinReply.Status.ToString();
                if (strInfo == "Success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Uploading()
        {
            while (true)
            {
                if (UploadList.Count==0)
                {
                    System.Threading.Thread.Sleep(5000);
                    continue;
                }
                Patient UploadingPatient;
                int Net_ERROR = 0;
                {
                    Upload = 1;
                    UploadingPatient = this.UploadList.Peek();
                    string result = Environment.GetEnvironmentVariable("USERPROFILE") + "\\Documents\\orbis\\";
                    string dir = result + "Data\\" + UploadingPatient.Patient_ID;
                    string[] files = Directory.GetFiles(dir, "*.jpg");
                    for (int i = 0; i < files.Length; i++)
                    {
                        string text = files[i];
                        string[] text1 = text.Split('\\');
                        string name = text1[text1.Length - 1];
                        NameValueCollection nvc = new NameValueCollection();
                        nvc.Add("patient_ID", UploadingPatient.Patient_ID);
                        nvc.Add("image_name", Net_info.rec_result + '/' + name);
                        try
                        {
                            Net_ERROR = HttpUploadFile(Net_info.Connect_IP + "upload_picture/?user_id=" + Net_info.user_id, text, "patient_img", "image/jpeg", nvc);
                            if (Net_ERROR == 1)
                                throw new MyException("ERROR");
                            else
                                Net_ERROR = 0;
                        }
                        catch(Exception ex)
                        {
                            if (PingIpOrDomainName(Net_info.Connect_IP))
                            {
                                i--;
                                Net_ERROR = 0;
                                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                                {
                                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                                    fs.WriteLine("异常类型：图片上传失败");
                                    fs.WriteLine("异常信息:" + ex.Message);
                                    fs.WriteLine("异常对象：" + ex.Source);
                                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                                    fs.WriteLine("触发方法：" + ex.TargetSite);
                                    fs.WriteLine();
                                    fs.Close();
                                }
                                continue;
                            }
                            else
                            {
                                if (Net_ERROR == 0)
                                {
                                    MessageBox.Show("网路连接异常，稍后重试");
                                    i--;
                                    Net_ERROR = 1;
                                    using (StreamWriter fs = new StreamWriter(LogAddress, true))
                                    {
                                        fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                                        fs.WriteLine("异常类型：图片上传时网络连接异常");
                                        fs.WriteLine("异常信息:" + ex.Message);
                                        fs.WriteLine("异常对象：" + ex.Source);
                                        fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                                        fs.WriteLine("触发方法：" + ex.TargetSite);
                                        fs.WriteLine();
                                        fs.Close();
                                    }
                                    System.Threading.Thread.Sleep(5000);
                                    continue;
                                }
                            }
                        }

                    }
                    UploadingPatient=this.UploadList.Dequeue();
                    Upload = 0;
                    Upload_Signal = 0;
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        public void DeleteList(string PatientID)
        {
            string rec_result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Net_info.Connect_IP + "delete_patient/?patient_ID=" + PatientID + "&nurse_username=" + Net_info.user_id);
            req.Method = "GET";
            req.ContentType = "text/html";
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.GetEncoding("utf-8")))
                {
                    rec_result = reader.ReadToEnd();
                    //MessageBox.Show("通信成功: " + rec_result, "错误提示");
                }
            }
            catch(Exception ex)
            {
                using (StreamWriter fs = new StreamWriter(LogAddress, true))
                {
                    fs.WriteLine("当前时间：" + DateTime.Now.ToString());
                    fs.WriteLine("异常类型：删除列表失败");
                    fs.WriteLine("异常信息:" + ex.Message);
                    fs.WriteLine("异常对象：" + ex.Source);
                    fs.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
                    fs.WriteLine("触发方法：" + ex.TargetSite);
                    fs.WriteLine();
                    fs.Close();
                }
                return;
            }
        }

        public void Init()
        {
            Patiented_List = new List<Patient>();
            Patient_List = new List<Patient>();
            UploadList = new Queue<Patient>();

        }
     }
}

