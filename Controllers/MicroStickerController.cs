﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using image_bot.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
namespace image_bot.Controllers
{
    [Route("api/micro-sticker")]
    [ApiController]
    public class MicroStickerController : ControllerBase
    {
        public UsersState db;
        public Cloudinary cloudinary;
        public Account account;

        private const int _stickerWidth = 512;
        private const int _stickerHeight = 100;
        public MicroStickerController(UsersState context)
        {
            db = context;
            account = new Account(AppSettings.CloudName, AppSettings.CloudKey, AppSettings.CloudSecret);
            cloudinary = new Cloudinary(account);
        }

        [Route("create-request")]
        [HttpPost]
        public async Task<ActionResult> CreateRequest(long chatId)
        {
            BotUser botUser = db.BotUsers.Where(b => b.ChatId == chatId).First();
            botUser.CurentCommand = Models.BotCommand.CreateMicroStickers;
            db.BotUsers.Update(botUser);
            CreateMicroStickersRequest request = new CreateMicroStickersRequest() { UserId = botUser.Id };
            if (db.ImageResizeRequests.Any(i => i.UserId == botUser.Id))
            {
                return BadRequest();
            }
            db.CreateMicroStickersRequests.Add(request);
            await db.SaveChangesAsync();
            return Ok();
        }

        [Route("set-name")]
        [HttpPost]
        public async Task<IActionResult> SetName(long chatId, string name)
        {
            BotUser user = db.BotUsers.Where(b => b.ChatId == chatId).First();
            CreateMicroStickersRequest request = db.CreateMicroStickersRequests.Include(u => u.User).Where(u => u.UserId == user.Id).First();
            request.Name = name;
            request.Status = MicroStickersStatus.AwaitingSticker;
            db.CreateMicroStickersRequests.Update(request);
            await db.SaveChangesAsync();
            return Ok();
        }

        [Route("create-sticker")]
        [HttpGet]
        public async Task<IActionResult> CreateSticker(string url)
        {
            ImageUploadParams uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(url),
                Transformation = new Transformation().Width(_stickerWidth).Height(_stickerHeight).Crop("pad").Gravity("west")
            };
            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            return new OkObjectResult(uploadResult.Url);
        }


    }
}
