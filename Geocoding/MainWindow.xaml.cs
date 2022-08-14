using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Geocoding
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Random _rand = new Random();
        private int _delay = 1010;
        private bool _abortFlag = false;


        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists("./API-Key.txt"))
            {
                apikey_txt.Text = File.ReadAllText("./API-Key.txt");
            }
        }

        private List<string> CleanInput()
        {
            HashSet<string> adresses = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            // Duplikate entfernen
            foreach (var addr in dest_txt.Text.Split('\n'))
                adresses.Add(addr.Trim());

            var list = adresses.ToList();
            list.Sort();
            dest_txt.Text = string.Join("\n", list);
            return list;
        }

        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(IEnumerable<T> source, int chunksize)
        {
            var pos = 0;
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(chunksize);
                pos += chunksize;
            }
        }

        private async void go_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mode_dist_rbtn.IsChecked == true)
                {
                    await CreateDistMatrix();
                }
                else
                {
                    await ResolveAdresses();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ResolveAdresses()
        {
            var addressList = CleanInput();
            progressbar.Value = 0;
            progressbar.Maximum = addressList.Count;
            _abortFlag = false;

            var resultList = new List<Place>();

            foreach (var addr in Chunk(addressList, 3))
            {
                var chunkResponse = addr.Select(async x => new Place { Adress = x, Coordinates = await LookupAddressAsync(x) }).ToArray();
                await Task.WhenAll(chunkResponse);
                resultList.AddRange(chunkResponse.Select(x => x.Result));
                ShowProgress(chunkResponse.Length);

                if (_abortFlag)
                    return;
            }

            var csvResult = resultList.Select(p => new
            {
                Ort = p.Adress,
                Koordinaten = p.Coordinates
            });

            using (var stream = new StreamWriter(savepath_txt.Text, false, Encoding.UTF8))
            {
                var csv = new CsvHelper.CsvWriter(stream, new CsvConfiguration(CultureInfo.CurrentUICulture) { Delimiter = ";", ShouldQuote = _ => true });
                csv.WriteRecords(csvResult);
            }
            delay_lbl.Content = "Fertig :-)";
        }

        private async Task CreateDistMatrix()
        {
            var addressList = CleanInput();
            var start = start_txt.Text;

            progressbar.Value = 0;
            // ReSharper disable once PossibleLossOfFraction
            progressbar.Maximum = addressList.Count;

            var tlist = new List<RouteResult[]>();
            _abortFlag = false;


            foreach (var addr in Chunk(addressList, 10))
            {
                tlist.Add(await LookupDistanceAsync(start, addr.ToArray()));
                ShowProgress(addr.Count());

                if (_abortFlag)
                    return;
            }

            var results = tlist.SelectMany(x => x).ToList();
            var csvResult = results.Select(r => new
            {
                Ziel = r.Destination,
                r.Status,
                Strecke_m = r.Distance,
                Fahrzeit_s = r.Duration,
                Koordinaten = r.Coordinates
            });

            using (var stream = new StreamWriter(savepath_txt.Text, false, Encoding.UTF8))
            {
                var csv = new CsvHelper.CsvWriter(stream, new CsvConfiguration(CultureInfo.CurrentUICulture) { Delimiter = ";", ShouldQuote = _ => true });
                csv.WriteRecords(csvResult);
            }
            delay_lbl.Content = "Fertig :-)";
        }

        private void ShowProgress(int count)
        {
            if (!CheckAccess())
                Dispatcher.Invoke(() => ShowProgress(count));
            else
            {
                progressbar.Value += count;
            }
        }


        private void SetDelay(double delay)
        {
            _delay = (int)delay;
            delay_lbl.Content = string.Format("Delay: {0}ms", _delay);
        }

        /// <summary>
        /// Schlägt mittels der Google API die Koordinaten einer bestimmten Adresse nach.
        /// </summary>
        /// <param name="addr">Eingangsadresse</param>
        /// <param name="retry">Wiederholversuche</param>
        /// <returns>Koordinaten als String</returns>
        private async Task<string> LookupAddressAsync(string addr, int retry = 2)
        {
            var apiUrl = "https://maps.googleapis.com/maps/api/geocode/xml?address={0}&region=de&bounds=47.2,6.1|54.9,14.8&key=" + apikey_txt.Text;

            await Task.Delay((int)(1.5 * _delay));

            var wr = WebRequest.CreateHttp(string.Format(apiUrl, addr));
            var response = await wr.GetResponseAsync();
            var document = XDocument.Load(response.GetResponseStream());
            try
            {
                var status = document.Element("GeocodeResponse")?.Element("status")?.Value ?? "";

                switch (status)
                {
                    case "OK":
                        var geom = document.Element("GeocodeResponse")?.Element("result")?.Element("geometry");
                        var loc = geom?.Element("location");
                        if (loc != null)
                            return string.Format("{0}, {1}", loc.Element("lat")?.Value, loc.Element("lng")?.Value);
                        else
                            return "Keine Koordinaten gefunden";

                    case "ZERO_RESULTS":
                        return "Nicht gefunden";

                    case "OVER_QUERY_LIMIT":
                        {
                            if (retry == 0)
                                return "Zuviele Zugriffe";
                            else
                                return await LookupAddressAsync(addr, retry - 1);
                        }
                    default:
                        return "Unbekannter Fehler";
                }
            }
            catch (NullReferenceException)
            {
                return "Unerwarteter Fehler";
            }
        }

        /// <summary>
        /// Schlägt die Distanz zwischen Adressen mittels der Google Distance Matrix API nach
        /// </summary>
        /// <param name="source">Anfangsort</param>
        /// <param name="dest">Zielort</param>
        /// <returns>Ein RouteResult, dass den Statuscode und die Distanz im metern enthält.</returns>
        private async Task<RouteResult[]> LookupDistanceAsync(string source, string[] destArr, bool resolveCoords = false, int retry = 5)
        {
            var apiUrl = "https://maps.googleapis.com/maps/api/distancematrix/xml?origins={0}&destinations={1}&mode=driving&language=de-DE&key={2}";

            var dest = string.Join("|", destArr);
            var response = await new WebClient().DownloadStringTaskAsync(string.Format(apiUrl, source, dest, apikey_txt.Text));
            if (response.Contains("error_message")) throw new Exception(response);
            //Debug.WriteLine(response);
            var document = XDocument.Parse(response);

            var status = document.Element("DistanceMatrixResponse")?.Element("status")?.Value ?? "";

            if (status == "OK")
            {
                var result = new RouteResult[destArr.Length];
                int idx = 0;
                foreach (var node in document.Element("DistanceMatrixResponse")?.Element("row")?.Elements("element"))
                {
                    var nodeStatus = node.Element("status")?.Value ?? "Fehler";
                    var dist = node.Element("distance")?.Element("value")?.Value ?? "0";
                    var dur = node.Element("duration")?.Element("value")?.Value ?? "0";
                    result[idx] = new RouteResult(uint.Parse(dist), uint.Parse(dur), nodeStatus, destArr[idx]);
                    idx++;
                }
                if (!resolveCoords)
                    return result;

                for (int i = 0; i < result.Length; i++)
                    result[i].Coordinates = await LookupAddressAsync(result[i].Destination);

                return result;
            }
            else if (retry > 0)
            {
                if (status == "OVER_QUERY_LIMIT")
                {
                    SetDelay(1.1 * _delay);
                }

                await Task.Delay(420);
                return await LookupDistanceAsync(source, destArr, resolveCoords, retry - 1);
            }
            else
                return new RouteResult[0];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var ownpath = AppDomain.CurrentDomain.BaseDirectory;

            savepath_txt.Text = Path.Combine(ownpath, "output.csv");
        }
    }

    public struct Place
    {
        public string Adress;
        public string Coordinates;
    }
}
