using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using printer;
using MySql.Data.MySqlClient;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Media;
using System.Threading;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Diagnostics;
using System.Media;
using System.IO;
//  
namespace My_project
{
    public partial class Form1 : Form
    {
        #region 变量
        Thread thread;
        private string database_ip;
        private string database_user;
        private string database_pass;
        private string database_name_mysqlStr_manufacture;
        private string database_name_mysqlStr_management;
        private string database_name_mysqlStr_produce;
        /// <summary>数据库为manufacture,生产</summary>
        private string mysqlStr_manufacture;
        /// <summary>数据库为management，管理</summary>
        private string mysqlStr_management;
        /// <summary>数据库为produce，产品</summary>
        private string mysqlStr_produce;
        /// <summary>操作员名字</summary>
        public string Operator_id;
        /// <summary>订单号</summary>
        public string Order_id;
        /// <summary>合同号</summary>
        public string Contract_id;
        /// <summary>组件号</summary>
        public string Sec_id;
        /// <summary>部件号</summary>
        public string Part_id;
        /// <summary>打包工单号</summary>
        public string Ap_id;
        DataTable dt_all_element;
        /// <summary>判断是否登入</summary>
        Boolean is_on = false;
        /// <summary>判断是否为新的包</summary>
        Boolean is_newpackage = true;
        /// <summary>判断是否为新的订单</summary>
        Boolean is_Order_new = true;
        /// <summary>判断是否全部完成</summary>
        Boolean is_all_done;
        private string Ap_id_new;
        private string job_id;
        private string Package_num;
        private string Order_id_old;

        private string count_contract_num;
        private string count_state_num;
        private string count_order_state_num;
        private string count_order_num;
        int Window_width, Window_hight;
        private string Password;
        private object t;
        private object kt;
        private string job_id_old;
        
        private string string_check_day_info = "`Create_time`>'" + DateTime.Now.ToString("yyyy-MM-") + (DateTime.Now.Day - Settings1.Default.Check_days).ToString() + " 00:00:00' AND `Create_time`<'" + DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59'";
        private string cancel_code;
        private bool is_last_one;
        private string Re_pack_Order_id;

        public string Re_pack_Pack_id { get; private set; }
        #endregion
        public Form1()
        {
            InitializeComponent();
            Init_DB();
            Init_Windows();
            // thread = new Thread(new ThreadStart(Thread1));//线程
            // thread.Start();
          
        }
        private void Init_Windows()
        {
            Window_width = Screen.GetBounds(this).Width;
            Window_hight = Screen.GetBounds(this).Height;
            WindowState = FormWindowState.Maximized;
            panel1.Width = Window_width-20;
            panel1.Height = Window_width / 10;
          
            change_pack.Location = new Point(panel1.Location.X, panel1.Location.Y + panel1.Height);
            change_pack.Width = Window_width / 3 * 2;
            change_pack.Height = Window_hight / 5*3;
            dg_all_element.Location= new Point(panel1.Location.X, panel1.Location.Y + panel1.Height);
            dg_all_element.Width = change_pack.Width;
            dg_all_element.Height = Window_hight / 5 * 2 ;

            dg_order_undone.Width = change_pack.Width;
            dg_order_undone.Height = Window_hight / 5 * 2 - label5.Height - 50;
         
            panel2.Width= Window_width / 3-20 ;
            panel2.Height = Window_hight / 5 * 2;
            dg_element.Width = panel2.Width;
            
            panel2.Location = new Point(change_pack.Location.X+ change_pack.Width, change_pack.Location.Y );
            dg_page_done.Width = panel2.Width;
            panle_reprint.Location= new Point(panel2.Location.X, panel2.Location.Y + panel2.Height);
            
        }

        /// <summary>数据库初始化</summary>
        private void Init_DB()
        {
            database_ip = Settings1.Default.database_ip;
            database_user = Settings1.Default.database_user;
            database_pass = Settings1.Default.database_pass;
            database_name_mysqlStr_manufacture = Settings1.Default.database_name_hanhai_manufacture;
            database_name_mysqlStr_management= Settings1.Default.database_name_hanhai_management;
            database_name_mysqlStr_produce= Settings1.Default.database_name_hanhai_produce;
            mysqlStr_manufacture = "Database='" + database_name_mysqlStr_manufacture + "';Data Source='" + database_ip + "';User Id='" + database_user + "';Password='" + database_pass + "';CharSet='utf8'";
            mysqlStr_management = "Database='" + database_name_mysqlStr_management + "';Data Source='" + database_ip + "';User Id='" + database_user + "';Password='" + database_pass + "';CharSet='utf8'";
            mysqlStr_produce= "Database='" + database_name_mysqlStr_produce + "';Data Source='" + database_ip + "';User Id='" + database_user + "';Password='" + database_pass + "';CharSet='utf8'";
            if (Settings1.Default.check_state=="瀚海")
            {
                本地ToolStripMenuItem.Checked = false;
                瀚海ToolStripMenuItem.Checked = true;
                
            }
            if (Settings1.Default.check_state == "本地")
            {
                本地ToolStripMenuItem.Checked = true;
                瀚海ToolStripMenuItem.Checked = false;
            }
            label7.Text = Settings1.Default.check_state;
        }
        private void Thread1()
        {
          
            Thread.Sleep(10000);
        }
    
        /// <summary>登入账号</summary>
        /// <param name="code">关键字</param>
        private void Login(String code)
        {
            
            Console.WriteLine("Start process" + code);
            if (code.Contains("YG") || code.Contains("yg"))
            {
                job_id = "";
                if (code.Contains("YG")) { job_id = code.Split('G')[1]; }    
                string sql_select = "SELECT `Position`, `Name` FROM `info_staff_new` WHERE `Job_id`=" + job_id;
                DataSet ds_user_info = MySqlHelper.GetDataSet(mysqlStr_management, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
                DataTable dt_user_info = ds_user_info.Tables[0];     
                if (dt_user_info.Rows.Count > 0)//查询到
                {
                    string Position= dt_user_info.Rows[0]["Position"].ToString();
                    if (is_on==false&&(Position==Settings1.Default.postion||Position==Settings1.Default.su_postion))
                    {
                        Operator_id = dt_user_info.Rows[0]["Name"].ToString();
                        label3.Text = Operator_id;
                        Speak(Operator_id + "已上线");
         
                        string sql_update1 = "UPDATE `info_staff_new` SET `is_login_state`=1,`checkin_time`=now(),`checkout_time`=null WHERE `Job_id`='" + job_id + "'";
                        MySqlHelper.ExecuteNonQuery(mysqlStr_management, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));

                        is_on = true;
                      
                        job_id_old = job_id;
                        refrash.Enabled = true;
                        Update_page_done();
                        Update_element();
                        Update_Sec_dgview_all();
                        Update_page_undone_temporary();


                    }
                    else if(job_id == job_id_old) {
                            Operator_id = dt_user_info.Rows[0]["Name"].ToString();
                            label3.Text = "请登录";
                            Speak(Operator_id + "已下线");
                            string sql_update1 = "UPDATE `info_staff_new` SET `is_login_state`=-1,`checkout_time`=now(),`checkin_time`=null WHERE `Job_id`='" + job_id + "'";
                            MySqlHelper.ExecuteNonQuery(mysqlStr_management, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));

                        is_on = false;
                             refrash.Enabled = false;
                             dg_all_element.Rows.Clear();
                    }
                }        
                else
                {
                    Console.WriteLine("没有查到工号信息", code);
                }
              
            }
          
        }
        private void Scan_order(String code)
        {
           
            Console.WriteLine("订单号为" + code);
            if (code.Contains("C") || code.Contains("O"))//判断是否为订货单号
            {
               
                string sql_select = "SELECT `Board_type`, `Color`,`Board_height`,`Board_width`,`Order_id`,`Sec_id`,`Part_id`,`Package_work_order_ap_id_hcj`,`Contract_id` FROM `order_element_online` WHERE `State`=" + Settings1.Default.未打包状态+" and `Code`='" + code + "'";
                DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
                DataTable dt_ds_element = ds_element.Tables[0];
                if (dt_ds_element.Rows.Count>0)
                {
                    Order_id = Convert.ToString(dt_ds_element.Rows[0]["Order_id"]);
                    Sec_id = Convert.ToString(dt_ds_element.Rows[0]["Sec_id"]);
                    Part_id = Convert.ToString(dt_ds_element.Rows[0]["Part_id"]);
                    Contract_id = Convert.ToString(dt_ds_element.Rows[0]["Contract_id"]);
                    Ap_id_new = Convert.ToString(dt_ds_element.Rows[0]["Package_work_order_ap_id_hcj"]);
                    
                    tb_Order_id.Text = Order_id;
                    tb_code.Text = code;

                    if (dg_element.Rows.Count==0)  //新订单，寄存old
                    {
                        

                        Update_page_undone_order();
                        Order_id_old = Order_id;
                    }
                    if (dg_element.Rows.Count > 0)  
                    {
                        //Update_page_undone_order();
                        Order_id_old = Convert.ToString(dg_element.Rows[0].Cells["订单号"].Value);
                    }
                    if (Order_id_old != Order_id)
                    {
                        is_Order_new = false;
                        Order_id = Order_id_old;
                    }
                    else
                    {
                        is_Order_new = true;
                    }
                    if (is_Order_new == true)//如果为第一块 即新订单
                    {
           
                        Order_id_old = Order_id;
                        int index = dg_element.Rows.Add();
                        dg_element.Rows[index].Cells["颜色"].Value = dt_ds_element.Rows[0]["Color"];
                        dg_element.Rows[index].Cells["门型"].Value = dt_ds_element.Rows[0]["Board_type"];
                        dg_element.Rows[index].Cells["高度"].Value = dt_ds_element.Rows[0]["Board_height"];
                        dg_element.Rows[index].Cells["宽度"].Value = dt_ds_element.Rows[0]["Board_width"];
                        dg_element.Rows[index].Cells["订单号"].Value = dt_ds_element.Rows[0]["Order_id"];
                        dg_element.Rows[index].Cells["条形码"].Value = code;
                       for (int i = 0; i < dg_order_undone.RowCount; i++)
                        {
                            if (Convert.ToString(dg_order_undone.Rows[i].Cells["条形码2"].Value)==code)
                            {  
                                dg_order_undone.Rows.RemoveAt(i);
                                break;
                            }
                        }

                        if (dg_order_undone.RowCount == 0)
                        {
                            Speak("最后一块");//语言播报第几块
                            
                        }
                        else
                        {
                            String speak = "第" + dg_element.Rows.Count + "块";
                            Speak(speak);//语言播报第几块 

                        }

                        //以订单号检索，判断该为第几包
                        if (is_newpackage == true)
                        {
                            is_newpackage = false;
                            string sql_select1 = "SELECT `Ap_id` FROM `work_package_task_list` WHERE `Order_id`='" + Order_id+ "'  GROUP BY 1  order by `Create_Time`  ";
                            DataSet ds = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select1, new MySqlParameter("@prodid", 24));
                            DataTable dt = ds.Tables[0];
                            if (dt.Rows.Count == 0)//此时此订单尚未有打包
                            {
                                Ap_id = "P"+ Order_id + "-1";
                            }
                                
                            else if(dt.Rows.Count > 0) //已有打包工单号，
                            {
                                if (dg_element.Rows.Count>1)
                                {
                                    Ap_id = Convert.ToString(dg_element.Rows[0].Cells["工单号"].Value);
                                }
                                else
                                {
                                    Ap_id = Convert.ToString(dt.Rows[dt.Rows.Count-1]["Ap_id"]);
                                    Ap_id = "P"+ Order_id + "-" + Convert.ToString(Convert.ToInt32(Ap_id.Split('-')[1]) + 1);

                                }

                            }
                                
                            Package_num = Ap_id.Split('-')[1];

                            
                         }
                        //更新至打包完成
                        //order_element_online

                        string sql_update1 = "update `order_element_online` set `State`=" + Settings1.Default.打包完成状态 + ",`Package_work_order_ap_id_hcj`='" + Ap_id + "', `Package_work_order_create_time`='" + DateTime.Now.ToString() + "',`Shelf_after_membrane_operator_id`= '" + job_id + "',`Shelf_after_membrane_time`='" + DateTime.Now.ToString() + "' WHERE `Part_id`='" + Part_id + "'";
                        MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));
                        //order_part_online 
                        string sql_update3 = "update `order_part_online` set `State`=" + Settings1.Default.打包完成状态 + " ,`Package_task_list_ap_id_hcj`='" + Ap_id + "', `Shelf_after_membrane_time`='" + DateTime.Now.ToString() + "',`Shelf_after_membrane_operator_id`= '" + job_id + "' WHERE `Part_id`='" + Part_id + "'";
                        MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update3, new MySqlParameter("@prodid", 24));

                        string sql_update4 = "insert into `work_package_task_list_temporary` set `Code`='" + code + "' ,`Ap_id`='" + Ap_id + "',`Color`='" + dt_ds_element.Rows[0]["Color"] + "',`Board_type`='" + dt_ds_element.Rows[0]["Board_type"] + "',`Board_height`='" + dt_ds_element.Rows[0]["Board_height"] + "',`Board_width`='" + dt_ds_element.Rows[0]["Board_width"] + "',`Order_id`='" + dt_ds_element.Rows[0]["Order_id"] + "',`Contract_id`='" + dt_ds_element.Rows[0]["Contract_id"] + "',`Sec_id`='" + dt_ds_element.Rows[0]["Sec_id"] + "',`Part_id`='" + dt_ds_element.Rows[0]["Part_id"] + "'";
                        MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update4, new MySqlParameter("@prodid", 24));
                        change_pack.SelectedIndex = 1;//切换至以订单
                        tabControl1.SelectedIndex = 1;
                        Update_Sec_dgview_by_order(Order_id);
                        label9.Text = "剩" + dg_order_undone.RowCount + "块";
                       

                    } 
                    else
                    {
                        SystemSounds.Hand.Play();
                        Speak("错误");
                        Console.WriteLine("已完成打包", code);
                    }
                }
                else
                {
                    SystemSounds.Hand.Play();
                    Speak("重复");
                    Console.WriteLine("重复代码", code);
                }
            }
            
            //打包条码(Package000000)
            if (code.Contains("000000"))
            {
                string sql_select = "SELECT `Order_id`,`Contract_id`,`Sec_id`,`Part_id`,`Board_type`,`Color`,`Board_height`,`Board_width`,`Code`,`Ap_id` FROM `work_package_task_list_temporary` WHERE 1";
                DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
                DataTable dt_all_element = ds_element.Tables[0]; //
                if (dt_all_element.Rows.Count > 0)
                {
                   
                    Sec_id = Convert.ToString(dt_all_element.Rows[0]["Sec_id"]);
                    Ap_id = Convert.ToString(dt_all_element.Rows[0]["Ap_id"]);
                    Order_id = Convert.ToString(dt_all_element.Rows[0]["Order_id"]);
                    Contract_id= Convert.ToString(dt_all_element.Rows[0]["Contract_id"]);
                    Package_num = Ap_id.Split('-')[1];
                }
              




                if (dg_element.Rows.Count > 0)
                {
                    
                    is_newpackage = true;
                    is_Order_new = true;
                    dg_element.Rows.Clear();
                    
                    //打包工单插入
                    string sql_insert = "INSERT INTO `work_package_task_list` (Ap_id, Operator_id, Sec_id,Create_Time,Print_Barcode,Package_num,Total_plies,Order_id) VALUES('"+Ap_id+ "','" + job_id + "','" + Sec_id + "','" + DateTime.Now.ToString()+ "','" + 100 + "','" + Package_num + "',1,'" + Order_id + "') ";
                    MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_insert, new MySqlParameter("@prodid", 24));
                    update_db_state_all(Settings1.Default.打包完成状态);

                    Update_page_done();
                    if (is_all_done==true)
                    {
                        Speak("打包完成");

                        string sql_select1 = "SELECT `Sec_id` FROM `order_section_online` WHERE  `Sec_type`=1 and `is_packed`=0 and `Order_id`='" + Order_id + "'";
                        DataSet ds_element1 = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select1, new MySqlParameter("@prodid", 24));
                        DataTable dt_ds_element1 = ds_element1.Tables[0];
                        if (dt_ds_element1.Rows.Count > 0)
                        {
                            bt_sec.Enabled = false;
                            bt_sec.Visible = true;
                            lb_sec.Visible = true;
                            Speak("打包完成,存在整套组件,请打包");
                         

                        }
                        //Update_Sec_dgview(Order_id);
                        string sql_clear = "delete from `work_package_task_list_temporary` ";
                        MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_clear, new MySqlParameter("@prodid", 24));
                      
                    }
                    else
                    {
                        
                        String s = "第" + Package_num + "包";
                        Speak(s);
                        string sql_clear = "delete from `work_package_task_list_temporary` ";
                        MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_clear, new MySqlParameter("@prodid", 24));
                      
                    }

                    //总包数更新
                    string sql_update = "update `order_order_online` set `Package_num_hcj`='" + Package_num + "'where `Order_id`='" + Order_id + "'";
                    MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update, new MySqlParameter("@prodid", 24));

                    Update_page_done_change(Order_id);
                }
                else
                {
                    SystemSounds.Hand.Play();
                    Speak("错误");
                }   
                
              }
        }
        
        /// <summary>语言模块</summary>
        
        private void Speak(string i)
        {
            try
            {
      
                SpeechSynthesizer voice = new SpeechSynthesizer();   //创建语音实例
                label8.Text = i;
                voice.Rate = 1; //设置语速,[-10,10]
                voice.Volume = 100; //设置音量,[0,100]
                string filepath = "C:\\mp3\\"+i+".wav";
                if (File.Exists(filepath))
                {
                    SoundPlayer s = new SoundPlayer(filepath);
                    s.Play();
                    Console.Read();
                }
                else 
                {
                   

                    voice.SetOutputToWaveFile(filepath);
                    voice.Speak(i);
                    voice.SetOutputToNull();
                    voice.SetOutputToDefaultAudioDevice();
                    voice.Dispose();
                    SoundPlayer s = new SoundPlayer(filepath);
                    s.Play();
                }
            }
            catch (Exception)
            {

                throw;
            }
           
        }
        private void Scan_load(String code)
        {
            code = code.ToUpper();
            Login(code);
            Scan_order(code);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.Width = Window_width;
            this.Height = Window_hight;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
           
          
        }
        /// <summary>刷新按钮</summary>
        private void refrash_Click(object sender, EventArgs e)
        {
            Update_page_undone_order();
            Update_element();
            Update_page_done();
            Update_Sec_dgview_all();

        }

        /// <summary>重新打印</summary>
        private void reprint_Click(object sender, EventArgs e)
        {

            if (textBox3.Text.Contains("P") && textBox3.Text.Contains("-")) //打包工单号
            {
                string sql_update1 = "update `work_package_task_list` set `Print_Barcode`='" + 100 + "' WHERE `Ap_id`='" + textBox3.Text + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));
                textBox3.Text = "";
            }
            else if (textBox3.Text.Contains("O") || textBox3.Text.Contains("C") || textBox3.Text.Contains("o")) //订单号
            {
                string sql_select1 = "SELECT `Package_work_order_ap_id_hcj` FROM `order_element_online` WHERE `Code`='" + textBox3.Text + "'";
                DataSet ds = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select1, new MySqlParameter("@prodid", 24));
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    string  Package_work_order_ap_id_hcj = Convert.ToString(dt.Rows[0]["Package_work_order_ap_id_hcj"]);
                    string sql_update1 = "update `work_package_task_list` set `Print_Barcode`='" + 100 + "' WHERE `Ap_id`='" + Package_work_order_ap_id_hcj + "'";
                    MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));
                    textBox3.Text = "";
                }
                else
                {
                    SystemSounds.Hand.Play();
                    Speak("错误");
                }
            }
            else
            {
                SystemSounds.Hand.Play();
                Speak("错误");
            }

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            textBox3.Text = dg_page_done[e.ColumnIndex, e.RowIndex].Value.ToString();//第一种取法
        }
        private void bt_submit_Click(object sender, EventArgs e) //确认
        {
            
            Scan_load(tb_person.Text.ToString());
            tb_person.Text = "";
            if (tb_rescan.Text!="")
            {
                Increase_element(tb_re_ad_id.Text,tb_rescan.Text);
                tb_rescan.Text = "";
            }
            if (tb_change_order.Text != "")
            {
                Update_page_done_change(tb_change_order.Text);
               
                tb_change_order.Text = "";
            }

            
            if (!tb_person.Focused)
            {
                tb_person.Focus();
            }

        }

        /// <summary>更新合同 组件 状态</summary>
        private void update_db_state_all(string State)   ///HCJ  更新合同 组件 状态
        {
            //合同
            String sql_count_contract_num = "select COUNT(*) from `order_element_online` where `Contract_id`='" + Contract_id + "' and `Element_type_id` in (1,3,9,4,5,6)";
            String sql_count_state_num = "select COUNT(*) from `order_element_online` where `Contract_id`='" + Contract_id + "' and `Element_type_id` in (1,3,9,4,5,6) and `State`=" + State;
            //订单
            String sql_count_order_num = "select COUNT(*) from `order_element_online` where `Order_id`='" + Order_id + "' and `Element_type_id` in (1,3,9,4,5,6)";
            String sql_count_order_state_num = "select COUNT(*) from `order_element_online` where `Order_id`='" + Order_id + "' and `Element_type_id` in (1,3,9,4,5,6) and `State`=" + State;
           
            DataSet ds_count = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_count_contract_num, new MySqlParameter("@prodid", 24));
            DataTable dt_count1= ds_count.Tables[0];
            count_contract_num = Convert.ToString(dt_count1.Rows[0][0]);

            DataSet ds_count2 = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_count_state_num, new MySqlParameter("@prodid", 24));
            DataTable dt_count2 = ds_count2.Tables[0];
            count_state_num = Convert.ToString(dt_count2.Rows[0][0]);

            DataSet ds_count3 = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_count_order_num, new MySqlParameter("@prodid", 24));
            DataTable dt_count3 = ds_count3.Tables[0];
            count_order_num = Convert.ToString(dt_count3.Rows[0][0]);

            DataSet ds_count4 = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_count_order_state_num, new MySqlParameter("@prodid", 24));
            DataTable dt_count4 = ds_count4.Tables[0];
            count_order_state_num = Convert.ToString(dt_count4.Rows[0][0]);



            Console.WriteLine("count_state_num=" + count_state_num + "count_contract_num=" + count_contract_num);
            if (count_state_num == count_contract_num)//当所有零件都完成状态，则合同状态改变
            {
                Console.WriteLine("所有零件都完成状态，合同状态改变");
                string sql_update = "UPDATE `order_contract_internal` SET `State`=" + State + " WHERE `Contract_id` = '" + Contract_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update, new MySqlParameter("@prodid", 24));
                
            }
         
            if (count_order_state_num == count_order_num)//更新 组件 订单库
            {
                is_all_done = true;
                Console.WriteLine("所有零件都完成状态，组件 订单状态改变");
                try
                {
                    string Sql_do1 = "UPDATE `order_order_online` SET `State`=" + State + ",`Package_num_hcj`=" + Package_num + "  WHERE `Order_id` = '" + Order_id + "'";
                    string Sql_do2 = "UPDATE `order_section_online` SET `State`=" + State + " WHERE `Order_id` = '" + Order_id + "'";
                    MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, Sql_do1, new MySqlParameter("@prodid", 24));
                    MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, Sql_do2, new MySqlParameter("@prodid", 24));
                    

                }
                catch (Exception)
                {

                    throw;
                }
               

            }
            else
            {
                is_all_done = false;
            }


        } //hcj 2018年8月20日
        /// <summary>更新未打包零件表格</summary>
        private void Update_element()
        {
            
            dg_all_element.Rows.Clear();
            string sql_select = "SELECT `Order_id`,`Board_thick`, `Board_type`,`Color`,`Board_height`,`Board_width`,`Open_way`,`Edge_type`,`Code`,`Package_work_order_ap_id_hcj` FROM `order_element_online` WHERE `State`=" + Settings1.Default.未打包状态 + " and `Element_type_id` in (1,3,9,4,5,6)";
            DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
            DataTable dt_all_element = ds_element.Tables[0]; //
            if (dt_all_element.Rows.Count > 0)
            {
                dg_all_element.Rows.Clear();
                if (dt_all_element.Rows.Count<100)
                {
                    for (int i = 0; i < dt_all_element.Rows.Count; i++)
                    {
                        int index = dg_all_element.Rows.Add();

                        dg_all_element.Rows[index].Cells["订单号1"].Value = dt_all_element.Rows[i]["Order_id"];
                        dg_all_element.Rows[index].Cells["基材1"].Value = dt_all_element.Rows[i]["Board_thick"];
                        dg_all_element.Rows[index].Cells["门型1"].Value = dt_all_element.Rows[i]["Board_type"];
                        dg_all_element.Rows[index].Cells["高度1"].Value = dt_all_element.Rows[i]["Board_height"];
                        dg_all_element.Rows[index].Cells["宽度1"].Value = dt_all_element.Rows[i]["Board_width"];
                        dg_all_element.Rows[index].Cells["颜色1"].Value = dt_all_element.Rows[i]["Color"];
                        dg_all_element.Rows[index].Cells["打孔1"].Value = dt_all_element.Rows[i]["Open_way"];
                        dg_all_element.Rows[index].Cells["边型1"].Value = dt_all_element.Rows[i]["Edge_type"];
                        dg_all_element.Rows[index].Cells["条形码1"].Value = dt_all_element.Rows[i]["Code"];
                    }
                }
                else
                {
                    for (int i = 0; i < 100; i++)
                    {
                        int index = dg_all_element.Rows.Add();

                        dg_all_element.Rows[index].Cells["订单号1"].Value = dt_all_element.Rows[i]["Order_id"];
                        dg_all_element.Rows[index].Cells["基材1"].Value = dt_all_element.Rows[i]["Board_thick"];
                        dg_all_element.Rows[index].Cells["门型1"].Value = dt_all_element.Rows[i]["Board_type"];
                        dg_all_element.Rows[index].Cells["高度1"].Value = dt_all_element.Rows[i]["Board_height"];
                        dg_all_element.Rows[index].Cells["宽度1"].Value = dt_all_element.Rows[i]["Board_width"];
                        dg_all_element.Rows[index].Cells["颜色1"].Value = dt_all_element.Rows[i]["Color"];
                        dg_all_element.Rows[index].Cells["打孔1"].Value = dt_all_element.Rows[i]["Open_way"];
                        dg_all_element.Rows[index].Cells["边型1"].Value = dt_all_element.Rows[i]["Edge_type"];
                        dg_all_element.Rows[index].Cells["条形码1"].Value = dt_all_element.Rows[i]["Code"];
                    }
                }

            }

            /*/此段为将原有打包方案所赋的值至NULL
            string Sql_do1 = "UPDATE `order_element_online` SET `Package_work_order_ap_id_hcj`= null WHERE `State`=" + Settings1.Default.未打包状态;
            MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, Sql_do1, new MySqlParameter("@prodid", 24));
            */
        }

        private void 瀚海ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            本地ToolStripMenuItem.Checked = false;
            瀚海ToolStripMenuItem.Checked = true;
            Settings1.Default.check_state = "瀚海";
            database_ip = Settings1.Default.database_ip_hanhai;
            database_user = Settings1.Default.database_user_hanhai;
            database_pass = Settings1.Default.database_pass_hanhai;
            Settings1.Default.database_ip = database_ip;
            Settings1.Default.database_user = database_user;
            Settings1.Default.database_pass = database_pass;
            Settings1.Default.Save();
            Init_DB();
        }

        private void 本地ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            瀚海ToolStripMenuItem.Checked = false;
            本地ToolStripMenuItem.Checked = true;
            Settings1.Default.check_state = "本地";
            database_ip = Settings1.Default.database_ip_local;
            database_user = Settings1.Default.database_user_local;
            database_pass = Settings1.Default.database_pass_local;
            Settings1.Default.database_ip = database_ip;
            Settings1.Default.database_user = database_user;
            Settings1.Default.database_pass = database_pass;
            Settings1.Default.Save();
            Init_DB();
        }

        private void 配置ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Password = Interaction.InputBox("请输入管理员密码", "数据库服务器修改", "", 100, 100);
            if (Password=="0")
            {
                //string database_name = Settings1.Default.database_name;
                 database_ip = Settings1.Default.database_ip;
                 database_user = Settings1.Default.database_user;
                 database_pass = Settings1.Default.database_pass;
                database_ip = Interaction.InputBox("请输入服务器IP", "数据库服务器修改", database_ip, 100, 100);
                Settings1.Default.database_ip = database_ip;
                database_user = Interaction.InputBox("请输入登陆用户名", "数据库服务器修改", database_user, 100, 100);
                Settings1.Default.database_user = database_user;
                database_pass = Interaction.InputBox("请输入登陆密码", "数据库服务器修改", database_pass, 100, 100);
                Settings1.Default.database_pass = database_pass;
                Settings1.Default.Save();
                Init_DB();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("密码输入错误...");
            }
            
        }



        /// <summary>更新已打包工单表格</summary>
        private void Update_page_done()
        {
            dg_page_done.Rows.Clear();
            string sql_select = "SELECT `Ap_id`,`Order_id` From `work_package_task_list` where "+string_check_day_info + " order by `Create_Time` desc";
            DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
            DataTable dt_all_element = ds_element.Tables[0]; //
            if (dt_all_element.Rows.Count > 0)
            {
                
                for (int i = 0; i < dt_all_element.Rows.Count; i++)
                {
                    if (Convert.ToString(dt_all_element.Rows[i]["Ap_id"]).Contains("P"))
                    {
                        dg_page_done.Rows.Add(dt_all_element.Rows[i].ItemArray);
                    }
                  
                }

            }
        }
        /// <summary>更新变更打包方案里的打包工单号</summary>
        private void Update_page_done_change(string Change_Order_id)
        {
            change_apid.Rows.Clear();
            string sql_select = "SELECT `Ap_id`,`Order_id` From `work_package_task_list` where `Ap_id` REGEXP '" + Change_Order_id + "'"; ;
            DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
            DataTable dt_all_element = ds_element.Tables[0]; //
            if (dt_all_element.Rows.Count > 0)
            {

                for (int i = 0; i < dt_all_element.Rows.Count; i++)
                {
                 
                    change_apid.Rows.Add(dt_all_element.Rows[i].ItemArray);
                    

                }

            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void 版本信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 fm = new AboutBox1();
            fm.Show();
        }

        /// <summary>以订单更新未打包表格</summary>
        private void Update_page_undone_order()
        {
            dg_order_undone.Rows.Clear();
            string sql_select = "SELECT `Order_id`,`Board_thick`, `Board_type`,`Color`,`Board_height`,`Board_width`,`Open_way`,`Edge_type`,`Code`,`Package_work_order_ap_id_hcj` FROM `order_element_online` WHERE `State`=" + Settings1.Default.未打包状态 + " and `Element_type_id` in (1,3,9,4,5,6) and `Order_id`='"+Order_id+"'";
            DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
            DataTable dt_all_element = ds_element.Tables[0]; //
            if (dt_all_element.Rows.Count > 0)
            {
                dg_order_undone.Rows.Clear();

                for (int i = 0; i < dt_all_element.Rows.Count; i++)
                {

                    int index = dg_order_undone.Rows.Add();

                    dg_order_undone.Rows[index].Cells["订单号2"].Value = dt_all_element.Rows[i]["Order_id"];
                    dg_order_undone.Rows[index].Cells["基材2"].Value = dt_all_element.Rows[i]["Board_thick"];
                    dg_order_undone.Rows[index].Cells["门型2"].Value = dt_all_element.Rows[i]["Board_type"];
                    dg_order_undone.Rows[index].Cells["高度2"].Value = dt_all_element.Rows[i]["Board_height"];
                    dg_order_undone.Rows[index].Cells["宽度2"].Value = dt_all_element.Rows[i]["Board_width"];
                    dg_order_undone.Rows[index].Cells["颜色2"].Value = dt_all_element.Rows[i]["Color"];
                    dg_order_undone.Rows[index].Cells["打孔2"].Value = dt_all_element.Rows[i]["Open_way"];
                    dg_order_undone.Rows[index].Cells["边型2"].Value = dt_all_element.Rows[i]["Edge_type"];
                    dg_order_undone.Rows[index].Cells["条形码2"].Value = dt_all_element.Rows[i]["Code"];
                }

            }
            label9.Text = "剩" + dg_order_undone.RowCount + "块";
        }

        private void dg_element_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void bt_cancel_Click(object sender, EventArgs e)
        {   
            cancel_code = tb_cancel.Text;
            cancel_code= cancel_code.ToUpper();
            tb_cancel.Text = "";
            if (cancel_code.Contains("C") || cancel_code.Contains("O") || cancel_code.Contains("o"))//判断是否为订货单号
            {
                string sql_select = "SELECT `Order_id`,`Sec_id`,`Part_id`,`Package_work_order_ap_id_hcj`,`Contract_id` FROM `order_element_online` WHERE `State`=" + Settings1.Default.打包完成状态 + " and `Code`='" + cancel_code + "'";
                DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
                DataTable dt_ds_element = ds_element.Tables[0];
                if (dt_ds_element.Rows.Count > 0)
                {
                    Order_id = Convert.ToString(dt_ds_element.Rows[0]["Order_id"]);
                    Sec_id = Convert.ToString(dt_ds_element.Rows[0]["Sec_id"]);
                    Part_id = Convert.ToString(dt_ds_element.Rows[0]["Part_id"]);
                    Contract_id = Convert.ToString(dt_ds_element.Rows[0]["Contract_id"]);
                    Ap_id_new = Convert.ToString(dt_ds_element.Rows[0]["Package_work_order_ap_id_hcj"]);
                }
                string sql_update1 = "update `order_element_online` set `State`=" + Settings1.Default.未打包状态 + ",`Package_work_order_ap_id_hcj`=null, `Package_work_order_create_time`=null,`Shelf_after_membrane_operator_id`= null,`Shelf_after_membrane_time`=null WHERE `Part_id`='" + Part_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));
                string sql_update3 = "update `order_part_online` set `State`=" + Settings1.Default.未打包状态 + " ,`Package_task_list_ap_id_hcj`=null, `Shelf_after_membrane_time`=null,`Shelf_after_membrane_operator_id`= null WHERE `Part_id`='" + Part_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update3, new MySqlParameter("@prodid", 24));
                string sql_clear = "delete from `work_package_task_list_temporary` where `Code`='"+ cancel_code+"'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_clear, new MySqlParameter("@prodid", 24));
                Update_page_undone_temporary();
                Update_page_undone_order();
                Update_page_done();
               
                Speak("取消");
            }
            else
            {
                SystemSounds.Hand.Play();
                Speak("错误");
            }
        }

        private void dg_all_element_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
          //  tb_person.Text = dg_all_element[e.ColumnIndex, e.RowIndex].Value.ToString();//第一种取法
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
        //撤销按钮
        private void button2_Click(object sender, EventArgs e)
        {
            Re_pack_Pack_id = Interaction.InputBox("请输入撤销打包工单号,该工单下所有零件恢复至未打包状态,需再次重新打包", "撤销打包的工单号号", "", 100, 100);
            Re_pack_Pack_id = Re_pack_Pack_id.ToUpper();
            if (Re_pack_Pack_id.Contains("P") && Re_pack_Pack_id.Contains("-"))//判断是否为订货单号
            {
                if (Re_pack_Pack_id.Contains("S"))
                {
                    string Sql_do2 = "UPDATE `order_section_online` SET `State`=" + Settings1.Default.未打包状态 + ",`is_packed`=0 WHERE `Sec_id` = '" + Re_pack_Pack_id + "'";
                    MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, Sql_do2, new MySqlParameter("@prodid", 24));

                }
                else
                {
                    string sql_update1 = "update `order_element_online` set `State`=" + Settings1.Default.未打包状态 + ",`Package_work_order_ap_id_hcj`=null, `Package_work_order_create_time`=null,`Shelf_after_membrane_operator_id`= null,`Shelf_after_membrane_time`=null WHERE `Package_work_order_ap_id_hcj`='" + Re_pack_Pack_id + "'";
                    MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));
                    string sql_update3 = "update `order_part_online` set `State`=" + Settings1.Default.未打包状态 + " ,`Package_task_list_ap_id_hcj`=null, `Shelf_after_membrane_time`=null,`Shelf_after_membrane_operator_id`= null WHERE `Package_task_list_ap_id_hcj`='" + Re_pack_Pack_id + "'";
                    MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update3, new MySqlParameter("@prodid", 24));
                    string sql_clear = "delete from `work_package_task_list` where `Ap_id`='" + Re_pack_Pack_id + "'";
                    MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_clear, new MySqlParameter("@prodid", 24));
                    Update_page_undone_order();
                    Update_page_done();
                    Speak("打包工单已取消");
                }
                
            }
            else if ( Re_pack_Pack_id.Contains("O"))
            {
                string sql_update1 = "update `order_element_online` set `State`=" + Settings1.Default.未打包状态 + ",`Package_work_order_ap_id_hcj`=null, `Package_work_order_create_time`=null,`Shelf_after_membrane_operator_id`= null,`Shelf_after_membrane_time`=null WHERE `Order_id`='" + Re_pack_Pack_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));
                string sql_update3 = "update `order_part_online` set `State`=" + Settings1.Default.未打包状态 + " ,`Package_task_list_ap_id_hcj`=null, `Shelf_after_membrane_time`=null,`Shelf_after_membrane_operator_id`= null WHERE `Order_id`='" + Re_pack_Pack_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update3, new MySqlParameter("@prodid", 24));
                string sql_clear = "delete from `work_package_task_list` where `Ap_id` REGEXP '" + Re_pack_Pack_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_clear, new MySqlParameter("@prodid", 24));
                Contract_id = Re_pack_Pack_id.Split('O')[0];
                string sql_update = "UPDATE `order_contract_internal` SET `State`=" + Settings1.Default.未打包状态 + " WHERE `Contract_id` = '" + Contract_id + "'";
                string Sql_do1 = "UPDATE `order_order_online` SET `State`=" + Settings1.Default.未打包状态 + ",`Package_num_hcj`=" + 0 + "  WHERE `Order_id` = '" + Re_pack_Pack_id + "'";
                string Sql_do2 = "UPDATE `order_section_online` SET `State`=" + Settings1.Default.未打包状态 + ",`is_packed`=0 WHERE `Order_id` = '" + Re_pack_Pack_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update, new MySqlParameter("@prodid", 24));
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, Sql_do1, new MySqlParameter("@prodid", 24));
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, Sql_do2, new MySqlParameter("@prodid", 24));

                Update_page_undone_order();
                Update_page_done();
                Speak("该订单已撤销打包");
            }
            else
            {
                SystemSounds.Hand.Play();
                Speak("错误");
            }

           




        }

        private void ver_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void change_apid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {  
                tb_re_ad_id.Text = change_apid["包装条码", e.RowIndex].Value.ToString();
                string a = tb_re_ad_id.Text;
                change_grid.Rows.Clear();
                if (tb_re_ad_id.Text.Contains("O"))
                {
                    if (tb_re_ad_id.Text.Contains("S"))
                    {
                        string sec_id = tb_re_ad_id.Text.Split('P')[1].Split('-')[0];
                        string sql_select1 = "SELECT `Sec_id`,`Sec_series`,`Sec_model`,`Sec_color`,`Sec_thick`,`Order_id` FROM `order_section_online` WHERE  `Sec_id`='" + sec_id + "'";
                        DataSet ds_element1 = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select1, new MySqlParameter("@prodid", 24));
                        DataTable dt_ds_element1 = ds_element1.Tables[0];
                 
                        if (dt_ds_element1.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt_ds_element1.Rows.Count; i++)
                            {

                                int index = change_grid.Rows.Add();
                                change_grid.Rows[index].Cells["颜色3"].Value = dt_ds_element1.Rows[i]["Sec_color"];
                                change_grid.Rows[index].Cells["门型3"].Value = dt_ds_element1.Rows[i]["Sec_model"];
                                change_grid.Rows[index].Cells["高度3"].Value = dt_ds_element1.Rows[i]["Sec_series"];
                                change_grid.Rows[index].Cells["宽度3"].Value = "";
                                change_grid.Rows[index].Cells["订单号3"].Value = dt_ds_element1.Rows[i]["Order_id"];
                                change_grid.Rows[index].Cells["条形码3"].Value = dt_ds_element1.Rows[i]["Sec_id"];
                                change_grid.Rows[index].Cells["包装条码3"].Value = tb_re_ad_id.Text;
                            }


                        }
                    }
                    else
                    {
                        string sql_select = "SELECT `Order_id`,`Board_type`,`Color`,`Board_height`,`Board_width`,`Code`,`Package_work_order_ap_id_hcj` FROM `order_element_online` WHERE `Package_work_order_ap_id_hcj`='" + a + "' and `Element_type_id` in (1,3,9,4,5,6)";
                        DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
                        DataTable dt_all_element1 = ds_element.Tables[0]; //
                        if (dt_all_element1.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt_all_element1.Rows.Count; i++)
                            {

                                int index = change_grid.Rows.Add();
                                change_grid.Rows[index].Cells["颜色3"].Value = dt_all_element1.Rows[i]["Color"];
                                change_grid.Rows[index].Cells["门型3"].Value = dt_all_element1.Rows[i]["Board_type"];
                                change_grid.Rows[index].Cells["高度3"].Value = dt_all_element1.Rows[i]["Board_height"];
                                change_grid.Rows[index].Cells["宽度3"].Value = dt_all_element1.Rows[i]["Board_width"];
                                change_grid.Rows[index].Cells["订单号3"].Value = dt_all_element1.Rows[i]["Order_id"];
                                change_grid.Rows[index].Cells["条形码3"].Value = dt_all_element1.Rows[i]["Code"];
                                change_grid.Rows[index].Cells["包装条码3"].Value = dt_all_element1.Rows[i]["Package_work_order_ap_id_hcj"];
                            }

                        }
                    }
                }
                
            }

        }

        private void change_grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
         
        }

        private void button4_Click(object sender, EventArgs e)
        {
          
            int a=change_grid.CurrentRow.Index;
            string Code_c1= Convert.ToString(change_grid.Rows[a].Cells["条形码3"].Value);
            
            if (Code_c1.Contains("C") || Code_c1.Contains("O") || Code_c1.Contains("o"))//判断是否为订货单号
            {
                string sql_select = "SELECT `Order_id`,`Sec_id`,`Part_id`,`Package_work_order_ap_id_hcj`,`Contract_id` FROM `order_element_online` WHERE `State`=" + Settings1.Default.打包完成状态 + " and `Code`='" + Code_c1 + "'";
                DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
                DataTable dt_ds_element = ds_element.Tables[0];
                if (dt_ds_element.Rows.Count > 0)
                {
                    Order_id = Convert.ToString(dt_ds_element.Rows[0]["Order_id"]);
                    Sec_id = Convert.ToString(dt_ds_element.Rows[0]["Sec_id"]);
                    Part_id = Convert.ToString(dt_ds_element.Rows[0]["Part_id"]);
                    Contract_id = Convert.ToString(dt_ds_element.Rows[0]["Contract_id"]);
                    Ap_id_new = Convert.ToString(dt_ds_element.Rows[0]["Package_work_order_ap_id_hcj"]);
                }
                string sql_update1 = "update `order_element_online` set `State`=" + Settings1.Default.未打包状态 + ",`Package_work_order_ap_id_hcj`=null, `Package_work_order_create_time`=null,`Shelf_after_membrane_operator_id`= null,`Shelf_after_membrane_time`=null WHERE `Part_id`='" + Part_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));
                string sql_update3 = "update `order_part_online` set `State`=" + Settings1.Default.未打包状态 + " ,`Package_task_list_ap_id_hcj`=null, `Shelf_after_membrane_time`=null,`Shelf_after_membrane_operator_id`= null WHERE `Part_id`='" + Part_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update3, new MySqlParameter("@prodid", 24));
                string sql_clear = "delete from `work_package_task_list_temporary` where `Code`='" + Code_c1 + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_clear, new MySqlParameter("@prodid", 24));
                Update_page_undone_temporary();
                Update_page_undone_order();
                Update_page_done();
                change_grid.Rows.RemoveAt(a);
                Speak("取消");
                
            }
            else
            {
                SystemSounds.Hand.Play();
                Speak("错误");
            }
        }
        private void Increase_element(string Ap_id,string code)
        {
            Console.WriteLine("订单号为" + code);
            string Ap_id_old="";
            if (code.Contains("C") || code.Contains("O"))//判断是否为订货单号
            {

                string sql_select = "SELECT `Board_type`, `Color`,`Board_height`,`Board_width`,`Order_id`,`Sec_id`,`Part_id`,`Package_work_order_ap_id_hcj`,`Contract_id`,`Code` FROM `order_element_online` WHERE `State`=" + Settings1.Default.未打包状态 + " and `Code`='" + code + "'";
                DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
                DataTable dt_ds_element = ds_element.Tables[0];
                if (dt_ds_element.Rows.Count > 0)
                {
                    Order_id = Convert.ToString(dt_ds_element.Rows[0]["Order_id"]);
                    Sec_id = Convert.ToString(dt_ds_element.Rows[0]["Sec_id"]);
                    Part_id = Convert.ToString(dt_ds_element.Rows[0]["Part_id"]);
                    Contract_id = Convert.ToString(dt_ds_element.Rows[0]["Contract_id"]);
                    Ap_id_old = Convert.ToString(dt_ds_element.Rows[0]["Package_work_order_ap_id_hcj"]);
                
                if (!Ap_id_old.Contains("P"))
                {
                    int index = change_grid.Rows.Add();
                    change_grid.Rows[index].Cells["颜色3"].Value = dt_ds_element.Rows[0]["Color"];
                    change_grid.Rows[index].Cells["门型3"].Value = dt_ds_element.Rows[0]["Board_type"];
                    change_grid.Rows[index].Cells["高度3"].Value = dt_ds_element.Rows[0]["Board_height"];
                    change_grid.Rows[index].Cells["宽度3"].Value = dt_ds_element.Rows[0]["Board_width"];
                    change_grid.Rows[index].Cells["订单号3"].Value = dt_ds_element.Rows[0]["Order_id"];
                    change_grid.Rows[index].Cells["条形码3"].Value = dt_ds_element.Rows[0]["Code"];
                    change_grid.Rows[index].Cells["包装条码3"].Value = Ap_id;
                }

                }
                string sql_update1 = "update `order_element_online` set `State`=" + Settings1.Default.打包完成状态 + ",`Package_work_order_ap_id_hcj`='" + Ap_id + "', `Package_work_order_create_time`='" + DateTime.Now.ToString() + "',`Shelf_after_membrane_operator_id`= '" + job_id + "',`Shelf_after_membrane_time`='" + DateTime.Now.ToString() + "' WHERE `Part_id`='" + Part_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));
                //order_part_online 
                string sql_update3 = "update `order_part_online` set `State`=" + Settings1.Default.打包完成状态 + " ,`Package_task_list_ap_id_hcj`='" + Ap_id + "', `Shelf_after_membrane_time`='" + DateTime.Now.ToString() + "',`Shelf_after_membrane_operator_id`= '" + job_id + "' WHERE `Part_id`='" + Part_id + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update3, new MySqlParameter("@prodid", 24));
                Speak("添加成功");
            }
            else
            {
                Speak("错误");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (tb_re_ad_id.Text.Contains("P") && tb_re_ad_id.Text.Contains("-")) //打包工单号
            {
                string sql_update1 = "update `work_package_task_list` set `Print_Barcode`='" + 100 + "' WHERE `Ap_id`='" + tb_re_ad_id.Text + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));
                update_db_state_all(Settings1.Default.打包完成状态);
            }
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void bt_sec_Click(object sender, EventArgs e)
        {
            if (dg_element.Rows.Count==0)
            {
                int pack_num = 1;
                string sql_select1 = "SELECT `Ap_id` FROM `work_package_task_list` WHERE `Order_id` ='" + lb_sec_order_id.Text + "' order by `Create_Time`  desc limit 1 ";
                DataSet ds = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select1, new MySqlParameter("@prodid", 24));
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    Ap_id = Convert.ToString(dt.Rows[0]["Ap_id"]);
                    pack_num = Convert.ToInt32(Ap_id.Split('-')[1]) + 1;
                }
                Ap_id = "P" + lb_sec.Text + "-" + pack_num;
                string sql_insert = "INSERT INTO `work_package_task_list` (Ap_id, Operator_id, Sec_id,Create_Time,Print_Barcode,Package_num,Total_plies,Order_id) VALUES('" + Ap_id + "','" + job_id + "','" + lb_sec.Text + "','" + DateTime.Now.ToString() + "','" + 100 + "','" + Convert.ToString(Ap_id.Split('-')[1]) + "',1,'" + lb_sec_order_id.Text + "') ";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_insert, new MySqlParameter("@prodid", 24));
                Speak("第" + pack_num + "包");
                string sql_update = "update `order_order_online` set `Package_num_hcj`='" + pack_num + "'where `Order_id`='" + lb_sec_order_id.Text + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update, new MySqlParameter("@prodid", 24));
                string sql_update1 = "update `order_section_online` set `is_packed`='1' where `Sec_id`='" + lb_sec.Text + "'";
                MySqlHelper.ExecuteNonQuery(mysqlStr_manufacture, CommandType.Text, sql_update1, new MySqlParameter("@prodid", 24));
                for (int i = 0; i < dg_sec.RowCount; i++)
                {
                    if (Convert.ToString(dg_sec.Rows[i].Cells["Sec_code_id"].Value) == lb_sec.Text)
                    {
                        dg_sec.Rows.RemoveAt(i);
                        break;
                    }
                }
                for (int i = 0; i < dg_sec_by_order.RowCount; i++)
                {
                    if (Convert.ToString(dg_sec_by_order.Rows[i].Cells["Sec_code_id2"].Value) == lb_sec.Text)
                    {
                        dg_sec_by_order.Rows.RemoveAt(i);
                        break;
                    }
                }
                bt_sec.Enabled = false;
                lb_sec.Text = "";
                if (dg_sec_by_order.RowCount == 0)
                {
                    Speak("整套组件打包完成");
                    bt_sec.Enabled = false;
                    //bt_sec.Visible = false;
                }

                Update_page_done();

            }
            else
            {
                Speak("请先打包板件");
            }


        }

        private void dg_sec_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                lb_sec.Text = dg_sec["Sec_code_id", e.RowIndex].Value.ToString();
                lb_sec_order_id.Text = dg_sec["Order_id_sec", e.RowIndex].Value.ToString();
                if (lb_sec.Text.Contains("S"))
                {
                    bt_sec.Enabled = true;
                }
                
            }
        }

        private void Update_page_undone_temporary()
        {
            dg_element.Rows.Clear();
            string sql_select = "SELECT `Order_id`,`Board_type`,`Color`,`Board_height`,`Board_width`,`Code`,`Ap_id` FROM `work_package_task_list_temporary` WHERE 1";
            DataSet ds_element = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select, new MySqlParameter("@prodid", 24));
            DataTable dt_all_element = ds_element.Tables[0]; //
            if (dt_all_element.Rows.Count > 0)
            {
           

                for (int i = 0; i < dt_all_element.Rows.Count; i++)
                {

                    int index = dg_element.Rows.Add();

                    dg_element.Rows[index].Cells["颜色"].Value = dt_all_element.Rows[i]["Color"];
                    dg_element.Rows[index].Cells["门型"].Value = dt_all_element.Rows[i]["Board_type"];
                    dg_element.Rows[index].Cells["高度"].Value = dt_all_element.Rows[i]["Board_height"];
                    dg_element.Rows[index].Cells["宽度"].Value = dt_all_element.Rows[i]["Board_width"];
                    dg_element.Rows[index].Cells["订单号"].Value = dt_all_element.Rows[i]["Order_id"];
                    dg_element.Rows[index].Cells["工单号"].Value = dt_all_element.Rows[i]["Ap_id"];
                    dg_element.Rows[index].Cells["条形码"].Value = dt_all_element.Rows[i]["Code"];
                }

            }
        }

        /// <summary>更新全部整套组件表格</summary>
        private void Update_Sec_dgview_all()
        {
            string sql_select1 = "SELECT `Sec_id`,`Sec_series`,`Sec_model`,`Sec_color`,`Sec_thick`,`Order_id` FROM `order_section_online` WHERE `is_packed`=0 and `Sec_type`=1";
            DataSet ds_element1 = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select1, new MySqlParameter("@prodid", 24));
            DataTable dt_ds_element1 = ds_element1.Tables[0];
            dg_sec.Rows.Clear();
            if (dt_ds_element1.Rows.Count > 0)
            {

                for (int i = 0; i < dt_ds_element1.Rows.Count; i++)
                {

                    int index = dg_sec.Rows.Add();
                    dg_sec.Rows[index].Cells["Sec_series"].Value = dt_ds_element1.Rows[i]["Sec_series"];
                    dg_sec.Rows[index].Cells["Sec_model"].Value = dt_ds_element1.Rows[i]["Sec_model"];
                    dg_sec.Rows[index].Cells["Sec_color"].Value = dt_ds_element1.Rows[i]["Sec_color"];
                    dg_sec.Rows[index].Cells["Sec_thick"].Value = dt_ds_element1.Rows[i]["Sec_thick"];
                    dg_sec.Rows[index].Cells["Sec_code_id"].Value = dt_ds_element1.Rows[i]["Sec_id"];
                    dg_sec.Rows[index].Cells["Order_id_sec"].Value = dt_ds_element1.Rows[i]["Order_id"];
                    if (is_all_done)
                    {
                        bt_sec.Visible = true;
                        bt_sec.Enabled = false;
                        lb_sec.Visible = true;
                        lb_sec.Text = "";
                    }


                }


            }
        }

        private void dg_sec_by_order_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                lb_sec.Text = dg_sec_by_order["Sec_code_id2", e.RowIndex].Value.ToString();
                lb_sec_order_id.Text = dg_sec_by_order["Order_id_sec2", e.RowIndex].Value.ToString();
                if (lb_sec.Text.Contains("S"))
                {
                    bt_sec.Enabled = true;
                }

            }
        }

        private void lb_sec_order_id_Click(object sender, EventArgs e)
        {

        }

        private void Update_Sec_dgview_by_order(String Order_id)
        {
            string sql_select1 = "SELECT `Sec_id`,`Sec_series`,`Sec_model`,`Sec_color`,`Sec_thick`,`Order_id` FROM `order_section_online` WHERE `is_packed`=0 and `Sec_type`=1 and `Order_id`='" + Order_id + "'";
            DataSet ds_element1 = MySqlHelper.GetDataSet(mysqlStr_manufacture, CommandType.Text, sql_select1, new MySqlParameter("@prodid", 24));
            DataTable dt_ds_element1 = ds_element1.Tables[0];
            dg_sec_by_order.Rows.Clear();
            if (dt_ds_element1.Rows.Count > 0)
            {

                for (int i = 0; i < dt_ds_element1.Rows.Count; i++)
                {

                    int index = dg_sec_by_order.Rows.Add();
                    dg_sec_by_order.Rows[index].Cells["Sec_series2"].Value = dt_ds_element1.Rows[i]["Sec_series"];
                    dg_sec_by_order.Rows[index].Cells["Sec_model2"].Value = dt_ds_element1.Rows[i]["Sec_model"];
                    dg_sec_by_order.Rows[index].Cells["Sec_color2"].Value = dt_ds_element1.Rows[i]["Sec_color"];
                    dg_sec_by_order.Rows[index].Cells["Sec_thick2"].Value = dt_ds_element1.Rows[i]["Sec_thick"];
                    dg_sec_by_order.Rows[index].Cells["Sec_code_id2"].Value = dt_ds_element1.Rows[i]["Sec_id"];
                    dg_sec_by_order.Rows[index].Cells["Order_id_sec2"].Value = dt_ds_element1.Rows[i]["Order_id"];
                    if (is_all_done)
                    {
                        bt_sec.Visible = true;
                        bt_sec.Enabled = false;
                        lb_sec.Visible = true;
                        lb_sec.Text = "";
                    }


                }


            }
        }
    }
}
