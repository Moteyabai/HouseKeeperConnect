﻿using Appwrite;
using Appwrite.Models;
using Appwrite.Services;
using AutoMapper;
using BusinessObject.DTO;
using BusinessObject.Models;
using BusinessObject.Models.AppWrite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace HouseKeeperConnect_API.Controllers
{
    [Route("api/[controller]")]
    public class IDVerificationsController : ControllerBase
    {
        private readonly IIDVerificationService _idVerificationService;
        private readonly IHouseKeeperService _houseKeeperService;
        private readonly IVerificationTaskService _verificationTaskService;
        private readonly IConfiguration _configuration;
        private readonly Client _appWriteClient;
        private readonly IMapper _mapper;

        public IDVerificationsController(IIDVerificationService idVerificationService, IHouseKeeperService houseKeeperService, IVerificationTaskService verificationTaskService, IMapper mapper, IConfiguration configuration)
        {
            _idVerificationService = idVerificationService;
            _houseKeeperService = houseKeeperService;
            _verificationTaskService = verificationTaskService;
            _mapper = mapper;
            _configuration = configuration;
            AppwriteSettings appW = new AppwriteSettings()
            {
                ProjectId = configuration.GetValue<string>("Appwrite:ProjectId"),
                Endpoint = configuration.GetValue<string>("Appwrite:Endpoint"),
                ApiKey = configuration.GetValue<string>("Appwrite:ApiKey")
            };
            _appWriteClient = new Client().SetProject(appW.ProjectId).SetEndpoint(appW.Endpoint).SetKey(appW.ApiKey);
        }

        [HttpGet("GetAllIDVerifications")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<IDVerificationDisplayDTO>>> GetAllIDVerifications(int pageNumber, int pageSize)
        {
            try
            {
                var idVerifications = await _idVerificationService.GetAllIDVerifysAsync(pageNumber, pageSize);
                if (idVerifications.Count == 0)
                {
                    return NotFound("No ID verifications found!");
                }

                var idVerificationDTOs = _mapper.Map<List<IDVerificationDisplayDTO>>(idVerifications);
                return Ok(idVerificationDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetIDVerificationByID")]
        [Authorize]
        public async Task<ActionResult<IDVerificationDisplayDTO>> GetIDVerificationByID([FromQuery] int id)
        {
            var idVerification = await _idVerificationService.GetIDVerifyByIDAsync(id);
            if (idVerification == null)
            {
                return NotFound("ID Verification not found!");
            }

            var idVerificationDTO = _mapper.Map<IDVerificationDisplayDTO>(idVerification);
            return Ok(idVerificationDTO);
        }

        [HttpPost("CreateIDVerification")]
        public async Task<ActionResult> CreateIDVerification([FromForm] IDVerificationCreateDTO idVerificationDTO, [FromQuery] int housekeeperId)
        {
            if (idVerificationDTO == null)
            {
                return BadRequest("Invalid data.");
            }

            if (idVerificationDTO.FrontPhoto == null || idVerificationDTO.FrontPhoto.Length == 0)
                throw new ArgumentException("No file uploaded.");
            if (idVerificationDTO.FacePhoto == null || idVerificationDTO.FacePhoto.Length == 0)
                throw new ArgumentException("No file uploaded.");
            if (idVerificationDTO.BackPhoto == null || idVerificationDTO.BackPhoto.Length == 0)
                throw new ArgumentException("No file uploaded.");

            var housekeeper = await _houseKeeperService.GetHousekeeperByIDAsync(housekeeperId);
            if (housekeeper == null)
            {
                return NotFound("Housekeeper not found.");
            }
            if (housekeeper.VerifyID.HasValue)
            {
                return BadRequest("This Housekeeper already has an ID Verification.");
            }

            var storage = new Storage(_appWriteClient);
            var buckID = "67e3d029000d5b9dd68e";
            var projectID = _configuration.GetValue<string>("Appwrite:ProjectId");

            List<string> perms = new List<string>() { Permission.Write(Appwrite.Role.Any()), Permission.Read(Appwrite.Role.Any()) };
            //Front Picture

            var idFr = Guid.NewGuid().ToString();
            var front = InputFile.FromStream(
                idVerificationDTO.FrontPhoto.OpenReadStream(),
                idVerificationDTO.FrontPhoto.FileName,
                idVerificationDTO.FrontPhoto.ContentType
                );
            var response = await storage.CreateFile(
                        buckID,
                        idFr,
                        front,
                        perms,
                        null
                        );

            var frontID = response.Id;
            var frontUrl = $"{_appWriteClient.Endpoint}/storage/buckets/{response.BucketId}/files/{frontID}/view?project={projectID}";

            //Back picture
            var idBk = Guid.NewGuid().ToString();
            var back = InputFile.FromStream(
            idVerificationDTO.BackPhoto.OpenReadStream(),
            idVerificationDTO.BackPhoto.FileName,
            idVerificationDTO.BackPhoto.ContentType
            );
            var response2 = await storage.CreateFile(
                buckID,
                idBk,
                back,
                perms,
                null
                );
            var backID = response2.Id;
            var backUrl = $"{_appWriteClient.Endpoint}/storage/buckets/{response2.BucketId}/files/{backID}/view?project={projectID}";

            //Face picture
            var idFc = Guid.NewGuid().ToString();
            var face = InputFile.FromStream(
        idVerificationDTO.FacePhoto.OpenReadStream(),
        idVerificationDTO.FacePhoto.FileName,
        idVerificationDTO.FacePhoto.ContentType
        );
            var response3 = await storage.CreateFile(
                buckID,
                idFc,
                face,
                perms,
                null
                );
            var faceID = response3.Id;
            var faceUrl = $"{_appWriteClient.Endpoint}/storage/buckets/{response3.BucketId}/files/{faceID}/view?project={projectID}";

            var idVerification = new IDVerification
            {
                FrontPhoto = frontUrl,
                BackPhoto = backUrl,
                FacePhoto = faceUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = 1 // Pending
            };

            // Lưu IDVerification vào database trước
            int verifyId = await _idVerificationService.AddIDVerifyAsync(idVerification);

            if (verifyId == 0)
            {
                throw new Exception("Failed to create IDVerification.");
            }

            // Cập nhật VerifyID vào Housekeeper
            housekeeper.VerifyID = verifyId;
            await _houseKeeperService.UpdateHousekeeperAsync(housekeeper);

            // Tạo VerificationTask
            var verificationTask = new VerificationTask
            {
                VerifyID = verifyId,
                AssignedDate = DateTime.Now,
                Status = 1 // Pending
            };

            await _verificationTaskService.CreateVerificationTaskAsync(verificationTask);

            return Ok(new { Message = "ID Verification created successfully!", VerifyID = verifyId });
        }

        private async Task<byte[]> ConvertToByteArrayAsync(IFormFile file)
        {
            if (file == null) return null;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        [HttpPut("UpdateIDVerification")]
        public async Task<ActionResult> UpdateIDVerification([FromForm] IDVerificationUpdateDTO idVerificationDTO)
        {
            try
            {
                /*var existingVerification = await _idVerificationService.GetIDVerifyByIDAsync(idVerificationDTO.VerifyID);
                if (existingVerification == null)
                {
                    return NotFound("ID Verification not found!");
                }

                if (idVerificationDTO.FrontPhoto != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await idVerificationDTO.FrontPhoto.CopyToAsync(memoryStream);
                        existingVerification.FrontPhoto = memoryStream.ToArray();
                    }
                }

                if (idVerificationDTO.BackPhoto != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await idVerificationDTO.BackPhoto.CopyToAsync(memoryStream);
                        existingVerification.BackPhoto = memoryStream.ToArray();
                    }
                }

                if (idVerificationDTO.FacePhoto != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await idVerificationDTO.FacePhoto.CopyToAsync(memoryStream);
                        existingVerification.FacePhoto = memoryStream.ToArray();
                    }
                }

                existingVerification.IDNumber = idVerificationDTO.IDNumber;
                existingVerification.RealName = idVerificationDTO.RealName;
                existingVerification.DateOfBirth = idVerificationDTO.DateOfBirth;
                existingVerification.UpdatedAt = DateTime.UtcNow;

                await _idVerificationService.UpdateIDVerifyAsync(existingVerification);*/
                return Ok("ID Verification updated successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}