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
            int biddingNumber = Convert.ToInt32(entity.Assessment?.BiddingNumber);
            var search = new ApplicationSearchEntity
            {
                ApplicationId = entity.Id,
                AssignedTo = entity.AssignedTo,
                CreatedAt = entity.CreatedAt,
                DateOfBirth = entity?.MainApplicant?.Person?.DateOfBirth ?? DateTime.MinValue,
                Title = entity?.MainApplicant?.Person?.Title,
                FirstName = entity?.MainApplicant?.Person?.FirstName,
                MiddleName = entity?.MainApplicant?.Person?.MiddleName,
                Surname = entity?.MainApplicant?.Person?.Surname,
                NationalInsuranceNumber = entity?.MainApplicant?.Person?.NationalInsuranceNumber,
                SensitiveData = entity?.SensitiveData ?? false,
                Status = EnsureConsistentEnumValue(entity?.Status),
                SubmittedAt = entity?.SubmittedAt ?? DateTime.MinValue,
                OtherMembers = GetOtherMembers(entity),
                HasAssessment = entity?.Assessment != null,
                BiddingNumber = biddingNumber == 0 ? (int?) null : biddingNumber,
                Reference = entity.Reference
            };

            return search;
        }

        private static string EnsureConsistentEnumValue(string status)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Length > 1)
                {
                    return char.ToUpper(status[0]) + status.Substring(1);
                }
                else
                {
                    return status;
                }
            }
            else
            {
                return status;
            }
        }

        private static List<ApplicationOtherMembersSearchEntity> GetOtherMembers(Application entity)
        {
            List<ApplicationOtherMembersSearchEntity> result = new List<ApplicationOtherMembersSearchEntity>();

            foreach (var otherMember in entity.OtherMembers)
            {
                ApplicationOtherMembersSearchEntity otherMemberEntity = new ApplicationOtherMembersSearchEntity
                {
                    DateOfBirth = otherMember?.Person?.DateOfBirth ?? DateTime.MinValue,
                    FirstName = otherMember?.Person?.FirstName,
                    Id = otherMember.Person?.Id ?? Guid.Empty,
                    MiddleName = otherMember?.Person?.MiddleName,
                    NationalInsuranceNumber = otherMember?.Person?.NationalInsuranceNumber,
                    Surname = otherMember?.Person?.Surname
                };

                result.Add(otherMemberEntity);
            }

            return result;
        }
    }
}
