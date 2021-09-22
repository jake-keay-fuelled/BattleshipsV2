using BattleshipsV2.Models;
using BattleshipsV2.ViewModels;
using BattleshipsV2.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BattleshipsV2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IJsonData _jsonData;
        private readonly IGridData _gridData;
        private readonly IGameHandler _gameHandler;

        public HomeController(IJsonData jsonData, IGridData gridData, IGameHandler gameHandler)
        {
            _jsonData = jsonData;
            _gridData = gridData;
            _gameHandler = gameHandler;

            _gridData.SetElementsFromJson(_jsonData.Retrieve());
            if (!_gameHandler.CheckIsActive()) _gameHandler.Initialise(_gridData);
        }

        public IActionResult Index()
        {
            return View(PopulateViewModel(null, null));
        }

        public IActionResult Win()
        {
            return View();
        }

        public IActionResult Lose()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PlaceShip(ShipType shipType, string coordA, string coordB)
        {
            MoveResult result = _gridData.PlaceShipOnPlayerGrid(shipType, coordA, coordB);

            if (result == MoveResult.InvalidCoords) return View("Index", PopulateViewModel("Please enter valid coordinates.", null));
            else if (result == MoveResult.InvalidSize) return View("Index", PopulateViewModel("Please enter coordinates matching ship size.", null));
            else if (result == MoveResult.SpaceIsOccupied) return View("Index", PopulateViewModel("This space is occupied! Please enter different coordinates", null));

            _jsonData.Commit(_gridData.FormatElements());
            return RedirectToAction("Index", "Home");
        }

        public IActionResult PlaceRandomPlayerShips()
        {
            _gridData.PlaceAllShipsRandomly(_gridData.GetPlayerGrid());
            _jsonData.Commit(_gridData.FormatElements());
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Fire(string coord)
        {
            MoveResult result = _gameHandler.HandlePlayerMove(coord, _gridData);

            if (result == MoveResult.LastAIHitPointTaken) return RedirectToAction("Win", "Home");
            else if (result == MoveResult.LastPlayerHitPointTaken) return RedirectToAction("Lose", "Home");
            else if (result == MoveResult.SpaceIsOccupied) return View("Index", PopulateViewModel(null, "Please fire at an unoccupied space."));
            else if (result == MoveResult.InvalidCoords) return View("Index", PopulateViewModel(null, "Please enter valid coordinates."));

            _jsonData.Commit(_gridData.FormatElements());
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Reset()
        {
            _jsonData.Reset();
            return RedirectToAction("Index", "Home");
        }

        private GridViewModel PopulateViewModel(string playerMessage, string aiMessage)
        {
            GridViewModel viewModel = new GridViewModel();
            viewModel.PlayerGrid = new CellContent[10, 10];
            viewModel.AIGrid = new CellContent[10, 10];
            viewModel.ValidShips = _gridData.GetValidShips();
            viewModel.PlayerMessage = playerMessage;
            viewModel.AIMessage = aiMessage;

            foreach (Coord C in _gridData.GetPlayerCoords())
            {
                viewModel.PlayerGrid[C.X, C.Y] = _gridData.GetPlayerGrid()[C];
            }
            foreach (Coord C in _gridData.GetAICoords())
            {
                viewModel.AIGrid[C.X, C.Y] = _gridData.GetAIGrid()[C];
            }

            return viewModel;
        }

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

        #endregion Boilerplate Methods
    }
}