namespace Geocoding
{
	public struct RouteResult
	{
		/// <summary>
		/// Die Wegstrecke in Metern oder null, falls keine Route ermittelt wurde.
		/// </summary>
		public readonly uint? Distance;

		/// <summary>
		/// Die berechnete Dauer in Sekunden oder null, falls keine Route ermittelt wurde.
		/// </summary>
		public readonly uint? Duration;

		/// <summary>
		///   OK: Die Antwort enth�lt ein g�ltiges result.
		///   NOT_FOUND: gibt an, dass mindestens einer der Standorte, die in der Anfrage als Startort, Zielort oder Wegpunkt angegeben wurden, nicht geocodiert werden konnte.
		///   ZERO_RESULTS: gibt an, dass keine Route zwischen Start- und Zielort gefunden werden konnte.
		///   MAX_WAYPOINTS_EXCEEDED: gibt an, dass zu viele waypoints in der Anfrage angegeben wurden. Die maximale Anzahl an waypoints betr�gt 8, plus Start- und Zielort. Kunden mit dem Google Maps API f�r Unternehmen k�nnen Anfragen mit bis zu 23 Wegpunkten ausf�hren.
		///   INVALID_REQUEST: gibt an, dass die Anfrage ung�ltig war.
		///   OVER_QUERY_LIMIT: gibt an, dass der Dienst innerhalb des zul�ssigen Zeitraums zu viele Anfragen von Ihrer Anwendung erhalten hat.
		///   REQUEST_DENIED: gibt an, dass der Dienst die Verwendung des Routenplanerdiensts durch Ihre Anwendung abgelehnt hat.
		///   UNKNOWN_ERROR: gibt an, dass eine Routenanfrage aufgrund eines Serverfehlers nicht verarbeitet werden konnte. Die Anfrage liefert m�glicherweise ein Ergebnis, wenn Sie es erneut versuchen.
		/// </summary>
		public string Status;

		public string Coordinates { get; set; }
		public string Destination { get; set; }

		public RouteResult(uint? distance, uint? duration, string status)
		{
			Distance = distance;
			Duration = duration;
			Status = status;
			Coordinates = "";
			Destination = "";
		}
		public RouteResult(uint? distance, uint? duration, string status, string dest) : this(distance, duration, status)
		{
			Destination = dest;
		}

		public override string ToString()
		{
			return Distance.HasValue ? string.Format("{0} m", Distance) : string.Format("N/A ({0})", Status);
		}
	}
}