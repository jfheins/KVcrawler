using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.IO;

namespace KVCrawler
{
    public partial class Form1 : Form
    {
        BindingList<Arzt> ArztList;
        Stack<Workitem> todo;
        bool proceed;
        int threads;
        System.Data.OleDb.OleDbConnection db = null;

        Regex rx_treffer = new Regex(@"<p>ergab (\d+) Treffer");
        Regex rx_arzt = new Regex(@"<tr>\s*<td class=""tabletext""><a href=""detail1.php\?id=(\d+)&go=0&Arztdataberechtigung=""");

        public Form1()
        {
            InitializeComponent();
            ArztList = new BindingList<Arzt>();
            listBox1.DataSource = ArztList;
            todo = new Stack<Workitem>();
        }

        private void StartRequest(Workitem task)
        {
            var webclient = new WebClient();
            webclient.UploadValuesCompleted += RequestDone;
            NameValueCollection values = new NameValueCollection();
            values.Add("PLZ", task.Plz.ToString());
            values.Add("start", task.Start.ToString());
            values.Add("Psychotherapeut", "na");
            webclient.UploadValuesAsync(new Uri("https://www.kvberlin.de/60arztsuche/suche.php"), "POST", values, task);
            threads++;
        }


        private void RequestDone(Object sender, UploadValuesCompletedEventArgs e)
        {
            var enc = Encoding.GetEncoding("iso-8859-1");
            string response = enc.GetString(e.Result);

            Workitem info = (Workitem)e.UserState;

            if (info.recurse)
            {
                var m = rx_treffer.Match(response);
                if (m.Success)
                {
                    var num = int.Parse(m.Groups[1].Value);

                    for (int i = 11; i <= num; i += 10)
                        todo.Push(new Workitem { Plz = info.Plz, Start = i, recurse = false });
                }
            }

            var treffer = rx_arzt.Matches(response);
            foreach (Match match in treffer)
            {
                new Arzt(int.Parse(match.Groups[1].Value), info.Plz).ParsingDone += AddArzt;
            }

            threads--;
            while (proceed && (threads < (int)threads_num.Value) && todo.Count > 0)
            {
                StartRequest(todo.Pop());
            }
            UpdateThreads();
        }

        internal void AddArzt(Arzt a)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => AddArzt(a)));
            }
            else
            {
                ArztList.Add(a);

                if (db == null)
                {
                    save_btn.Enabled = true;
                }
                else
                {
                    a.InsertIntoDB(db);
                }
            }
        }

        private void UpdateThreads()
        {
            threads_lbl.Text = string.Format("({0} running)", threads);

            bool running = threads > 0;
            start_btn.Enabled = !running;
            stopp_btn.Enabled = running;
            progressBar1.Style = running ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
        }

        private void start_btn_Click(object sender, EventArgs e)
        {
            foreach (var line in textBox1.Lines.Reverse())
            {
                todo.Push(new Workitem() { Plz = int.Parse(line.Substring(0, 5)), Start = 1, recurse = true });
            }

            if (todo.Count > 0)
            {
                proceed = true;
                threads = 0;
                StartRequest(todo.Pop());
                UpdateThreads();
            }
            start_btn.Enabled = false;
        }

        private void stopp_btn_Click(object sender, EventArgs e)
        {
            proceed = false;
        }



        private void save_btn_Click(object sender, EventArgs e)
        {
            //var xml = new XmlSerializer(typeof(BindingList<Arzt>), new Type[] { typeof(Arzt), typeof(List<string>), typeof(Gender) });
            //var fs = new StreamWriter(@"D:\daten.txt");
            //xml.Serialize(fs, ArztList);
            //fs.Close();


            db = Arzt.createDB();
            if (db != null)
            {
                save_btn.Enabled = false;

                foreach (var item in ArztList)
                    item.InsertIntoDB(db);
            }
            else
            {
                save_btn.Enabled = true;
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (db != null)
                db.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            start_btn.Enabled = true;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
                return;

            var a = ArztList[listBox1.SelectedIndex];

            lbl_name.Text = a.Name;
            lbl_plz.Text = a.Plz.ToString();
            lbl_gender.Text = a.Geschlecht.ToString();
            lbl_fach.Text = string.Join(", ", a.Fach);
            lbl_schwerp.Text = string.Join(", ", a.Schwerp);
            lbl_zusatz.Text = string.Join("\r\n", a.Zusatz);
        }
    }

    public enum Gender
    {
        Male, Female
    }

    public class RegexDidNotMatchException : Exception
    {

    }
}
