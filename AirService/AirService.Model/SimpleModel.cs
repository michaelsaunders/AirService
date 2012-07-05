using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AirService.Model
{
    public enum CmsStatus { Active = 1, Frozen = 2, Deleted = 3 };

    [DataContract]
    public abstract class SimpleModel
    {
        public const int StatusActive = (int)CmsStatus.Active,
            StatusFrozen = (int)CmsStatus.Frozen,
            StatusDeleted = (int)CmsStatus.Deleted;

        [Key]
        [DataMember(Name = "id")]
        public int Id { get; set; }

        public int Status { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        protected SimpleModel()
        {
            Status = StatusActive;
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
        }

        [NotMapped]
        [DataMember(Name="createDate")]
        public string UtcCreatedDate
        {
            get
            {
                return this.CreateDate.ToIso8061DateString();
            }
            private set
            {
                this.CreateDate = DateUtility.FromIso8061FormattedDateString(value);
            }
        }

        [NotMapped]
        [DataMember(Name = "updateDate")]
        public string UtcUpdateDate
        {
            get
            {
                return this.UpdateDate.ToIso8061DateString();
            }
            private set
            {
                this.UpdateDate = DateUtility.FromIso8061FormattedDateString(value);
            }
        } 
    } 
}