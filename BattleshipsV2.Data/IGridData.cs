using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipsV2.Data
{
    public interface IGridData
    {
        void SetElementsFromJson(GameDataFormat gameDataFormat);
        GameDataFormat FormatElements();
        List<Coord> GetPlayerCoords();
        List<Coord> GetAICoords();
        List<ShipType> GetValidShips();
        Dictionary<Coord, CellContent> GetPlayerGrid();
        Dictionary<Coord, CellContent> GetAIGrid();
        MoveResult FireAtCellOnAIGrid(string x, string y);
        MoveResult FireAtCellOnPlayerGrid(int x, int y);
        MoveResult PlaceShipOnPlayerGrid(ShipType type, string start, string end);
        void PlaceAllShipsRandomly(Dictionary<Coord, CellContent> grid);
    }
}
