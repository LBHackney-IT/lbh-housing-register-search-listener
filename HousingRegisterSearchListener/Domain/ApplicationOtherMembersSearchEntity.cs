using System;
using System.Collections.Generic;
using System.Text;

namespace HousingRegisterSearchListener.Domain
{
    public class ApplicationOtherMembersSearchEntity
    {

        public Guid Id { get; set; }
        public string NationalInsuranceNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        public String FirstName { get; set; }

        public string MiddleName { get; set; }

        public string Surname { get; set; }
    }
}
