
using System.Net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkTrafficKiller
{
    public partial class Form1 : Form
    {
        private WebRequest httpRequest;
        private WebResponse httpResponse;
        private byte[] buffer;
        private Thread[] downloadThread;
        private long length;
        private long[] downlength;
        private long[] lastlength;
        //private FileStream fs;
        private int[] totalseconds; //总用时
        private int threadNumber = 1;
        public delegate void updateData(string value);
        private String link;
        private float[] speeds;
        private bool run;
        private int[] restart;
        private double sumNetTraffic; // 记录MB
        private String startTime;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            buffer = new byte[100000];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sumNetTraffic = 0;
            this.run = true;
            link = richTextBox1.Text;
            threadNumber = Int32.Parse(this.textBox1.Text);

            this.timer1.Enabled = true;
            DateTime now = DateTime.Now;
            startTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            

            downloadThread = new Thread[threadNumber];
            downlength = new long[threadNumber];
            lastlength = new long[threadNumber];
            totalseconds = new int[threadNumber];
            speeds = new float[threadNumber];
            restart = new int[threadNumber];
            for (int i = 0; i < threadNumber; i++)
            {
                string nag = Convert.ToString(i);
                downloadThread[i] = new Thread(new ParameterizedThreadStart(downloadFile));
                downloadThread[i].Start(nag);
            }



        }
        private void downloadFile(object arg)
        {
            int nag = Convert.ToInt32(arg);
            while (this.run)
            {
                totalseconds[nag] = 0;
                downlength[nag] = 0;
                lastlength[nag] = 0;
                restart[nag] = 0;
                try
                {
                    httpRequest = WebRequest.Create(link);
                    httpResponse = httpRequest.GetResponse();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("链接无法访问，请更换链接！！！\n\r 以下用于Debug，用户请忽视:\n\r"+ex.ToString());
                    this.richTextBox2.Text = "链接无法访问，请更换链接！！！";
                    this.timer1.Stop();
                    return;
                }
               
                Stream ns = httpResponse.GetResponseStream();
                int i;
                try
                {
                    while ((i = ns.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (!run) return;
                        if (restart[nag] > 5)
                        {
                            // System.Diagnostics.Debug.WriteLine(Convert.ToString(i)+"重启"); 
                            break;
                        };
                        downlength[nag] += i;
                    }
                }
                catch
                {
                    this.richTextBox2.Text = this.richTextBox2.Text + Convert.ToString(nag)+"连接被关闭";
                }
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            this.richTextBox2.Text = "";
            String tmp = "";
            float sum = 0;
            float restartLimit = float.Parse(this.textBox2.Text);
            for (int i = 0; i < threadNumber; i++)
            {
                speeds[i] = ((downlength[i] - lastlength[i]) / 1024)/1024; // MB/s
                lastlength[i] = downlength[i];
                totalseconds[i]++;

                sumNetTraffic += speeds[i];

                sum += speeds[i];
                if (this.textBox2.Text!="0" && speeds[i] < restartLimit) 
                    restart[i]++; 
                else
                {
                    if (speeds[i] > 1) restart[i]--;
                }
                
                tmp += "线程 "+Convert.ToString(i+1)+" ："+speeds[i]+"MB/s";
                if (restart[i] > 5) tmp += "\t\t重连接";
                tmp += "\n"; 
            }
            
            tmp += "总流量 "+Convert.ToString(ToFixed(sumNetTraffic / 1024,2))+" GB\t" +"("+Convert.ToString(sumNetTraffic)+" MB)"+"\n";
            tmp += "开始于" + startTime;
            this.richTextBox2.Text = tmp;
            this.SpeedLabel.Text = Convert.ToString(sum) + "MB/s";
            if (this.textBox3.Text != "0" && sumNetTraffic > float.Parse(this.textBox3.Text) * 1024)
            {
                DateTime now = DateTime.Now;
                this.richTextBox2.Text = this.textBox3.Text + "GB 测量完毕" + now.ToString("yyyy-MM-dd HH:mm:ss");
                button2_Click(this, null);
            }
        }
        public static double ToFixed(double d, int s)
        {
            double sp = Math.Pow(10, s);

            if (d < 0)
                return Math.Truncate(d) + Math.Ceiling((d - Math.Truncate(d)) * sp) / sp;
            else
                return Math.Truncate(d) + Math.Floor((d - Math.Truncate(d)) * sp) / sp;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.timer1.Stop();
            for (int i = 0; i < threadNumber; i++)
            {
                this.run = false;
                Thread.Sleep(10);
            }
            DateTime now = DateTime.Now;
            this.richTextBox2.Text = "开始于"+startTime+"\n总流量 " + Convert.ToString(ToFixed(sumNetTraffic / 1024, 2)) + " GB\t" + " 停止 " + now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            button2_Click(this, null);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/lvxiangyu11/NetWorkTest");
            }
            catch
            {
                DialogResult MsgBoxResult;
                MsgBoxResult = System.Windows.Forms.MessageBox.Show("无法自动打开网页，是否将本软件网址复制到剪切板。", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
                if (MsgBoxResult.ToString() == "Yes")//如果对话框的返回值是YES（按"Y"按钮）
                {
                    Clipboard.SetDataObject("https://github.com/lvxiangyu11/NetWorkTest");
                }
                if (MsgBoxResult.ToString() == "No")//如果对话框的返回值是NO（按"N"按钮）
                {
                    
                }

            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("本软件用于网络测速，完全免费，可从我的github上进行下载最新款，我会按需求更新一些新功能。\n当重连接次数过多，请减小线程数或减小重连接阈值，或检测链接源的访问性。\n本软件仅用于网络测试，请勿用于非法用途。\n由于本软件为单机软件，本人无法控制任何本软件的副本，特此声明。\n v0.1 2022年4月16日");
        }
    }

}