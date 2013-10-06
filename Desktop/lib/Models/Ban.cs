using System;
using CloudsdaleWin7.lib.CloudsdaleLib;
using Newtonsoft.Json;
namespace CloudsdaleWin7.lib.Models
{
    public sealed class Ban : CloudsdaleResource
    {
        public Ban(string id) : base(id)
        {
        }

        /// <summary>
        /// The date at which the ban will expire
        /// </summary>
        [JsonProperty("due")]
        public DateTime? Due { get; set; }

        /// <summary>
        /// The date at which the ban was originally issued
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime? Issued { get; set; }

        /// <summary>
        /// The date at which the ban was last updated
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime? Updated { get; set; }

        /// <summary>
        /// The ID of the offending party
        /// </summary>
        [JsonProperty("offender_id")]
        public string OffenderId { get; set; }

        /// <summary>
        /// The ID of the enforcer who issued the ban
        /// </summary>
        [JsonProperty("enforcer_id")]
        public string EnforcerId { get; set; }

        /// <summary>
        /// The type of jurisdiction where the ban was enacted
        /// </summary>
        [JsonProperty("jurisdiction_type")]
        public string JurisdictionType { get; set; }

        /// <summary>
        /// The ID of the jurisdiction in which the ban was enacted
        /// </summary>
        [JsonProperty("jurisdiction_id")]
        public string JurisdictionId { get; set; }

        /// <summary>
        /// The offending user
        /// </summary>
        public User Offender
        {
            get { return Cloudsdale.UserProvider.GetUser(OffenderId); }
        }

        /// <summary>
        /// The enforcing user
        /// </summary>
        public User Enforcer
        {
            get { return Cloudsdale.UserProvider.GetUser(EnforcerId); }
        }

        /// <summary>
        /// The cloud where the ban was enacted
        /// </summary>
        public Cloud Jurisdiction
        {
            get { return Cloudsdale.CloudProvider.GetCloud(JurisdictionId); }
        }

        /// <summary>
        /// The given reason for the offense
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Whether the ban has been revoked
        /// </summary>
        [JsonProperty("revoke")]
        public bool? Revoked { get; set; }

        /// <summary>
        /// Whether the ban has expired
        /// </summary>
        [JsonProperty("has_expired")]
        public bool? Expired { get; set; }

        /// <summary>
        /// If the ban is still active
        /// </summary>
        [JsonProperty("is_active")]
        public bool? Active { get; set; }

        /// <summary>
        /// Bans cannot be validated
        /// </summary>
        /// <returns>Always returns false</returns>
        public override bool CanValidate()
        {
            return false;
        }
    }
}
