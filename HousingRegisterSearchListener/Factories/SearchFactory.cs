using HousingRegisterApi.V1.Domain;
using HousingRegisterSearchListener.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HousingRegisterSearchListener.Factories
{
    public static class SearchFactory
    {
        public static ApplicationSearchEntity ToSearch(this Application entity)
        {
            var search = new ApplicationSearchEntity
            {
                ApplicationId = entity.Id,
                AssignedTo = entity.AssignedTo,
                CreatedAt = entity.CreatedAt,
                DateOfBirth = entity.MainApplicant.Person.DateOfBirth,
                Title = entity.MainApplicant.Person.Title,
                FirstName = entity.MainApplicant.Person.FirstName,
                MiddleName = entity.MainApplicant.Person.MiddleName,
                Surname = entity.MainApplicant.Person.Surname,
                NationalInsuranceNumber = entity.MainApplicant.Person.NationalInsuranceNumber,
                SensitiveData = entity.SensitiveData,
                Status = entity.Status,
                SubmittedAt = entity.SubmittedAt,
                OtherMembers = GetOtherMembers(entity),
                HasAssessment = entity.Assessment !=null
            };

            return search;
        }

        private static List<ApplicationOtherMembersSearchEntity> GetOtherMembers(Application entity)
        {
            List<ApplicationOtherMembersSearchEntity> result = new List<ApplicationOtherMembersSearchEntity>();

            foreach (var otherMember in entity.OtherMembers)
            {
                ApplicationOtherMembersSearchEntity otherMemberEntity = new ApplicationOtherMembersSearchEntity
                {
                    DateOfBirth = otherMember.Person.DateOfBirth,
                    FirstName = otherMember.Person.FirstName,
                    Id = otherMember.Person.Id,
                    MiddleName = otherMember.Person.MiddleName,
                    NationalInsuranceNumber = otherMember.Person.NationalInsuranceNumber,
                    Surname = otherMember.Person.Surname
                };

                result.Add(otherMemberEntity);
            }

            return result;
        }
    }
}
