﻿using BookingApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Services.Interfaces
{
    public interface INotificationService
    {
        Task ForgetPasswordMail(ApplicationUser user);
    }
}
