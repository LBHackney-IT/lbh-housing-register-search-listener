using System;
using System.Collections.Generic;
using System.Text;

namespace HousingRegisterSearchListener.Domain
{
    public class ApplicationSearchEntity
    {
        public Guid ApplicationId { get; set; }

        public string NationalInsuranceNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Title { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string Surname { get; set; }
        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string AssignedTo { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public bool SensitiveData { get; set; }

        public List<ApplicationOtherMembersSearchEntity> OtherMembers { get; set; }

        public bool HasAssessment { get; set; }

        public string Reference { get; set; }

        public int? BiddingNumber { get; set; }
    }
}
