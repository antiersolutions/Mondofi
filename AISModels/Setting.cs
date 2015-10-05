using AIS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace AISModels
{
    [Table("Setting")]
    public class Setting
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        [Key]
        public int SettingId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public Int64? UpdatedBy { get; set; }
        
        [ForeignKey("UpdatedBy")]
        public virtual UserProfile UpdatedByUser { get; set; }
    }
}
