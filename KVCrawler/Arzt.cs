using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;

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
        public string Fachgebiete => string.Join("; ", Fach);
        public string Schwerpunkte => string.Join("; ", Schwerp);
        public string Zusatzbezeichner => string.Join("; ", Zusatz);

        public bool Hausarzt { get; set; }
		public string Straﬂe { get; set; }
		public int Plz { get; set; }
		public string Ort { get; set; }
		public string Telefon { get; set; }
		public string Telefax { get; set; }
        public List<string> QsLeistungen { get; set; }

        public event ArztAddCallback ParsingDone;


		private Regex rx_name_geschlecht = new Regex(@"<h1>(.+?)</h1>\s*<h4>niedergelassener? Kassen(\w+)(.*?)</h4>");
		private Regex rx_fach = new Regex(@"<p><b>Fachgebiete:</b></p><ul>(.+?)</ul>", RegexOptions.Singleline);
		private Regex rx_schwerp = new Regex(@"<p><b>Schwerpunkte:</b></p><ul>(.+?)</ul>", RegexOptions.Singleline);
		private Regex rx_zusatz = new Regex(@"<p><b>Zusatzbezeichnungen:</b></p><ul>(.+?)</ul>", RegexOptions.Singleline);
		private Regex rx_listitems = new Regex(@"\s*<li>([^<]+)</li>");
		private Regex rx_hausarzt = new Regex(@"<p>Haus‰rztliche Versorgung: \w+</p>");
		private Regex rx_adresse = new Regex(@"<h2>(.+?)</h2>\s*<p>\s*(.+?)<br>\s*(\d+) (.+?)\s*</p>");
		private Regex rx_telefon = new Regex(@"Telefon-Nummer:\s+([\d/-]+)");
		private Regex rx_telefax = new Regex(@"Fax-Nummer:\s+([\d/-]+)");


		private string strip(string str)
		{
			return Regex.Replace(Regex.Replace(str, @"[\s]+", " "), @"<br>\s?", "\r\n").Trim();
		}

		public Arzt(int id, int plz) : this()
		{
			ID = id;
			Plz = plz;
		}

		public Arzt()
		{
			Fach = new List<string>();
			Zusatz = new List<string>();
			Schwerp = new List<string>();
            QsLeistungen = new List<string>();
        }

		public async Task RetrieveDetailsAsync()
		{
            // Allgemeine Details
			var wc = new WebClient();
            wc.Encoding = Encoding.GetEncoding("iso-8859-1");

            var response = await wc.DownloadStringTaskAsync(new Uri("https://www.kvberlin.de/60arztsuche/detail1.php?id=" + ID)).ConfigureAwait(false);
            ParseDetails(response);

            // Qualifizierte Leistungen
            response = await wc.DownloadStringTaskAsync(new Uri("https://www.kvberlin.de/60arztsuche/detail3.php?id=" + ID)).ConfigureAwait(false);
            ParseDetails2(response);

            ParsingDone(this);
        }

		private void ParseDetails(string response)
		{
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
					case "‰rztin":
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

				Straﬂe = strip(m.Groups[2].Value);
				Plz = int.Parse(m.Groups[3].Value);
				Ort = strip(m.Groups[4].Value);

				m = rx_telefon.Match(response, m.Index);
				Telefon = strip(m.Groups[1].Value);

				m = rx_telefax.Match(response, m.Index);
				if (m.Success)
					Telefax = strip(m.Groups[1].Value);
			}
			catch (Exception)
			{
				throw new RegexDidNotMatchException();
			}
		}


        private void ParseDetails2(string response)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(response);

            var collection = doc.DocumentNode.SelectNodes("//tr/td[1]");

            foreach (var tag in collection)
            {
                if (tag.GetClasses().Any(x => x == "tabletext"))
                {
                    QsLeistungen.Add(strip(tag.FirstChild.InnerText));
                }
            }
        }


        public override string ToString()
		{
			return string.Format("{0} - {1}", ID, Name);
		}
	}
}