using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GraniteHouse.Models
{
    public class ProductSelectedForAppointment
    {
        public int Id { get; set; }

        public int ProductsId { get; set; }

        [ForeignKey("ProductsId")]
        public virtual Products Products { get; set; }

        public int AppointmentsId { get; set; }

        [ForeignKey("AppointmentsId")]
        public virtual Appointments Appointments { get; set; }
    }
}
