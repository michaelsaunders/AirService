using System;
using System.ComponentModel.DataAnnotations;
using AirService.Model.Eway;

namespace AirService.Model
{
    public class VenueTransaction : IRebillTransaction
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string RebillId { get; set; }

        public int VenueId { get; set; }

        [MaxLength(256)]
        public string Note { get; set; }

        [Column("Amount")]
        public decimal TransactionAmount { get; set; }

        public int Status { get; set; }

        #region IRebillTransaction Members

        public DateTime TransactionDate { get; set; }

        TransactionStatus IRebillTransaction.Status
        {
            get { return (TransactionStatus) this.Status; }
            set { this.Status = (int) value; }
        }

        [MaxLength(100)]
        public string TransactionNumber { get; set; }

        string IRebillTransaction.TransactionError
        {
            get { return this.Note; }
            set { this.Note = value; }
        }

        public virtual Venue Venue { get; set; }

        #endregion
    }
}
