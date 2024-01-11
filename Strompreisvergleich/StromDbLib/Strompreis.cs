using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StromDbLib;
public class Strompreis
{
    [Key]
    public int StrompreisId { get; set; }

    public DateTime Von { get; set; }
    public DateTime Bis { get; set; }
    public double Preis { get; set; }
}
