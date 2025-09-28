using Godot;
using System;
using BoilerTronicsObjects.Layers;

namespace BoilerTronicsObjects.Placable
{
    public abstract partial class PlacableObject
    {
        // These are used to keep track of the index that these placabel objects are in our map
        Vector2 OGTilePos;
        Vector2 CurrTilePos;

        public void moveObject(int newX, int newY)
        {
            // changes the original posistion of the object
            OGTilePos.X = newX;
            OGTilePos.Y = newY;
        }

        public void setPosistion()
        {

        }
    }
}