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
    public delegate void ArztAddCallback(Arzt a);

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
            webclient.UploadValuesAsync(new Uri("http://www.kvberlin.de/60arztsuche/suche.php"), "POST", values, task);
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
            foreach (var line in textBox1.Lines)
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
                foreach (var item in ArztList)
                    item.InsertIntoDB(db);
            }

            save_btn.Enabled = false;
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
    }

    public class Workitem
    {
        public int Plz;
        public int Start;
        public bool recurse;

        public override string ToString()
        {
            if (recurse)
                return string.Format("{0} (Alle)", Plz);
            else
                return string.Format("{0} ({1})", Plz, Start / 10 + 1);
        }
    }

    public enum Gender
    {
        Male, Female
    }

    public class RegexDidNotMatchException : Exception
    {

    }

    public class Arzt
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Gender Geschlecht { get; set; }
        public List<string> Fach { get; set; }
        public List<string> Zusatz { get; set; }
        public bool Hausarzt { get; set; }
        public string Straße { get; set; }
        public int Plz { get; set; }
        public string Ort { get; set; }
        public string Telefon { get; set; }
        public string Telefax { get; set; }

        public event ArztAddCallback ParsingDone;


        private Regex rx_name_geschlecht = new Regex(@"<h1>(.+?)</h1>\s*<h4>niedergelassener? Kassen(\w+)(.*?)</h4>");
        private Regex rx_fach = new Regex(@"<p><b>Fachgebiete:</b></p><ul>(.+?)</ul>", RegexOptions.Singleline);
        private Regex rx_zusatz = new Regex(@"<p><b>Zusatzbezeichnungen:</b></p><ul>(.+?)</ul>", RegexOptions.Singleline);
        private Regex rx_listitems = new Regex(@"\s*<li>([^<]+)</li>");
        private Regex rx_hausarzt = new Regex(@"<p>Hausärztliche Versorgung: \w+</p>");
        private Regex rx_adresse = new Regex(@"<h2>(.+?)</h2>\s*<p>\s*(.+?)<br>\s*(\d+) (.+?)\s*</p>");
        private Regex rx_telefon = new Regex(@"Telefon-Nummer:\s+([\d/-]+)");
        private Regex rx_telefax = new Regex(@"Fax-Nummer:\s+([\d/-]+)");


        private string strip(string str)
        {
            return Regex.Replace(Regex.Replace(str, @"[\s]+", " "), @"<br>\s?", "\r\n").Trim();
        }

        public Arzt(int id, int plz)
        {
            ID = id;
            Plz = plz;
            Fach = new List<string>();
            Zusatz = new List<string>();
            RetrieveDetails();
        }

        public Arzt()
        {
            Fach = new List<string>();
            Zusatz = new List<string>();
        }

        public void RetrieveDetails()
        {
            var wc = new WebClient();
            wc.DownloadDataCompleted += ParseDetails;
            wc.DownloadDataAsync(new Uri("http://www.kvberlin.de/60arztsuche/detail1.php?id=" + ID));
        }

        private void ParseDetails(Object sender, DownloadDataCompletedEventArgs e)
        {
            var enc = Encoding.GetEncoding("iso-8859-1");
            string response = enc.GetString(e.Result);

            Match m;
            try
            {
                m = rx_name_geschlecht.Match(response);
                Name = strip(m.Groups[1].Value);
                switch (m.Groups[2].Value)
                {
                    case "arzt":
                        Geschlecht = Gender.Male;
                        break;
                    case "ärztin":
                        Geschlecht = Gender.Female;
                        break;
                    default:
                        throw new RegexDidNotMatchException();
                }

                m = rx_fach.Match(response, m.Index);
                var listitems = rx_listitems.Matches(m.Groups[1].Value);
                foreach (Match item in listitems)
                    Fach.Add(strip(item.Groups[1].Value));

                m = rx_zusatz.Match(response, m.Index);
                if (m.Success)
                {
                    listitems = rx_listitems.Matches(m.Groups[1].Value);
                    foreach (Match item in listitems)
                        Zusatz.Add(strip(item.Groups[1].Value));
                }
                m = rx_hausarzt.Match(response, m.Index);
                Hausarzt = m.Success;

                m = rx_adresse.Match(response, m.Index);

                Straße = strip(m.Groups[2].Value);
                Plz = int.Parse(m.Groups[3].Value);
                Ort = strip(m.Groups[4].Value);

                m = rx_telefon.Match(response, m.Index);
                Telefon = strip(m.Groups[1].Value);

                m = rx_telefax.Match(response, m.Index);
                if (m.Success)
                    Telefax = strip(m.Groups[1].Value);

                ParsingDone(this);
            }
            catch (Exception)
            {
                throw new RegexDidNotMatchException();
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", ID, Name);
        }

        // Fügt den Arzt in die Datenbank ein
        public bool InsertIntoDB(System.Data.OleDb.OleDbConnection db)
        {
            bool retVal = true;

            string strSQL = "INSERT INTO AerzteVZ " +
                            "('ArztName','Geschlecht','Fach','Zusatz','Hausarzt','Plz','Ort','Strasse','Tel','Fax','Link')" +
                            "VALUES ('" + Name + "','" + Geschlecht.ToString() + "','" + string.Join(", ", Fach) +
                            "','" + string.Join(", ", Zusatz) + "','" + (Hausarzt ? 'J' : 'N') + "'," + Plz.ToString() +
                            ",'" + Ort + "','" + Straße + "','" + Telefon + "','" + Telefax + "','" + ID.ToString() + "');";

            System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(strSQL, db);

            try
            {
                int zeilen = cmd.ExecuteNonQuery();
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Datensatz konnte nicht geschrieben werden!\n\n" + ex.Message);
                retVal = false;
            }

            return retVal;
        }

        // Öffnet einen File-Dialog um eine Datenbankdatei zu erstellen und zu öffnen.
        // Gibt das Handle zur geöffneten DB zurück oder NULL wenn keine DB geöffnet werden konnte.
        public static System.Data.OleDb.OleDbConnection createDB()
        {
            System.Data.OleDb.OleDbConnection db = null;    // Datenbank handle, muss noch geöffnet werden

            // File-dialog erstellen und anziegen lassen
            System.Windows.Forms.OpenFileDialog file = new OpenFileDialog();
            file.ValidateNames = true;
            file.Filter = "Access 2003 Datenbanken (*.mdb)|*.mdb";
            file.Title = "Datenbankdatei wählen...";

            // Wenn eine Datei gewähltwurde muss diese erstellt werden
            if (file.ShowDialog() == DialogResult.OK)
            {
                db = new System.Data.OleDb.OleDbConnection(
                                    @"Provider=Microsoft.Jet.OLEDB.4.0;
                                Data Source=" + file.FileName);
                db.Open();

                // SQL-Strin für die anzulegende tabelle
                string strSQL = "CREATE TABLE AerzteVZ ( 'id' AUTOINCREMENT PRIMARY KEY," +
                                                        "'ArztName' CHAR(255)," +
                                                        "'Geschlecht' CHAR(6)," +
                                                        "'Fach' CHAR(255)," +
                                                        "'Zusatz' CHAR(255)," +
                                                        "'Hausarzt' CHAR(6)," +
                                                        "'Plz' INTEGER," +
                                                        "'Ort' CHAR(255)," +
                                                        "'Strasse' CHAR(255)," +
                                                        "'Tel' CHAR(20)," +
                                                        "'Fax' CHAR(20)," +
                                                        "'Link' CHAR(255));";

                System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(strSQL, db);

                try
                {
                    int zeilen = cmd.ExecuteNonQuery();
                }
                catch (System.InvalidOperationException ex)
                {
                    MessageBox.Show("Fehler beim erstellen der Tabelle!\n\n" + ex.Message);
                    db.Close();
                    db = null;
                }
                catch
                {
                }
            }
            return db;
        }
    }
}
