using System.ComponentModel.DataAnnotations;

namespace StromDbLib;
public class Stromverbrauch
{
    [Key]
    public int StromverbrauchId { get; set; }

    public DateTime Zeitpunkt { get; set; }
    public bool IsWaermepumpe { get; set; }
    public double Verbrauch { get; set; }
}
