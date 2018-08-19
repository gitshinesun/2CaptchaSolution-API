using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CaptchaSolve
{
    public partial class Main
    {
        //2captcha.com
		string api_key = "12345678901234567890123456789012";
		string api_url = "http://2captcha.com/in.php";

        private void SolveCaptcha()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
			
			string filename = open_browser();
            main_engine(filename);
        }

        public async void main_engine(string filename)
        {
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();
            System.Net.ServicePointManager.Expect100Continue = false;
            
            byte[] fileBytes = File.ReadAllBytes(filename);
            string base64string = Convert.ToBase64String(fileBytes);

            form.Add(new StringContent("base64"), "method");
            form.Add(new StringContent(api_key), "key");
            form.Add(new StringContent(base64string), "body");
            
            HttpResponseMessage response = await httpClient.PostAsync(api_url, form);

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            string sd = response.Content.ReadAsStringAsync().Result;
            string str_answer = string.Empty;

            //OK|60367926990
            if (sd.IndexOf("OK") >= 0)
            {
                string[] arr_status = sd.Split('|');
                string str_id = arr_status[1];
                string str_res = string.Empty;
                do
                {
                    str_res = get_resource(api_key, str_id);
                    if (str_res.CompareTo("CAPCHA_NOT_READY") != 0)
                    {
                        if (str_res.CompareTo("OK") >= 0)
                        {
                            string[] arr_result = str_res.Split('|');
                            str_answer = arr_result[1];
                        }
                        else
                        {
                            str_answer = "error";
                        }

                        break;
                    }
                    Thread.Sleep(1000);
                }
                while (true);
            }

            Console.WriteLine(str_answer);
        }

        private string get_resource(string strkey, string strid)
        {
            string str_res;

            string str_url = string.Format("http://2captcha.com/res.php?key={0}&action=get&id={1}", strkey, strid);

            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(str_url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        str_res = content.ReadAsStringAsync().Result;

                        Console.WriteLine(str_res);
                    }
                }
            }

            return str_res;
        }

        private string open_browser()
        {
            using (var selectFileDialog = new OpenFileDialog())
            {
                selectFileDialog.Filter = "Image files|*.bmp;*.jpg";
                selectFileDialog.Title = "Output file...";
                if (selectFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return selectFileDialog.FileName;
                }
            }
        }
    }
}
