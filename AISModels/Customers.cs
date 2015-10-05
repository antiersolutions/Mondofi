using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;


namespace AISModels
{
    [Table("Customers")]
    public class Customers
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int64 CustomerId { get; set; }

        public DateTime DateCreated { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime Anniversary { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public int? CityId { get; set; }
        public virtual Cities Cities { get; set; }

        public string CityName { get; set; }

        public string Notes { get; set; }

        public string PhotoPath { get; set; }

        public virtual ICollection<CustomersPhoneNumbers> PhoneNumbers { get; set; }
        public virtual ICollection<CustomersEmails> Emails { get; set; }
        public virtual ICollection<CustomerSpecialStatus> SpecialStatus { get; set; }
        public virtual ICollection<CustomersAllergies> Allergies { get; set; }
        public virtual ICollection<CustomersRestrictions> Restrictions { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
        public virtual ICollection<Waiting> Waitings { get; set; }
    }
}
