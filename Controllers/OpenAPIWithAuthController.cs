using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Globalization;

[ApiController]
[Route("api/[controller]")]
public class ApiController : ControllerBase
{
    [HttpPost]
    public IActionResult Post(RequestDto request)
    {
        try
        {
            // Check if the input is null or white space
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                return BadRequest("Id is required.");
            }

            // Check if the input contains any special characters as well as paces between the letters
            if (!Regex.IsMatch(request.Id, "^[a-zA-Z0-9]+$"))
            {
                return BadRequest("Id must not contain special characters. and spaces & negative values");
            }

            // Check if the input is alphanumeric and contains both letters and numbers
            if (!request.Id.Any(char.IsLetter) || !request.Id.Any(char.IsDigit))
            {
                return BadRequest("Id must contain both letters and numbers.");
            }

        
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name cannot be null or empty. ");

            if (!Regex.IsMatch(request.Name, "^[a-zA-Z]+$"))
            {
                return BadRequest("Name must contain only alphabetic characters (no spaces, numbers, or special characters).");
            }


            if (request.AccountNumber == null || request.AccountNumber <= 0)
            {
                return BadRequest("Invalid AccountNumber. It must be a positive integer.");
            }
            // Check if the account number contains only digits (0-9) and is a valid integer
            if (!Regex.IsMatch(request.AccountNumber.ToString(), @"^\d+$"))
            {
                return BadRequest("Account number must contain only numeric digits (no special characters, letters, or spaces).");
            }

            // Get the current date and time in server's local time zone
            DateTime localTime = DateTime.Now; // Use DateTime.Now for the current local timing

            // Format the date and time to "dd/MM/yy hh:mm:ss tt" (12-hour format with AM/PM)
            var formattedRequestTime = localTime.ToString("dd/MM/yy hh:mm:ss tt", CultureInfo.InvariantCulture);

            return Ok(new
            {
                // Return the formatted date and time in the desired format
                RequestTime = formattedRequestTime,
                Status = "Success"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    private bool IsAlphanumeric(string str) =>
        Regex.IsMatch(str, "^[a-zA-Z0-9]+$");
}
