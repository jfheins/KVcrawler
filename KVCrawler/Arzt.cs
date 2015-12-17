using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace KVCrawler
{
	public delegate void ArztAddCallback(Arzt a);

	public class Arzt
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public Gender Geschlecht { get; set; }
		public List<string> Fach { get; set; }
		public List<string> Schwerp { get; set; }
		public List<string> Zusatz { get; set; }
		public bool Hausarzt { get; set; }
		public string Stra�e { get; set; }
		public int Plz { get; set; }
		public string Ort { get; set; }
		public string Telefon { get; set; }
		public string Telefax { get; set; }

		public event ArztAddCallback ParsingDone;


		private Regex rx_name_geschlecht = new Regex(@"<h1>(.+?)</h1>\s*<h4>niedergelassener? Kassen(\w+)(.*?)</h4>");
		private Regex rx_fach = new Regex(@"<p><b>Fachgebiete:</b></p><ul>(.+?)</ul>", RegexOptions.Singleline);
		private Regex rx_schwerp = new Regex(@"<p><b>Schwerpunkte:</b></p><ul>(.+?)</ul>", RegexOptions.Singleline);
		private Regex rx_zusatz = new Regex(@"<p><b>Zusatzbezeichnungen:</b></p><ul>(.+?)</ul>", RegexOptions.Singleline);
		private Regex rx_listitems = new Regex(@"\s*<li>([^<]+)</li>");
		private Regex rx_hausarzt = new Regex(@"<p>Haus�rztliche Versorgung: \w+</p>");
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
			Schwerp = new List<string>();
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
					case "�rztin":
						Geschlecht = Gender.Female;
						break;
					default:
						throw new RegexDidNotMatchException();
				}

				m = rx_fach.Match(response, m.Index);
				var listitems = rx_listitems.Matches(m.Groups[1].Value);
				foreach (Match item in listitems)
					Fach.Add(strip(item.Groups[1].Value));

				// neu
				m = rx_schwerp.Match(response, m.Index);
				if (m.Success)
				{
					listitems = rx_listitems.Matches(m.Groups[1].Value);
					foreach (Match item in listitems)
						Schwerp.Add(strip(item.Groups[1].Value));
				}
				// neu ende

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

				Stra�e = strip(m.Groups[2].Value);
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

		// F�gt den Arzt in die Datenbank ein
		public bool InsertIntoDB(System.Data.OleDb.OleDbConnection db)
		{
			bool retVal = true;

			string strSQL = "INSERT INTO AerzteVZ " +
			                "('ArztName','Geschlecht','Fach','Schwerpunkte','Zusatz','Hausarzt','Plz','Ort','Strasse','Tel','Fax','Link')" +
			                "VALUES ('" + Name + "','" + Geschlecht.ToString() + "','" + string.Join(", ", Fach) +"','" + string.Join(", ", Schwerp) +
			                "','" + string.Join(", ", Zusatz) + "','" + (Hausarzt ? 'J' : 'N') + "'," + Plz.ToString() +
			                ",'" + Ort + "','" + Stra�e + "','" + Telefon + "','" + Telefax + "','http://www.kvberlin.de/60arztsuche/detail1.php?id=" + ID.ToString() + "');";

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

		// �ffnet einen File-Dialog um eine Datenbankdatei zu erstellen und zu �ffnen.
		// Gibt das Handle zur ge�ffneten DB zur�ck oder NULL wenn keine DB ge�ffnet werden konnte.
		public static System.Data.OleDb.OleDbConnection createDB()
		{
			System.Data.OleDb.OleDbConnection db = null;    // Datenbank handle, muss noch ge�ffnet werden

			// File-dialog erstellen und anziegen lassen
			System.Windows.Forms.OpenFileDialog file = new OpenFileDialog();
			file.ValidateNames = true;
			file.Filter = "Access 2003 Datenbanken (*.mdb)|*.mdb";
			file.Title = "Datenbankdatei w�hlen...";

			// Wenn eine Datei gew�hltwurde muss diese erstellt werden
			if (file.ShowDialog() == DialogResult.OK)
			{
				db = new System.Data.OleDb.OleDbConnection(
					@"Provider=Microsoft.Jet.OLEDB.4.0;
                                Data Source=" + file.FileName);
				db.Open();

				// SQL-Strin f�r die anzulegende tabelle
				string strSQL = "CREATE TABLE AerzteVZ ( 'id' AUTOINCREMENT PRIMARY KEY," +
				                "'ArztName' CHAR(255)," +
				                "'Geschlecht' CHAR(6)," +
				                "'Fach' CHAR(255)," +
				                "'Schwerpunkte' CHAR(255)," +
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