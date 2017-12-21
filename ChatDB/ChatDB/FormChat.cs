﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatDB
{
    public partial class FormChat : Form
    {
        public FormChat()
        {
            InitializeComponent();
        }

        public string ConString = "", UserName = ""; 

        chatdbdatabaseEntities cdbentity = new chatdbdatabaseEntities();

        //speed of the chat refresh
        decimal interval;

        bool shift = false;
        bool enter = false;
        int id;

        private void FormChat_Load(object sender, EventArgs e)
        {
            cdbentity.Database.Connection.ConnectionString = ConString;
            id = CheckID();
            RefreshData();

            rich_chat.SelectionStart = rich_chat.Text.Length;
            rich_chat.ScrollToCaret();

            tmr_refresh.Start();
        }

        private void RefreshData()
        {
            rich_chat.Clear();

            foreach (chatdb cdb in cdbentity.chatdb)
            {
                rich_chat.Text = rich_chat.Text + cdb.username + " - " + cdb.date + ": " + cdb.message + "\n";
            }
        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            if (!txt_message.Text.Trim().Equals(""))
            {
                try
                {
                    chatdb tablecdb = new chatdb();
                    tablecdb.mid = CheckID();
                    tablecdb.username = UserName;
                    tablecdb.date = DateTime.Now;
                    tablecdb.message = txt_message.Text;

                    cdbentity.chatdb.Add(tablecdb);

                    cdbentity.SaveChanges();

                    id = CheckID();

                    RefreshData();

                    rich_chat.SelectionStart = rich_chat.Text.Length;
                    rich_chat.ScrollToCaret();

                    txt_message.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private int CheckID()
        {
            try
            {
                return cdbentity.chatdb.Max(x => x.mid) + 1;
            }
            catch
            {
                return 0;
            }
        }

        private void txt_message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                shift = true;
            }

            if (e.KeyCode == Keys.Enter)
            {
                 enter = true;
            }

            if (enter && shift)
            {
                
            }
            else if (enter)
            {
                btn_send.PerformClick();
            }
        }

        private void intervalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //set interval with form interval
            FormInterval finterval = new FormInterval();
            if (finterval.ShowDialog() == DialogResult.OK)
            {
                tmr_refresh.Stop();

                interval = finterval.Interval;

                tmr_refresh.Interval = Convert.ToInt32(interval*1000);
                tmr_refresh.Start();
            }

            finterval.Dispose();
        }

        private void tmr_refresh_Tick(object sender, EventArgs e)
        {
            if (CheckID() != id)
            {
                id = CheckID();
                RefreshData();
            }
        }

        private void txt_message_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                shift = false;
            }

            if (e.KeyCode == Keys.Enter)
            {
                enter = false;
            }
        }
    }
}
