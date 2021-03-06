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

        private void btn_send_Click(object sender, EventArgs e)
        {
            if (!rich_message.Text.Trim().Equals(""))
            {
                try
                {
                    chatdb tablecdb = new chatdb();
                    tablecdb.mid = CheckID();
                    tablecdb.username = UserName;
                    tablecdb.date = DateTime.Now;
                    tablecdb.message = rich_message.Text;

                    cdbentity.chatdb.Add(tablecdb);

                    cdbentity.SaveChanges();

                    RefreshData();
                    id = CheckID();

                    rich_chat.SelectionStart = rich_chat.Text.Length;
                    rich_chat.ScrollToCaret();

                    rich_message.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void rich_message_KeyDown(object sender, KeyEventArgs e)
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
                e.SuppressKeyPress = true; //normal enter should not add a new line, supress the key press and only use it to send message
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

        private int CheckID()
        {
            try
            {
                //get the biggest id in the table and and 1 to it
                return cdbentity.chatdb.Max(x => x.mid) + 1;
            }
            catch
            {
                //if the table is empty first id is 0
                return 0;
            }
        }

        private void RefreshData()
        {
            //take the last 30 messages from the chat table, if there are not 30 available take all
            var getlastmessages = (cdbentity.chatdb.Count()<30)? cdbentity.chatdb.OrderByDescending(x => x.mid):cdbentity.chatdb.OrderByDescending(x => x.mid).Take(30);
            getlastmessages = getlastmessages.OrderBy(x => x.mid); //then order ascending again

            //add new messages
            foreach (chatdb cdb in getlastmessages)
            {
                if(!rich_chat.Text.Contains(cdb.username + " - " + cdb.date + ": " + cdb.message)) //if the chat box doesn't contain that message yet, add it
                    rich_chat.Text += cdb.username + " - " + cdb.date + ": " + cdb.message + "\n";               
            }            
        }

        #region refresh tick

        private void tmr_refresh_Tick(object sender, EventArgs e)
        {
            Task trefresh = new Task(() => taskRefresh()); //starts a new task, so the ui thread doesn't lag
            trefresh.Start();
        }

        private void taskRefresh()
        {
            if (CheckID() != id) //check id isn't local id, means there is/are new messages
            {
                id = CheckID();

                this.rich_chat.BeginInvoke((MethodInvoker)delegate ()
                {
                    //when the method is starded with a task, an invoke is required

                    //take the last 30 messages from the chat table, if there are not 30 available take all
                    var getlastmessages = (cdbentity.chatdb.Count() < 30) ? cdbentity.chatdb.OrderByDescending(x => x.mid) : cdbentity.chatdb.OrderByDescending(x => x.mid).Take(30);

                    foreach (chatdb cdb in getlastmessages.OrderBy(x => x.mid))
                    {
                        if (!rich_chat.Text.Contains(cdb.username + " - " + cdb.date + ": " + cdb.message)) //if the chat box doesn't contain that message yet, add it
                            rich_chat.Text += cdb.username + " - " + cdb.date + ": " + cdb.message + "\n";
                    }

                    rich_chat.SelectionStart = rich_chat.Text.Length;
                    rich_chat.ScrollToCaret();
                });
            }
        }

        #endregion

        private void rich_message_KeyUp(object sender, KeyEventArgs e)
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
