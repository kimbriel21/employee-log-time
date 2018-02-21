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

namespace employee_log_time
{
    public partial class Form3 : Form
    {

        //SqlConnection cn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\kimbriel\Documents\Visual Studio 2012\Projects\employee_log_time\employee_log_time\employee_timesheet_db.mdf;Integrated Security=True");
        SqlConnection cn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\employee_timesheet_db.mdf;Integrated Security=True");
        SqlCommand cmd = new SqlCommand();
        SqlDataReader dr;

        public Form3()
        {
            InitializeComponent();
            cmd.Connection = cn;
            fill_timesheet_log();
            
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void Form3_Load(object sender, EventArgs e)
        {
            
        }

        private void search_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (check_time_in_out("time_in")) // check if status still time in
                {
                    DateTime time = DateTime.Now;
                    string formatDate = "MM-dd-yyyy";
                    string formatTime = "HH:mm:ss";
                    
                    cn.Open();
                    cmd.CommandText = "insert into employee_timesheet (employee_project_id, payroll_employee_id, time_in, date) values ('0','1','" + time.ToString(formatTime) + "', '" + time.ToString(formatDate) + "')";
                    cmd.ExecuteNonQuery();
                    cmd.Clone();
                    cn.Close();
                    fill_timesheet_log();
                }
                else
                {
                    MessageBox.Show("You're status still time in please time out first");
                }
                
                //truncate_table_timesheet();
                
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
                    DateTime time = DateTime.Now;
                    string formatTime = "HH:mm:ss";
                    int employee_timesheet_id = get_last_time_sheet_data_id();
                    cn.Open();
                    cmd.CommandText = "update employee_timesheet set time_out='" + time.ToString(formatTime) + "' where employee_timesheet_id='" + employee_timesheet_id + "'";
                    cmd.ExecuteNonQuery();
                    cmd.Clone();
                    cn.Close();
                    fill_timesheet_log();
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
            SqlCommand cmd2 = new SqlCommand("select employee_timesheet_id as '#',time_in as 'Time In', date as 'Date' from employee_timesheet");
            cmd2.Connection = cn2;
            cn.Open();
            SqlDataAdapter sda = new SqlDataAdapter();
            
            sda.SelectCommand = cmd2;
            DataTable dt = new DataTable();
            sda.Fill(dt);
            BindingSource bs = new BindingSource();

            bs.DataSource = dt;
            timesheet_log_grid_view.DataSource = bs;
            sda.Update(dt);
            cn.Close();



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


        private void sync_data()
        {

        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        
    }
}
