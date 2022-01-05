﻿using GuessNumber.Data;
using GuessNumber.Service;
using GuessNumber.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuessNumber.Controllers
{
    public class GameResultsController : Controller
    {

        private readonly AuthDbContext _context;
        private readonly IUserService userService;

        public GameResultsController(AuthDbContext context, IUserService userService)
        {
            _context = context;
            this.userService = userService;

        }
        public async Task<IActionResult> Index()
        {
            return View(await GetTopScores());
        }

        private async Task<List<LeaderBoardViewModel>> GetTopScores()
        {
            var topUsers = await _context.GameResult
                .Include(u => u.Player)
                .GroupBy(g => g.PlayerId)
                .Select(a => new { TotalScore = a.Sum(b => b.GamePoint), Name = a.Select(s => s.Player.FirstName).FirstOrDefault() })
                .OrderByDescending(a => a.TotalScore)
                .AsNoTracking()
                .Take(10)
                .ToListAsync();

            string playerId = userService.GetUserId();
            var userScore = await _context.GameResult
                 .Include(u => u.Player)
                 .Where(w => w.PlayerId == playerId)
                 .GroupBy(g => g.PlayerId)
                 .Select(a => new { TotalScore = a.Sum(b => b.GamePoint), Name = a.Select(s => s.Player.FirstName).FirstOrDefault() })
                 .OrderByDescending(a => a.TotalScore)
                 .AsNoTracking()
                 .Take(1)
                 .ToListAsync();

            foreach (var item in userScore)
            {
                ViewBag.Name = item.Name;
                ViewBag.TotalScore = item.TotalScore;
            }


            List<LeaderBoardViewModel> vm = new List<LeaderBoardViewModel>();

            foreach (var item in topUsers)
            {
                LeaderBoardViewModel lm = new LeaderBoardViewModel();
                lm.PlayerName = item.Name ?? "Unknown";
                lm.TotalPoint = item.TotalScore;
                vm.Add(lm);
            }
            return vm;
        }
    }
}
