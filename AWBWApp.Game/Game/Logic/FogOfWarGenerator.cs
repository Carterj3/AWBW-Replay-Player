﻿using System;
using System.Collections.Generic;
using AWBWApp.Game.Game.Building;
using AWBWApp.Game.Game.Unit;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Primitives;

namespace AWBWApp.Game.Game.Logic
{
    public class FogOfWarGenerator
    {
        public Bindable<bool[,]> FogOfWar;

        private GameMap gameMap;

        public FogOfWarGenerator(GameMap map)
        {
            gameMap = map;
            FogOfWar = new Bindable<bool[,]>
            {
                Value = new bool[gameMap.MapSize.X, gameMap.MapSize.Y]
            };
        }

        public void ClearFog(bool makeFoggy, bool triggerChange)
        {
            var fogArray = FogOfWar.Value;

            if (!makeFoggy)
            {
                for (int x = 0; x < gameMap.MapSize.X; x++)
                {
                    for (int y = 0; y < gameMap.MapSize.Y; y++)
                        fogArray[x, y] = true;
                }
            }
            else
                Array.Clear(fogArray, 0, fogArray.Length);

            if (triggerChange)
                FogOfWar.TriggerChange();
        }

        public void GenerateFogForPlayer(int player, int rangeIncrease, bool canSeeIntoHiddenTiles, bool resetFog = true) => generateFog(gameMap.GetDrawableBuildingsForPlayer(player), gameMap.GetDrawableUnitsFromPlayer(player), rangeIncrease, canSeeIntoHiddenTiles, resetFog);

        private void generateFog(IEnumerable<DrawableBuilding> buildings, IEnumerable<DrawableUnit> units, int rangeIncrease, bool canSeeIntoHiddenTiles, bool resetFog = true)
        {
            var fogArray = FogOfWar.Value;

            if (resetFog)
                Array.Clear(fogArray, 0, fogArray.Length);

            //All the buildings the player owns shows its own tile.
            foreach (var drawableBuilding in buildings)
                fogArray[drawableBuilding.MapPosition.X, drawableBuilding.MapPosition.Y] = true;

            foreach (var drawableUnit in units)
            {
                var visionRange = Math.Max(1, drawableUnit.UnitData.Vision + rangeIncrease);

                for (int x = -visionRange; x <= visionRange; x++)
                {
                    for (int y = -visionRange; y <= visionRange; y++)
                    {
                        var tilePosition = drawableUnit.MapPosition + new Vector2I(x, y);
                        if (tilePosition.X < 0 || tilePosition.X >= gameMap.MapSize.X || tilePosition.Y < 0 || tilePosition.Y >= gameMap.MapSize.Y)
                            continue;

                        var distance = Math.Abs(x) + Math.Abs(y);
                        if (distance > visionRange)
                            continue;

                        if (!canSeeIntoHiddenTiles)
                        {
                            if (gameMap.TryGetDrawableBuilding(tilePosition, out DrawableBuilding building))
                            {
                                if (building.BuildingTile.LimitFogOfWarSightDistance > 0 && distance > building.BuildingTile.LimitFogOfWarSightDistance)
                                    continue;
                            }
                            else
                            {
                                var tile = gameMap.GetDrawableTile(tilePosition);
                                if (tile.TerrainTile.LimitFogOfWarSightDistance > 0 && distance > tile.TerrainTile.LimitFogOfWarSightDistance)
                                    continue;
                            }
                        }

                        fogArray[tilePosition.X, tilePosition.Y] = true;
                    }
                }
            }

            FogOfWar.TriggerChange();
        }
    }
}
