using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace employee_log_time
{   
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void log_in_Click(object sender, EventArgs e)
        {
            email.Text = "kimbriel_oraya@yahoo.com";
            password.Text = "340561497-0000";
            var json = JsonConvert.SerializeObject("");//, Formatting.Indented
            send_data();
        }

        private void register_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }
        public void send_data()
        {
            try
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                var postData = "email=" + email.Text.ToString();
                postData += "&password=" + password.Text.ToString();
                
                byte[] data = encoding.GetBytes(postData);

                WebRequest request = (HttpWebRequest)WebRequest.Create("http://payrolldigima.com/member/payroll/biometrics/employee_login");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                Stream stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                WebResponse response = request.GetResponse();
                stream = response.GetResponseStream();

                StreamReader sr = new StreamReader(stream);
                string employee_number = sr.ReadToEnd();

                if (employee_number == "")
                {
                    MessageBox.Show("Login Failed: Wrong email or password");
                }
                else
                {
                    data_holder.payroll_employee_id = employee_number;
                    Form3 f3 = new Form3();
                    this.Hide();
                    f3.Show();
                }
                
            }

            catch (Exception ex)
            {
                MessageBox.Show("Failed to import data\n" + ex.Message);
            }
        }
    }
}
