using BattleshipsV2.Data;
using BattleshipsV2.Models;
using BattleshipsV2.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BattleshipsV2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IJsonData _jsonData;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IJsonData jsonData, ILogger<HomeController> logger)
        {
            _jsonData = jsonData;
            _logger = logger;
        }

        #region Page Interaction
        public IActionResult Index()
        {
            HomeScreenViewModel viewModel = new HomeScreenViewModel();

            viewModel.IsActive = _jsonData.GetIsActive();

            return View(viewModel);
        }

        public IActionResult Game()
        {
            if (GridHandler.PlayerGrid == null) return RedirectToAction("Index", "Home");
            else
            {
                GameScreenViewModel viewModel = new GameScreenViewModel();
                viewModel.PlayerGrid = GridHandler.PlayerGrid.GetCells();
                viewModel.AIGrid = GridHandler.AIGrid.GetCells();
                viewModel.GameStage = GridHandler.GameStage;
                viewModel.PrimaryMessage = GridHandler.PrimaryMessage;
                viewModel.TrackingMessage = GridHandler.TrackingMessage;
                viewModel.ValidShips = GridHandler.ValidPlayerShips;
                (string player, string ai) hitPoints = GridHandler.GetHitPoints();
                viewModel.PlayerHitPoints = hitPoints.player;
                viewModel.AIHitPoints = hitPoints.ai;
                viewModel.Turn = GridHandler.GetTurn();

                if (GridHandler.ValidPlayerShips.Count == 0) GridHandler.GameStage = 2;

                // If player or AI has won, reset json data
                if (GridHandler.GameStage == 2) _jsonData.Commit();
                else _jsonData.ResetAllData();

                return View(viewModel);
            }
        }
        #endregion

        #region Data Interaction
        public IActionResult HandleGameStart()
        {
            _jsonData.ResetAllData();
            GridHandler.HandleGameStart();
            return RedirectToAction("Game", "Home");
        }

        public IActionResult HandleContinue()
        {
            GridHandler.HandleGameStart();
            GridHandler.SetAllGridCells(_jsonData.GetPlayerGridContent(), _jsonData.GetAIGridContent());
            GridHandler.UpdateDetails(_jsonData.GetNumberOfTurns(), _jsonData.GetGameStage());

            return RedirectToAction("Game", "Home");
        }

        public IActionResult HandleReturnToIndex()
        {
            _jsonData.ResetAllData();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Stage1PlaceShip(ShipType shipType, string pairA, string pairB)
        {
            if (pairA == null || pairA.Length < 2 || (pairA.Length > 2 && pairA.Substring(1) != "10")
                || pairB == null || pairB.Length < 2 || (pairB.Length > 2 && pairB.Substring(1) != "10"))
            {
                GridHandler.TrackingMessage = "Please enter valid coordinates.";
            }
            else
            {
                string pairAcoordA = pairA[0].ToString();
                string pairAcoordB;
                if (pairA.Length > 2) pairAcoordB = "10";
                else pairAcoordB = pairA[1].ToString();

                string pairBcoordA = pairB[0].ToString();
                string pairBcoordB;
                if (pairB.Length > 2) pairBcoordB = "10";
                else pairBcoordB = pairB[1].ToString();

                MoveResult result = GridHandler.PlacePlayerShip(shipType, pairAcoordA, pairAcoordB, pairBcoordA, pairBcoordB);

                if (result == MoveResult.Success)
                {
                    GridHandler.TrackingMessage = "";
                }
                else if (result == MoveResult.SpaceIsOccupied)
                {
                    GridHandler.TrackingMessage = "Please choose an unoccupied space.";
                }
                else
                {
                    GridHandler.TrackingMessage = "Please enter valid coordinates.";
                }
            }

            return RedirectToAction("Game", "Home");
        }

        public IActionResult Stage1PlaceRandomShips()
        {
            GridHandler.PlaceRandomPlayerShips();
            return RedirectToAction("Game", "Home");
        }

        public IActionResult Stage2PlayerMove(string coords)
        {
            if (GridHandler.GameStage < 2)
            {
                GridHandler.PrimaryMessage = "Please place your ships before firing.";
            }
            else if (coords == null || coords.Length < 2 || (coords.Length > 2 && coords.Substring(1) != "10"))
            {
                GridHandler.PrimaryMessage = "Please make sure your grid reference is formatted correctly (e.g. A7)";
            }
            else
            {
                string coordA = coords[0].ToString();
                string coordB;
                if (coords.Length > 2) coordB = "10";
                else coordB = coords[1].ToString();
                MoveResult result = GridHandler.AttemptPlayerMove(coordA, coordB);

                if (result == MoveResult.Success)
                {
                    GridHandler.PrimaryMessage = "";
                }
                else if (result == MoveResult.InvalidCoords)
                {
                    GridHandler.PrimaryMessage = "Please enter valid coordinates.";
                }
                else
                {
                    GridHandler.PrimaryMessage = "You've already fired here! Please enter new coordinates.";
                }
            }

            return RedirectToAction("Game", "Home");
        }
        #endregion

        #region Boilerplate Methods
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion
    }
}
