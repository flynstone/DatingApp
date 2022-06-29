namespace Api.Extensions
{
    public static class DateTimeExtensions
    {
        // Calculate user's age according to his date of birth.
        public static int CalculateAge(this DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
