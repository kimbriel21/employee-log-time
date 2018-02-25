using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

using System.Globalization;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace employee_log_time
{
    public partial class Form3 : Form
    {

        //SqlConnection cn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\kimbriel\Documents\Visual Studio 2012\Projects\employee_log_time\employee_log_time\employee_timesheet_db.mdf;Integrated Security=True");
        SqlConnection cn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\employee_timesheet_db.mdf;Integrated Security=True");
        SqlCommand cmd = new SqlCommand();
        SqlDataReader dr;
        Timer _tmr = new Timer();
       
        static Timer _idle_timer = new Timer();
        string dateFrom, dateTo;

        /*Start For Idle User*/
        [DllImport("User32.dll")]

        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        internal struct LASTINPUTINFO
        {
            public uint cbSize;

            public uint dwTime;
        }

        private uint GetLastInputTime(int secondsIdle)
        {
            uint idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            uint envTicks = (uint)Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint lastInputTick = lastInputInfo.dwTime;
                idleTime = envTicks - lastInputTick;
            }
            //millisecond->second
            // idleTime = (milliseconds/1000)*60
            double movement_idle = idleTime / 1000;
            //5 = 5 seconds;

            if (movement_idle > secondsIdle)
            {
                return 1;
            }

            return 0;
        }

        public void startclock()
        {
            _idle_timer.Interval = 1000;
            _idle_timer.Tick += new EventHandler(idle_timer_Tick);
            _idle_timer.Start();
            
        }

        public static void stopclock()
        {
            _idle_timer.Enabled = false;
            _idle_timer.Stop();
        }

        private void idle_timer_Tick(object sender, System.EventArgs e)
        {
            int seconds_idle = 600;
            uint check = GetLastInputTime(seconds_idle);

            if (check == 1)
            {
                btn_time_in.Text = "START TIME";
                statusLabel.Text = "TIME NOT RUNNING";
                backgroundStatus.BackColor = Color.Red;
                btn_send_data.Enabled = true;
            }
        }
        /*End For Idle User*/

        public Form3()
        {
            InitializeComponent();
            cmd.Connection = cn;
            DateTime time = DateTime.Now;
            dateFrom = time.ToString("MM/dd/yyyy");

            dateTo = time.ToString("MM/dd/yyyy");
            fill_timesheet_log();
            label_time_spent.Text = today_time_spent() ;
            labelSearchTimeSpent.Text = seach_time_spent(dateFrom, dateTo);

        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void Form3_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(this.btn_time_in, "Start And Stop Timer");
            ToolTip1.SetToolTip(this.btn_send_data, "Upload all data to the payroll system");
            ToolTip1.SetToolTip(this.btn_log_out, "Log out to this application");
            ToolTip1.SetToolTip(this.search, "Show Timesheet Data In Selected Date");
        }

        private void search_Click(object sender, EventArgs e)
        {
            dateFrom = datePickerFrom.Text.ToString();
            dateTo = datePickerTo.Text.ToString();
            fill_timesheet_log();
            labelSearchTimeSpent.Text = seach_time_spent(dateFrom, dateTo);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(1000, "TIMER STOPPED", "Your are idle in very long time, timer automatically stop", ToolTipIcon.Info);
            try
            {
                if (check_time_in_out("time_in")) // check if status still time in
                {
                    if (btn_time_in.Text.ToString() == "START TIME")
                    {
                        time_in();
                        fill_timesheet_log();
                        timerStart();
                        btn_time_in.Text = "STOP TIME";
                        statusLabel.Text = "TIME RUNNING...";
                        backgroundStatus.BackColor = Color.Green;
                        btn_send_data.Enabled = false;
                        startclock();
                    }
                    else
                    {
                        
                        timerStop();
                        btn_time_in.Text = "START TIME";
                        statusLabel.Text = "TIME NOT RUNNING";
                        backgroundStatus.BackColor = Color.Red;
                        btn_send_data.Enabled = true;
                       
                    }
                }
                else
                {
                    MessageBox.Show("You're status still time in please time out first");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex+"");
            }
        }

        private void btn_time_out_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(check_time_in_out("time_out"))) // check if status still time in
                {
                    time_out();
                    fill_timesheet_log();
                    timerStop();
                }
                else
                {
                    MessageBox.Show("You're status still time out please time in first");
                }

                //truncate_table_timesheet();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + "");
            }
        }

        private void btn_send_data_Click(object sender, EventArgs e)
        {

        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void btn_log_out_Click(object sender, EventArgs e)
        {
            timerStop();
            Form1 f1 = new Form1();
            f1.Show();
            this.Hide();
        }

        private void send_data_Click(object sender, EventArgs e)
        {
            string json_timesheet = convert_timesheet_datatable_to_json();
            send_time_sheet(json_timesheet);
           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            time_out();
            fill_timesheet_log();
            label_time_spent.Text = today_time_spent();
        }

        private string today_time_spent()
        {
            string _return = "00:00";
            DateTime time = DateTime.Now;
            string formatDate = "MM/dd/yyyy";
            DateTime dt1;
            DateTime dt2;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0);
            cn.Open();
            string query = "SELECT * FROM employee_timesheet where date = '" + time.ToString(formatDate) + "'";
            cmd.CommandText = query;
            dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                dt1 = DateTime.ParseExact(dr[3] + "", "HH:mm:ss", CultureInfo.InvariantCulture);
                dt2 = DateTime.ParseExact(dr[4] + "", "HH:mm:ss", CultureInfo.InvariantCulture);
                TimeSpan temp = dt2.Subtract(dt1);
                ts = ts + temp;
                _return = ts + "";
            }

            cmd.Clone();
            cn.Close();

            return Convert.ToDateTime(_return).ToString("HH:mm") + " HOUR/S";
        }

        private string getDateTimeServer()
        {
            string _return = "";
            try
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                var postData = "payroll_employee_id=" + data_holder.payroll_employee_id;
                byte[] data = encoding.GetBytes(postData);

                WebRequest request = (HttpWebRequest)WebRequest.Create("http://digimahouse.test/member/payroll/biometrics/get_time");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                Stream stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                WebResponse response = request.GetResponse();
                stream = response.GetResponseStream();

                StreamReader sr = new StreamReader(stream);
                string response_output = sr.ReadToEnd();

                _return = response_output;

            }

            catch (Exception ex)
            {
                MessageBox.Show("Failed to import data\n" + ex.Message);
            }
            return _return;
        }

        private string seach_time_spent(string From, string To)
        {
            string _return = "00:00";
            DateTime time = DateTime.Now;
           
            DateTime dt1;
            DateTime dt2;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0);
            cn.Open();
            string query = "SELECT * FROM employee_timesheet where date between '" + From + "' and '" + To + "'";
            cmd.CommandText = query;
            dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                dt1 = DateTime.ParseExact(dr[3] + "", "HH:mm:ss", CultureInfo.InvariantCulture);
                dt2 = DateTime.ParseExact(dr[4] + "", "HH:mm:ss", CultureInfo.InvariantCulture);
                TimeSpan temp = dt2.Subtract(dt1);
                ts = ts + temp;
                _return = ts + "";
            }

            cmd.Clone();
            cn.Close();

            return Convert.ToDateTime(_return).ToString("HH:mm") + " HOUR/S";
        }

        private int get_last_time_sheet_data_id()
        {
            int return_val = 0;
            cn.Open();
            string query = "SELECT TOP 1 * FROM employee_timesheet ORDER BY employee_timesheet_id DESC";
            cmd.CommandText = query;
            dr = cmd.ExecuteReader();
            if (dr.Read()) //chect if have data
            {
                if (!(string.IsNullOrEmpty(dr[0].ToString()))) //check if has time out
                {
                    return_val = Int32.Parse(dr[0].ToString());
                }
            }
            cmd.Clone();
            cn.Close();

            return return_val;
        }

        private bool check_time_in_out(string action)
        {
            bool return_val = true;
            cn.Open();
            string query = "SELECT TOP 1 * FROM employee_timesheet ORDER BY employee_timesheet_id DESC";
            cmd.CommandText = query;
            dr = cmd.ExecuteReader();
            if (dr.Read()) //chect if have data
            {
                if (string.IsNullOrEmpty(dr[4].ToString())) //check if has time out
                {
                    return_val = false;
                }
            }

            if(action != "time_in")
            {
                
                if (dr.Read()) //chect if have data
                {
                    if (string.IsNullOrEmpty(dr[4].ToString())) //check if has time out
                    {
                        return_val = false;
                    }
                    
                    
                }
            }
            
            cmd.Clone();
            cn.Close();

            return return_val;
        }

        private void fill_timesheet_log()
        {
            //SqlConnection cn2 = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\kimbriel\Documents\Visual Studio 2012\Projects\employee_log_time\employee_log_time\employee_timesheet_db.mdf;Integrated Security=True");
            SqlConnection cn2 = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\employee_timesheet_db.mdf;Integrated Security=True");
            SqlCommand cmd2 = new SqlCommand("select employee_timesheet_id as '#',time_in as 'Time In', time_out as 'Time Out', date as 'Date', status from employee_timesheet where date between '" + dateFrom + "' and '" + dateTo + "'");
            cmd2.Connection = cn2;
            cn2.Open();
            SqlDataAdapter sda = new SqlDataAdapter();
            
            sda.SelectCommand = cmd2;
            DataTable dt = new DataTable();
            sda.Fill(dt);
            BindingSource bs = new BindingSource();

            bs.DataSource = dt;
            timesheet_log_grid_view.DataSource = bs;
            sda.Update(dt);
            cn2.Close();
        }

        private void truncate_table_timesheet()
        {
            cn.Open();
            string query = "TRUNCATE TABLE employee_timesheet";
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
            cmd.Clone();
            cn.Close();
        }

        private void time_in()
        {
            string [] dateTime = getDateTimeServer().Split(' ');
            string formatDate = dateTime[0];
            string formatTime = dateTime[1];

            cn.Open();
            cmd.CommandText = "insert into employee_timesheet (employee_project_id, payroll_employee_id, time_in, time_out, date) values ('0','" + data_holder.payroll_employee_id + "','" + formatTime + "','" + formatTime + "', '" + formatDate + "')";
            cmd.ExecuteNonQuery();
            cmd.Clone();
            cn.Close();
        }

        private void time_out()
        {
            string lastTimeOut = "";
            cn.Open();
            string query = "SELECT TOP 1 * FROM employee_timesheet ORDER BY employee_timesheet_id DESC";
            cmd.CommandText = query;
            dr = cmd.ExecuteReader();
            if (dr.Read()) //chect if have data
            {
                if (!(string.IsNullOrEmpty(dr[0].ToString()))) //check if has time out
                {
                    lastTimeOut = dr[4].ToString();
                }
            }
            cmd.Clone();
            cn.Close();
            TimeSpan timeOut = (TimeSpan.Parse(lastTimeOut) + new TimeSpan(0,0,1,0));
            int employee_timesheet_id = get_last_time_sheet_data_id();
            cn.Open();
            cmd.CommandText = "update employee_timesheet set time_out='" + timeOut + "' where employee_timesheet_id='" + employee_timesheet_id + "'";
            cmd.ExecuteNonQuery();
            cmd.Clone();
            cn.Close();
        }

        private void sync_data()
        {

        }

        private void timerStop()
        {
            _tmr.Enabled = false;
            _tmr.Stop();
            
            
        }

        private void timerStart()
        {
            _tmr.Interval = 60000;  // interval in millisecond
            _tmr.Tick += new EventHandler(timer1_Tick);
            _tmr.Start();           
        }

        private string convert_timesheet_datatable_to_json()
        {
            SqlConnection cn2 = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\employee_timesheet_db.mdf;Integrated Security=True");
            SqlCommand cmd2 = new SqlCommand("select payroll_employee_id ,time_in , time_out , date from employee_timesheet where status = '0'");
            cmd2.Connection = cn2;
            cn.Open();
            SqlDataAdapter sda = new SqlDataAdapter();

            sda.SelectCommand = cmd2;
            DataTable dt = new DataTable();
            sda.Fill(dt);
            cn.Close();
            var json_timesheet = JsonConvert.SerializeObject(dt);
            
            return json_timesheet;
        }

        public void send_time_sheet(string timesheet_json)
        {
            try
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                var postData = "timesheet_data=" + timesheet_json;
                postData += "&payroll_employee_id=" + data_holder.payroll_employee_id;
                byte[] data = encoding.GetBytes(postData);

                WebRequest request = (HttpWebRequest)WebRequest.Create("http://digimahouse.test/member/payroll/biometrics/flexi_data_importation");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                Stream stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                WebResponse response = request.GetResponse();
                stream = response.GetResponseStream();

                StreamReader sr = new StreamReader(stream);
                string response_output = sr.ReadToEnd();

                MessageBox.Show(response_output);

                updateStatusSuccessImportData();
                fill_timesheet_log();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Failed to import data\n" + ex.Message);
            }
        }

        public void updateStatusSuccessImportData()
        {
            ArrayList myArryList = new ArrayList();
            cn.Open();
            string query = "SELECT * FROM employee_timesheet where status = '0'";
            cmd.CommandText = query;
            dr = cmd.ExecuteReader();

            while(dr.Read()) //chect if have data
            {
                myArryList.Add(dr[0]);
            }

            cmd.Clone();
            cn.Close();

            cn.Open();
            foreach(int id in myArryList)
            {
                cmd.CommandText = "update employee_timesheet set status = '1' where employee_timesheet_id='" + id + "'";
                cmd.ExecuteNonQuery();
            }
            cmd.Clone();
            cn.Close();
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        
        
    }
}
