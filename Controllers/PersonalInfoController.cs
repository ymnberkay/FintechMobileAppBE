using Microsoft.AspNetCore.Mvc;
using TechMobileBE.Services;

namespace TechMobileBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonalInfoController : ControllerBase
    {
        private readonly PersonalInfoService _service;

        public PersonalInfoController(PersonalInfoService service)
        {
            _service = service;
        }

        [HttpPost("residence")]
        public async Task<IActionResult> Step1([FromBody] Step1ResidenceDto dto)
        {
            var id = await _service.CreateResidenceAsync(dto);
            if (id == null)
            {
                var errorResponse = new ApiResidenceResponse<object>(false, "Failed to create residence");
                return NotFound(errorResponse);
            }
            var response = new ApiResidenceResponse<object>(true, "Residence created successfully", id);
            return Ok(response);
        }

        [HttpPost("name")]
        public async Task<IActionResult> Step2([FromBody] Step2NameDto dto)
        {
            var success = await _service.UpdateStep2Async(dto);
            if (success)
            {
                var response = new ApiResponse<object>(true, "Name updated successfully");
                return Ok(response);
            }
            else
            {
                var errorResponse = new ApiResponse<object>(false, "Failed to update name");
                return NotFound(errorResponse);
            }
        }

        [HttpPost("adress")]
        public async Task<IActionResult> Step3([FromBody] Step3AddressDto dto)
        {
            var success = await _service.UpdateStep3Async(dto);
            if (success)
            {
                var response = new ApiResponse<object>(true, "Address updated successfully");
                return Ok(response);
            }
            var errorResponse = new ApiResponse<object>(false, "Failed to update address");
            return NotFound(errorResponse);
        }

        [HttpPost("email")]
        public async Task<IActionResult> Step4([FromBody] Step4EmailDto dto)
        {
            var success = await _service.UpdateStep4Async(dto);
            if (success)
            {
                var response = new ApiResponse<object>(true, "Email updated successfully");
                return Ok(response);
            }
            else
            {
                var errorResponse = new ApiResponse<object>(false, "Failed to update email");
                return NotFound(errorResponse);
            }
        }

        [HttpPost("passcode")]
        public async Task<IActionResult> Step5([FromBody] Step5PasscodeDto dto)
        {
            var success = await _service.UpdateStep5Async(dto);
            if (success)
            {
                var response = new ApiResponse<object>(true, "Passcode updated successfully");
                return Ok(response);
            }
            else
            {
                var errorResponse = new ApiResponse<object>(false, "Failed to update passcode");
                return NotFound(errorResponse);
            }
        }

        

        [HttpGet("{userId:length(24)}")]
        public async Task<IActionResult> GetPersonalInfo(string userId)
        {
            var personalInfo = await _service.GetPersonalInfoAsync(userId);
            if (personalInfo == null)
            {
                return NotFound(new ApiResponse<object>(false, "Personal info not found"));
            }
            var response = new ApiResidenceResponse<object>(true, "Personal info fetched success.", personalInfo);
            return Ok(response);
        }
        

        [HttpGet("by-email")]
        public async Task<IActionResult> GetPersonalInfoByEmail([FromQuery] string email)
        {
            var personalInfo = await _service.GetPersonalInfoByEmailAsync(email);
            if (personalInfo == null)
            {
                return NotFound(new ApiResponse<object>(false, "Personal info not found for this email"));
            }
            
            var response = new ApiResidenceResponse<object>(true, "Personal info fetched successfully by email.", personalInfo);
            return Ok(response);
        }
    }
}
