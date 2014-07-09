using System.Collections.Generic;

namespace Tu.Washing
{
    public class WashingOutputColumnMapping : Dictionary<string, string>
    {
        public WashingOutputColumnMapping()
        {
            //Add("Filler Code 1", "");
            //Add("Record Type", "");
            //Add("Filler Code 2", "");
            //Add("Primary Sort Key", "");
            //Add("Sec Sort Key", "");
            //Add("Tertiary Sort Key", "");
            //Add("Match Type", "");
            //Add("Performance Indicator", "");
            Add("Client Ref No 1", "DataProviderID");
            //Add("Client Ref No 2", "");
            Add("Surname", "Surname");
            Add("Forename 1", "FirstNames");
            Add("Forename 2", "FirstNames");
            Add("Forename 3", "FirstNames");
            //Add("DOB", "");
            Add("RSA ID", "GovID");
            //Add("Other ID", "");
            Add("Gender", "Gender");
            Add("Title", "Title");
            Add("Marital Status", "MaritalStatus");
            Add("Spouse Name", "SpouseName");
            Add("ITCFirstAddressUpdateDate", "Address1DataProviderLastUpdated");
            Add("ITCFirstAddressYearsOfTenure", "Address1YearsOfTenure");
            Add("ITCFirstAddressLine1", "Address1Line1");
            Add("ITCFirstAddressLine2", "Address1Line2");
            Add("ITCFirstAddressLine3", "Address1Line3");
            Add("ITCFirstAddressLine4", "Address1Line4");
            Add("ITCFirstAddressPostCode", "Address1PostalCode");
            Add("ITCFirstAddressProvCode", "Address1Province");
            Add("ITCFirstAddressCountryCode", "Address1Country");
            Add("ITCFirstAddresOTInd", "Address1OwnershipStatus");
            Add("ITCSecondAddressUpdateDate", "Address2DataProviderLastUpdated");
            Add("ITCSecondAddressYearsOfTenure", "Address2YearsOfTenure");
            Add("ITCSecondAddressLine1", "Address2Line1");
            Add("ITCSecondAddressLine2", "Address2Line2");
            Add("ITCSecondAddressLine3", "Address2Line3");
            Add("ITCSecondAddressLine4", "Address2Line4");
            Add("ITCSecondAddressPostCode", "Address2PostalCode");
            Add("ITCSecondAddressProvCode", "Address2Province");
            Add("ITCSecondAddressCountryCode", "Address2Country");
            Add("ITCSecondAddressOTInd", "Address2OwnershipStatus");
            Add("ITCThirdAddressUpdateDate", "Address3DataProviderLastUpdated");
            Add("ITCThirdAddressYearsOfTenure", "Address3YearsOfTenure");
            Add("ITCThirdAddressLine1", "Address3Line1");
            Add("ITCThirdAddressLine2", "Address3Line2");
            Add("ITCThirdAddressLine3", "Address3Line3");
            Add("ITCThirdAddressLine4", "Address3Line4");
            Add("ITCThirdAddressPostCode", "Address3PostalCode");
            Add("ITCThirdAddressProvCode", "Address3Province");
            Add("ITCThirdAddressCountryCode", "Address3Country");
            Add("ITCThirdAddressOTInd", "Address3OwnershipStatus");
            Add("ITCFourthAddressUpdateDate", "Address4DataProviderLastUpdated");
            Add("ITCFourthAddressYearsOfTenure", "Address4YearsOfTenure");
            Add("ITCFourthAddressLine1", "Address4Line1");
            Add("ITCFourthAddressLine2", "Address4Line2");
            Add("ITCFourthAddressLine3", "Address4Line3");
            Add("ITCFourthAddressLine4", "Address4Line4");
            Add("ITCFourthAddressPostCode", "Address4PostalCode");
            Add("ITCFourthAddressProvCode", "Address4Province");
            Add("ITCFourthAddressCountryCode", "Address4Country");
            Add("ITCFourthAddressOTInd", "Address4OwnershipStatus");
            Add("Employer 1", "Employer1");
            Add("Emp 1 Update Date", "Employer1DataProviderLastUpdated");
            Add("Emp 1 Occupation", "Employer1Occupation");
            Add("Employer 2", "Employer2");
            Add("Emp 2 Update Date", "Employer2DataProviderLastUpdated");
            Add("Emp 2 Occupation", "Employer2Occupation");
            Add("Employer 3", "Employer3");
            Add("Emp 3 Update Date", "Employer3DataProviderLastUpdated");
            Add("Emp 3 Occupation", "Employer3Occupation");
            Add("Work Tel 1 Date", "WorkTel1DataProviderLastUpdated");
            Add("Work Tel 1 Code", "WorkTel1Code");
            Add("Work Tel 1 No", "WorkTel1No");
            Add("Work Tel 2 Date", "WorkTel2DataProviderLastUpdated");
            Add("Work Tel 2 Code", "WorkTel2Code");
            Add("Work Tel 2 No", "WorkTel2No");
            Add("Work Tel 3 Date", "WorkTel3DataProviderLastUpdated");
            Add("Work Tel 3 Code", "WorkTel3Code");
            Add("Work Tel 3 No", "WorkTel3No");
            Add("Home Tel 1 Date", "HomeTel1DataProviderLastUpdated");
            Add("Home Tel 1 Code", "HomeTel1Code");
            Add("Home Tel 1 No", "HomeTel1No");
            Add("Home Tel 2 Date", "HomeTel2DataProviderLastUpdated");
            Add("Home Tel 2 Code", "HomeTel2Code");
            Add("Home Tel 2 No", "HomeTel2No");
            Add("Home Tel 3 Date", "HomeTel3DataProviderLastUpdated");
            Add("Home Tel 3 Code", "HomeTel3Code");
            Add("Home Tel 3 No", "HomeTel3No");
            Add("Cell 1 Date", "Mobile1DataProviderLastUpdated");
            Add("Cell 1 No", "Mobile1No");
            Add("Cell 2 Date", "Mobile2DataProviderLastUpdated");
            Add("Cell 2 No", "Mobile2No");
            Add("Cell 3 Date", "Mobile3DataProviderLastUpdated");
            Add("Cell 3 No", "Mobile3No");
            Add("Email Add", "Email1");
        }
    }
}
