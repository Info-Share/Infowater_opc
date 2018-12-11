using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LSE_ClientCS
{
    public partial class Form1 : Form
    {
        private const int DELAY_TIME = 1000;
        LSEOPC OPCServer;   

        public Form1()
        {
            InitializeComponent();
            
            OPCServer= new LSEOPC();
            OPCServer.LSE_Connect();

        }
        Thread thread1;
        private void button1_Click(object sender, EventArgs e)
        {
            List<int> t1Data = new List<int>();

            //Display items data
            if (OPCServer.iItemCount == 0) { }
               // MessageBox.Show("Read nodata .... Please confirm LSEOPC Server and Address (Devicename:Address)...");
            else
            { 
                thread1 = new Thread(() => StartSendData());
                thread1.Start();
            }
        } 
        public void StartSendData()
        {
            while (true)
            {
                List<int> t1Data = new List<int>();
                for (int i = 0; i < OPCServer.iItemCount; i++)
                {
                    t1Data.Add(Convert.ToInt16(OPCServer.Values[i]));
                }

                DbUtil dbutil = new DbUtil();
                dbutil.WriteDataBase(t1Data[0], t1Data[1], t1Data[2], t1Data[3]);

                Thread.Sleep(DELAY_TIME);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            thread1.Abort();
        }
    }
}
